using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data.Mappers;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IAuditRepository"/> backed by the encrypted SQLite database.
/// Hash chain computation is performed by the Security layer; this repository only persists and retrieves entries.
/// </summary>
internal sealed class AuditRepository(HnVueDbContext context) : IAuditRepository
{
    /// <inheritdoc/>
    public async Task<Result> AppendAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = EntityMapper.ToEntity(entry);
            context.AuditLogs.Add(entity);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure(
                ErrorCode.DatabaseError,
                ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<string?>> GetLastHashAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var lastEntry = await context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.TimestampTicks)
                .ThenByDescending(a => a.EntryId)
                .Select(a => a.CurrentHash)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            return lastEntry is null
                ? Result.SuccessNullable<string?>(null)
                : Result.Success<string?>(lastEntry);
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure<string?>(
                ErrorCode.DatabaseError,
                ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<AuditEntry>>> QueryAsync(AuditQueryFilter filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = context.AuditLogs.AsNoTracking();

            if (filter.UserId is not null)
                query = query.Where(a => a.UserId == filter.UserId);

            if (filter.FromDate.HasValue)
            {
                var fromTicks = filter.FromDate.Value.UtcTicks;
                query = query.Where(a => a.TimestampTicks >= fromTicks);
            }

            if (filter.ToDate.HasValue)
            {
                var toTicks = filter.ToDate.Value.UtcTicks;
                query = query.Where(a => a.TimestampTicks <= toTicks);
            }

            var entities = await query
                .OrderBy(a => a.TimestampTicks)
                .Take(filter.MaxResults)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            IReadOnlyList<AuditEntry> records = entities
                .Select(EntityMapper.ToRecord)
                .ToList();

            return Result.Success(records);
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure<IReadOnlyList<AuditEntry>>(
                ErrorCode.DatabaseError,
                ex.InnerException?.Message ?? ex.Message);
        }
    }
}
