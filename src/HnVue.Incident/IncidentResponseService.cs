using HnVue.Common.Enums;
using HnVue.Common.Results;
using HnVue.Incident.Models;

namespace HnVue.Incident;

/// <summary>
/// Implements incident response logging and escalation for the HnVue X-ray console.
/// </summary>
/// <remarks>
/// Incident records are append-only — they cannot be deleted or modified after creation.
/// Critical incidents trigger emergency callbacks immediately.
/// IEC 62304 §5.8: incident investigation and anomaly management.
/// </remarks>
public sealed class IncidentResponseService
{
    private readonly IIncidentRepository _repository;
    private readonly List<Func<IncidentRecord, Task>> _criticalCallbacks = new();

    /// <summary>
    /// Initialises a new <see cref="IncidentResponseService"/>.
    /// </summary>
    public IncidentResponseService(IIncidentRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    // @MX:ANCHOR RecordAsync - @MX:REASON: Safety-critical incident entry point, triggers Critical escalation immediately, append-only audit trail per IEC 62304 §5.8
    /// <summary>
    /// Records an incident and triggers escalation for Critical severity incidents.
    /// </summary>
    /// <param name="severity">Severity level of the incident.</param>
    /// <param name="category">Short category tag (e.g., "DOSE", "NETWORK", "HARDWARE").</param>
    /// <param name="description">Human-readable description of the incident.</param>
    /// <param name="reportedByUserId">Identifier of the user or system component reporting the incident.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<Result<IncidentRecord>> RecordAsync(
        IncidentSeverity severity,
        string category,
        string description,
        string reportedByUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(category);
        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(reportedByUserId);

        if (string.IsNullOrWhiteSpace(category))
            return Result.Failure<IncidentRecord>(ErrorCode.ValidationFailed, "Category is required.");

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<IncidentRecord>(ErrorCode.ValidationFailed, "Description is required.");

        var record = new IncidentRecord(
            IncidentId: Guid.NewGuid().ToString(),
            OccurredAt: DateTimeOffset.UtcNow,
            ReportedByUserId: reportedByUserId.Trim(),
            Severity: severity,
            Category: category.Trim(),
            Description: description.Trim(),
            Resolution: null,
            IsResolved: false,
            ResolvedAt: null,
            ResolvedByUserId: null);

        var saveResult = await _repository.SaveAsync(record, cancellationToken).ConfigureAwait(false);
        if (saveResult.IsFailure)
            return Result.Failure<IncidentRecord>(saveResult.Error!.Value, saveResult.ErrorMessage!);

        // Escalate critical incidents immediately
        if (severity == IncidentSeverity.Critical)
            await EscalateAsync(record, cancellationToken).ConfigureAwait(false);

        return Result.Success(record);
    }

    /// <summary>
    /// Returns all incidents with the specified severity filter.
    /// </summary>
    public async Task<Result<IReadOnlyList<IncidentRecord>>> GetBySeverityAsync(
        IncidentSeverity severity,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetBySeverityAsync(severity, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Marks an incident as resolved with a resolution note.
    /// </summary>
    public async Task<Result> ResolveAsync(
        string incidentId,
        string resolution,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(incidentId);
        ArgumentNullException.ThrowIfNull(resolution);

        return await _repository.ResolveAsync(incidentId, resolution, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Registers a callback invoked whenever a Critical incident is recorded.
    /// Use this to integrate with safety interlocks or external notification systems.
    /// </summary>
    public void OnCritical(Func<IncidentRecord, Task> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _criticalCallbacks.Add(callback);
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private async Task EscalateAsync(IncidentRecord record, CancellationToken cancellationToken)
    {
        foreach (var callback in _criticalCallbacks)
        {
            try
            {
                await callback(record).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                // Escalation failures must not suppress the incident record.
            }
        }
    }
}
