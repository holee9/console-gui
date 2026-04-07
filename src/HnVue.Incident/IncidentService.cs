using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Incident.Models;
using Microsoft.Extensions.Logging;

namespace HnVue.Incident;

// @MX:WARN IncidentService - @MX:REASON: Safety-critical module for IEC 62304 Class B incident tracking
/// <summary>
/// Default implementation of <see cref="IIncidentService"/>.
/// Every incident mutation produces a tamper-evident audit entry via <see cref="IAuditService"/>.
/// Thread-safe: concurrent calls to <see cref="ReportAsync"/> are serialised by the
/// underlying <see cref="IncidentRepository"/> ConcurrentDictionary.
/// </summary>
internal sealed partial class IncidentService(
    IncidentRepository repository,
    IAuditService auditService,
    NotificationService notificationService,
    ILogger<IncidentService> logger) : IIncidentService
{
    private readonly IncidentRepository _repository = repository;
    private readonly IAuditService _auditService = auditService;
    private readonly NotificationService _notificationService = notificationService;
    private readonly ILogger<IncidentService> _logger = logger;

    // ── High-performance LoggerMessage delegates (CA1848) ─────────────────────

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Audit write failed for incident {IncidentId}: {Error}")]
    private partial void LogAuditWriteFailed(string incidentId, string? error);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Audit write failed for resolved incident {IncidentId}: {Error}")]
    private partial void LogAuditWriteFailedOnResolve(string incidentId, string? error);

    // @MX:ANCHOR ReportAsync - @MX:REASON: High fan_in - called by all UI ViewModels and test fixtures
    /// <inheritdoc/>
    public async Task<Result<IncidentRecord>> ReportAsync(
        string reportedByUserId,
        IncidentSeverity severity,
        string category,
        string description,
        CancellationToken cancellationToken = default)
    {
        var record = new IncidentRecord(
            IncidentId: Guid.NewGuid().ToString(),
            OccurredAt: DateTimeOffset.UtcNow,
            ReportedByUserId: reportedByUserId,
            Severity: severity,
            Category: category,
            Description: description,
            Resolution: null,
            IsResolved: false,
            ResolvedAt: null,
            ResolvedByUserId: null);

        var addResult = await _repository.AddAsync(record, cancellationToken).ConfigureAwait(false);
        if (addResult.IsFailure)
            return Result.Failure<IncidentRecord>(ErrorCode.IncidentLogFailed, addResult.ErrorMessage ?? "Failed to store incident.");

        // @MX:NOTE IEC 62304 risk management: Critical incidents require special audit marker
        // Build audit details; Critical incidents get an additional safety marker.
        var details = severity == IncidentSeverity.Critical
            ? $"severity={severity},category={category},description={description},CRITICAL_INCIDENT"
            : $"severity={severity},category={category},description={description}";

        var auditEntry = new AuditEntry(
            timestamp: record.OccurredAt,
            userId: reportedByUserId,
            action: "INCIDENT_REPORTED",
            currentHash: "pending",
            details: details);

        var auditResult = await _auditService.WriteAuditAsync(auditEntry, cancellationToken).ConfigureAwait(false);
        if (auditResult.IsFailure)
            LogAuditWriteFailed(record.IncidentId, auditResult.ErrorMessage);

        _notificationService.Notify(record);

        return Result.Success(record);
    }

    /// <inheritdoc/>
    public async Task<Result<IncidentRecord>> GetByIdAsync(
        string incidentId,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(incidentId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<IncidentRecord>>> ListAsync(
        IncidentSeverity? severityFilter = null,
        DateTimeOffset? from = null,
        DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default)
    {
        return await _repository.QueryAsync(severityFilter, from, toDate, cancellationToken).ConfigureAwait(false);
    }

    // @MX:ANCHOR ResolveAsync - @MX:REASON: Core incident lifecycle operation, used by incident management UI
    /// <inheritdoc/>
    public async Task<Result<IncidentRecord>> ResolveAsync(
        string incidentId,
        string resolvedByUserId,
        string resolution,
        CancellationToken cancellationToken = default)
    {
        var getResult = await _repository.GetByIdAsync(incidentId, cancellationToken).ConfigureAwait(false);
        if (getResult.IsFailure)
            return Result.Failure<IncidentRecord>(ErrorCode.NotFound, getResult.ErrorMessage ?? $"Incident '{incidentId}' not found.");

        var existing = getResult.Value;
        if (existing.IsResolved)
        {
            return Result.Failure<IncidentRecord>(
                ErrorCode.ValidationFailed,
                $"Incident '{incidentId}' is already resolved.");
        }

        var resolved = existing with
        {
            Resolution = resolution,
            IsResolved = true,
            ResolvedAt = DateTimeOffset.UtcNow,
            ResolvedByUserId = resolvedByUserId,
        };

        var updateResult = await _repository.UpdateAsync(resolved, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
            return Result.Failure<IncidentRecord>(ErrorCode.IncidentLogFailed, updateResult.ErrorMessage ?? "Failed to update incident.");

        var auditEntry = new AuditEntry(
            timestamp: resolved.ResolvedAt!.Value,
            userId: resolvedByUserId,
            action: "INCIDENT_RESOLVED",
            currentHash: "pending",
            details: $"incidentId={incidentId},resolution={resolution}");

        var auditResult = await _auditService.WriteAuditAsync(auditEntry, cancellationToken).ConfigureAwait(false);
        if (auditResult.IsFailure)
            LogAuditWriteFailedOnResolve(incidentId, auditResult.ErrorMessage);

        return Result.Success(resolved);
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> VerifyAuditIntegrityAsync(
        CancellationToken cancellationToken = default)
    {
        return await _auditService.VerifyChainIntegrityAsync(cancellationToken).ConfigureAwait(false);
    }
}
