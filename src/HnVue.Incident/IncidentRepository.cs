using System.Collections.Concurrent;
using HnVue.Common.Enums;
using HnVue.Common.Results;
using HnVue.Incident.Models;

namespace HnVue.Incident;

// @MX:TODO IncidentRepository needs database persistence (deferred to Wave 4 per README)
// @MX:NOTE Thread-safe via ConcurrentDictionary - no additional synchronization needed
/// <summary>
/// In-memory repository for <see cref="IncidentRecord"/> instances.
/// Thread-safe via <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// Wave 2 implementation: database persistence is deferred to Wave 4.
/// </summary>
internal sealed class IncidentRepository
{
    private readonly ConcurrentDictionary<string, IncidentRecord> _store = new();

    /// <summary>Adds a new incident record to the store.</summary>
    /// <param name="record">The record to add.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The added record on success, or a failure if an entry with the same ID already exists.</returns>
    public Task<Result<IncidentRecord>> AddAsync(IncidentRecord record, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(record);

        if (_store.TryAdd(record.IncidentId, record))
            return Task.FromResult(Result.Success(record));

        return Task.FromResult(
            Result.Failure<IncidentRecord>(
                ErrorCode.AlreadyExists,
                $"Incident '{record.IncidentId}' already exists in the store."));
    }

    /// <summary>Retrieves an incident by its unique identifier.</summary>
    /// <param name="id">The incident identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The matching record, or <see cref="ErrorCode.NotFound"/> failure.</returns>
    public Task<Result<IncidentRecord>> GetByIdAsync(string id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        return _store.TryGetValue(id, out var record)
            ? Task.FromResult(Result.Success(record))
            : Task.FromResult(Result.Failure<IncidentRecord>(ErrorCode.NotFound, $"Incident '{id}' was not found."));
    }

    // @MX:ANCHOR QueryAsync - @MX:REASON: Used by incident list UI and reporting dashboards
    /// <summary>
    /// Queries incidents with optional severity and date-range filters.
    /// </summary>
    /// <param name="severity">When non-null, restricts results to that severity level.</param>
    /// <param name="from">Inclusive lower bound on <see cref="IncidentRecord.OccurredAt"/>.</param>
    /// <param name="to">Inclusive upper bound on <see cref="IncidentRecord.OccurredAt"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Filtered list of incident records ordered by occurrence time.</returns>
    public Task<Result<IReadOnlyList<IncidentRecord>>> QueryAsync(
        IncidentSeverity? severity,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        IEnumerable<IncidentRecord> query = _store.Values;

        if (severity.HasValue)
            query = query.Where(r => r.Severity == severity.Value);

        if (from.HasValue)
            query = query.Where(r => r.OccurredAt >= from.Value);

        if (to.HasValue)
            query = query.Where(r => r.OccurredAt <= to.Value);

        IReadOnlyList<IncidentRecord> result = query
            .OrderBy(r => r.OccurredAt)
            .ToList()
            .AsReadOnly();

        return Task.FromResult(Result.Success(result));
    }

    /// <summary>
    /// Replaces an existing incident record in the store (optimistic, last-write-wins).
    /// </summary>
    /// <param name="record">The updated record. Must have the same <see cref="IncidentRecord.IncidentId"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated record on success, or <see cref="ErrorCode.NotFound"/> if the ID is unknown.</returns>
    public Task<Result<IncidentRecord>> UpdateAsync(IncidentRecord record, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(record);

        if (!_store.ContainsKey(record.IncidentId))
        {
            return Task.FromResult(
                Result.Failure<IncidentRecord>(
                    ErrorCode.NotFound,
                    $"Cannot update: incident '{record.IncidentId}' was not found."));
        }

        _store[record.IncidentId] = record;
        return Task.FromResult(Result.Success(record));
    }
}
