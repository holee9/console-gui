using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

/// <summary>
/// Defines dose validation and recording operations.
/// Implemented by the HnVue.Dose module.
/// </summary>
public interface IDoseService
{
    /// <summary>
    /// Validates proposed exposure parameters against configured dose reference levels (DRL)
    /// and patient-specific dose history before permitting an exposure.
    /// </summary>
    /// <param name="parameters">Technique factors for the planned exposure.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing a <see cref="DoseValidationResult"/>.
    /// The result level indicates whether to allow, warn, or block the exposure.
    /// </returns>
    Task<Result<DoseValidationResult>> ValidateExposureAsync(
        ExposureParameters parameters,
        CancellationToken cancellationToken = default);

    /// <summary>Persists a dose record after a completed exposure.</summary>
    /// <param name="dose">The dose record to store.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result> RecordDoseAsync(
        DoseRecord dose,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the dose record for the specified study, or <see langword="null"/> if none exists.</summary>
    /// <param name="studyInstanceUid">DICOM Study Instance UID to look up.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result<DoseRecord?>> GetDoseByStudyAsync(
        string studyInstanceUid,
        CancellationToken cancellationToken = default);
}
