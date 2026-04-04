using HnVue.Common.Enums;

namespace HnVue.Incident;

/// <summary>
/// Represents a recorded system incident.
/// Incidents are append-only once created.
/// </summary>
public sealed record IncidentRecord(
    string IncidentId,
    IncidentSeverity Severity,
    string Category,
    string Description,
    string Source,
    DateTimeOffset OccurredAt,
    bool IsResolved,
    string? Resolution);
