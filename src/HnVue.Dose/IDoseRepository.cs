using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Dose;

/// <summary>
/// Abstracts persistence of dose records.
/// Implemented by the Data layer; this interface is local to the Dose module
/// to keep the dependency graph unidirectional.
/// </summary>
public interface IDoseRepository
{
    /// <summary>Persists a dose record.</summary>
    Task<Result> SaveAsync(DoseRecord dose, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the dose record for the specified study, or a
    /// successful null result when no record exists.
    /// </summary>
    Task<Result<DoseRecord?>> GetByStudyAsync(string studyInstanceUid, CancellationToken cancellationToken = default);
}
