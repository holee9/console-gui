using HnVue.Common.Enums;
using HnVue.Common.Results;
using HnVue.Incident.Models;

namespace HnVue.Incident;

/// <summary>
/// Defines operations for incident reporting, classification, and audit trail management.
/// Implementations must guarantee that every incident write is accompanied by a tamper-evident
/// audit entry (IEC 62304 Class B safety requirement).
/// </summary>
public interface IIncidentService
{
    /// <summary>
    /// Reports a new incident and persists it together with a tamper-evident audit entry.
    /// Critical severity incidents include an additional "CRITICAL_INCIDENT" marker in the
    /// audit entry details to flag the record for immediate operator attention.
    /// </summary>
    /// <param name="reportedByUserId">Identifier of the user reporting the incident.</param>
    /// <param name="severity">Severity classification of the incident.</param>
    /// <param name="category">Domain category code (e.g. "DOSE_EXCEEDED", "HARDWARE_FAULT").</param>
    /// <param name="description">Free-text description of what occurred.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the newly created <see cref="IncidentRecord"/>,
    /// or a failure result if the incident could not be stored.
    /// </returns>
    Task<Result<IncidentRecord>> ReportAsync(
        string reportedByUserId,
        IncidentSeverity severity,
        string category,
        string description,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the incident with the specified identifier.
    /// </summary>
    /// <param name="incidentId">The unique identifier of the incident to retrieve.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the <see cref="IncidentRecord"/>,
    /// or a failure with <see cref="ErrorCode.NotFound"/> if no matching incident exists.
    /// </returns>
    Task<Result<IncidentRecord>> GetByIdAsync(
        string incidentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a list of incidents, optionally filtered by severity and date range.
    /// </summary>
    /// <param name="severityFilter">When non-null, restricts results to the specified severity.</param>
    /// <param name="from">Inclusive lower bound of the occurrence time range; null means no lower bound.</param>
    /// <param name="toDate">Inclusive upper bound of the occurrence time range; null means no upper bound.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A successful <see cref="Result{T}"/> with the matching records, or a failure.</returns>
    Task<Result<IReadOnlyList<IncidentRecord>>> ListAsync(
        IncidentSeverity? severityFilter = null,
        DateTimeOffset? from = null,
        DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an open incident as resolved and writes a corresponding audit entry.
    /// </summary>
    /// <param name="incidentId">The unique identifier of the incident to resolve.</param>
    /// <param name="resolvedByUserId">Identifier of the user resolving the incident.</param>
    /// <param name="resolution">Free-text description of how the incident was resolved.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the updated <see cref="IncidentRecord"/>,
    /// a failure with <see cref="ErrorCode.NotFound"/> if the incident does not exist,
    /// or a failure with <see cref="ErrorCode.ValidationFailed"/> if the incident is already resolved.
    /// </returns>
    Task<Result<IncidentRecord>> ResolveAsync(
        string incidentId,
        string resolvedByUserId,
        string resolution,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies the integrity of the tamper-evident audit chain for incident-related entries.
    /// Delegates to the underlying <see cref="HnVue.Common.Abstractions.IAuditService"/>.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with <see langword="true"/> when the chain is intact,
    /// <see langword="false"/> when tampering is detected, or a failure if the check cannot be performed.
    /// </returns>
    Task<Result<bool>> VerifyAuditIntegrityAsync(
        CancellationToken cancellationToken = default);
}
