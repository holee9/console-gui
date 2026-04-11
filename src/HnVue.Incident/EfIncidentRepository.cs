using HnVue.Common.Enums;
using HnVue.Common.Results;
using HnVue.Data;
using HnVue.Incident.Models;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Incident;

/// <summary>
/// EF Core implementation of <see cref="IIncidentRepository"/>.
/// Persists and queries incident records for IEC 62304 traceability.
/// All write operations execute within a transaction for safety-critical data integrity.
/// </summary>
public sealed class EfIncidentRepository(HnVueDbContext context) : IIncidentRepository
{
    /// <inheritdoc/>
    public async Task<Result> SaveAsync(IncidentRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        try
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            var entity = new Data.Entities.IncidentEntity
            {
                IncidentId = record.IncidentId,
                OccurredAtTicks = record.OccurredAt.UtcTicks,
                OccurredAtOffsetMinutes = (int)record.OccurredAt.Offset.TotalMinutes,
                ReportedByUserId = record.ReportedByUserId,
                SeverityValue = (int)record.Severity,
                Category = record.Category,
                Description = record.Description,
                Resolution = record.Resolution,
                IsResolved = record.IsResolved,
                ResolvedAtTicks = record.ResolvedAt?.UtcTicks,
                ResolvedAtOffsetMinutes = record.ResolvedAt.HasValue
                    ? (int)record.ResolvedAt.Value.Offset.TotalMinutes
                    : null,
                ResolvedByUserId = record.ResolvedByUserId,
            };

            context.Incidents.Add(entity);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure(ErrorCode.DatabaseError, ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<IncidentRecord>>> GetBySeverityAsync(
        IncidentSeverity severity, CancellationToken cancellationToken = default)
    {
        try
        {
            var severityValue = (int)severity;
            var entities = await context.Incidents
                .AsNoTracking()
                .Where(i => i.SeverityValue == severityValue)
                .OrderByDescending(i => i.OccurredAtTicks)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            IReadOnlyList<IncidentRecord> records = entities.Select(ToRecord).ToList();
            return Result.Success(records);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<IReadOnlyList<IncidentRecord>>(ErrorCode.DatabaseError, ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result> ResolveAsync(string incidentId, string resolution, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(incidentId);
        ArgumentNullException.ThrowIfNull(resolution);

        try
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            var entity = await context.Incidents
                .FirstOrDefaultAsync(i => i.IncidentId == incidentId, cancellationToken)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.Failure(ErrorCode.NotFound, $"Incident '{incidentId}' not found.");

            if (entity.IsResolved)
                return Result.Failure(ErrorCode.ValidationFailed, $"Incident '{incidentId}' is already resolved.");

            var now = DateTimeOffset.UtcNow;
            entity.Resolution = resolution;
            entity.IsResolved = true;
            entity.ResolvedAtTicks = now.UtcTicks;
            entity.ResolvedAtOffsetMinutes = (int)now.Offset.TotalMinutes;
            entity.ResolvedByUserId = null; // Set by caller through service layer

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure(ErrorCode.DatabaseError, ex.InnerException?.Message ?? ex.Message);
        }
    }

    private static IncidentRecord ToRecord(Data.Entities.IncidentEntity entity) =>
        new(
            IncidentId: entity.IncidentId,
            OccurredAt: new DateTimeOffset(entity.OccurredAtTicks, TimeSpan.FromMinutes(entity.OccurredAtOffsetMinutes)),
            ReportedByUserId: entity.ReportedByUserId,
            Severity: (IncidentSeverity)entity.SeverityValue,
            Category: entity.Category,
            Description: entity.Description,
            Resolution: entity.Resolution,
            IsResolved: entity.IsResolved,
            ResolvedAt: entity.ResolvedAtTicks.HasValue
                ? new DateTimeOffset(entity.ResolvedAtTicks.Value, TimeSpan.FromMinutes(entity.ResolvedAtOffsetMinutes ?? 0))
                : null,
            ResolvedByUserId: entity.ResolvedByUserId);
}
