using HnVue.Common.Enums;
using HnVue.Common.Results;

namespace HnVue.Incident;

/// <summary>
/// Abstracts persistence for incident records.
/// </summary>
public interface IIncidentRepository
{
    /// <summary>Persists a new incident record.</summary>
    Task<Result> SaveAsync(IncidentRecord record, CancellationToken cancellationToken = default);

    /// <summary>Returns all incidents with the specified severity.</summary>
    Task<Result<IReadOnlyList<IncidentRecord>>> GetBySeverityAsync(
        IncidentSeverity severity, CancellationToken cancellationToken = default);

    /// <summary>Marks an incident as resolved.</summary>
    Task<Result> ResolveAsync(string incidentId, string resolution, CancellationToken cancellationToken = default);
}
