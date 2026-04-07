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

    /// <summary>
    /// Returns dose records for the specified patient within an optional date range.
    /// SWR-DM-051~052: cumulative dose history retrieval by patient.
    /// </summary>
    /// <param name="patientId">Patient identifier to query.</param>
    /// <param name="from">Inclusive start of the date range. <see langword="null"/> means no lower bound.</param>
    /// <param name="until">Inclusive end of the date range. <see langword="null"/> means no upper bound.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of matching dose records, ordered by <see cref="DoseRecord.RecordedAt"/> ascending.</returns>
    Task<Result<IReadOnlyList<DoseRecord>>> GetByPatientAsync(
        string patientId,
        DateTimeOffset? from,
        DateTimeOffset? until,
        CancellationToken cancellationToken = default);
}
