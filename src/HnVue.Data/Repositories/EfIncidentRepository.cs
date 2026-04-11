using HnVue.Common.Results;
using HnVue.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Repositories;

/// <summary>
/// EF Core repository for radiation safety incident records in the HnVue.Data layer.
/// All write operations execute within a transaction for safety-critical data integrity (IEC 62304).
/// For DI registration, use <c>HnVue.Incident.EfIncidentRepository</c> which implements <c>IIncidentRepository</c>.
/// REQ-COORD-003: SPEC-COORDINATOR-001 EF Core incident persistence.
/// </summary>
public sealed class EfIncidentRepository(HnVueDbContext context)
{
    /// <summary>Persists an incident record within a transaction.</summary>
    public async Task<Result> SaveAsync(IncidentEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

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

    /// <summary>Returns all incident records with the specified severity value.</summary>
    public async Task<Result<IReadOnlyList<IncidentEntity>>> GetBySeverityAsync(
        int severityValue, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await context.Incidents
                .AsNoTracking()
                .Where(i => i.SeverityValue == severityValue)
                .OrderByDescending(i => i.OccurredAtTicks)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            IReadOnlyList<IncidentEntity> result = entities;
            return Result.Success(result);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<IReadOnlyList<IncidentEntity>>(ErrorCode.DatabaseError, ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <summary>Marks an incident as resolved within a transaction.</summary>
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

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure(ErrorCode.DatabaseError, ex.InnerException?.Message ?? ex.Message);
        }
    }
}
