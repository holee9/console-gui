using HnVue.Common.Enums;

namespace HnVue.Incident.Models;

/// <summary>
/// Represents a single incident record captured by the incident management module.
/// Immutable after creation; use <see cref="IncidentRecord"/> constructor overloads
/// or record with expressions to create resolved copies.
/// IEC 62304 traceability: fulfils risk management event-log requirement.
/// </summary>
/// <param name="IncidentId">Unique identifier (GUID string) for this incident.</param>
/// <param name="OccurredAt">UTC timestamp when the incident occurred.</param>
/// <param name="ReportedByUserId">Identifier of the user who reported the incident.</param>
/// <param name="Severity">Classification of the incident's severity level.</param>
/// <param name="Category">Domain category code, e.g. "DOSE_EXCEEDED", "HARDWARE_FAULT", "SOFTWARE_ERROR".</param>
/// <param name="Description">Free-text description of the incident.</param>
/// <param name="Resolution">Free-text resolution notes; <see langword="null"/> until resolved.</param>
/// <param name="IsResolved">Indicates whether the incident has been resolved.</param>
/// <param name="ResolvedAt">UTC timestamp when the incident was resolved; <see langword="null"/> if open.</param>
/// <param name="ResolvedByUserId">Identifier of the user who resolved the incident; <see langword="null"/> if open.</param>
public sealed record IncidentRecord(
    string IncidentId,
    DateTimeOffset OccurredAt,
    string ReportedByUserId,
    IncidentSeverity Severity,
    string Category,
    string Description,
    string? Resolution,
    bool IsResolved,
    DateTimeOffset? ResolvedAt,
    string? ResolvedByUserId);
