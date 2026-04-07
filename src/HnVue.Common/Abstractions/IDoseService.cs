using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

/// <summary>
/// Defines dose validation and recording operations.
/// Implemented by the HnVue.Dose module.
/// </summary>
// @MX:ANCHOR IDoseService — Safety-critical dose calculation interface; 7 downstream consumers. IEC 60601-2-54 compliance required.
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

    /// <summary>
    /// Calculates Entrance Surface Dose (ESD) from DAP using IEC 60601-2-54 methodology.
    /// SWR-DM-042~043.
    /// </summary>
    /// <param name="dap">Dose-area product in mGy·cm².</param>
    /// <param name="fieldAreaCm2">X-ray field area at the image receptor in cm².</param>
    /// <param name="backscatterFactor">Backscatter factor (default 1.35 for diagnostic X-ray per IAEA TECDOC-1423).</param>
    /// <returns>Entrance surface dose in mGy.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="fieldAreaCm2"/> is less than 1.0 cm².</exception>
    /// <remarks>
    /// Formula: ESD (mGy) = DAP (mGy·cm²) / FieldArea (cm²) × BackScatterFactor
    ///
    /// The backscatter factor accounts for radiation scattered back from the patient's body,
    /// which increases the skin dose. Default value of 1.35 is standard for diagnostic X-ray
    /// examinations per IAEA TECDOC-1423.
    /// </remarks>
    double CalculateEsd(double dap, double fieldAreaCm2, double backscatterFactor = 1.35);

    /// <summary>
    /// Calculates Exposure Index (EI) per IEC 62494-1.
    /// SWR-DM-047~048.
    /// </summary>
    /// <param name="meanPixelValue">Mean pixel value from the acquired image (0-65535 for 16-bit).</param>
    /// <param name="targetPixelValue">Target pixel value for the specific body part and imaging system.</param>
    /// <returns>Exposure Index (EI). Normal operating range: 1000-2000 per IEC 62494-1.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="meanPixelValue"/> is negative or <paramref name="targetPixelValue"/> is not positive.</exception>
    /// <remarks>
    /// Formula: EI = meanPixelValue / targetPixelValue × 1000
    ///
    /// Target values per body part (IEC 62494-1 recommended values):
    ///   CHEST:    1500
    ///   ABDOMEN:  1200
    ///   SPINE:    1800
    ///   SKULL:    1400
    ///   Default:  1500
    ///
    /// EI provides a standardized metric for image exposure quality:
    ///   EI &lt; 800:  Underexposed (increased quantum noise)
    ///   EI 800-1200: Low end of acceptable range
    ///   EI 1200-2000: Optimal exposure range
    ///   EI &gt; 2000: Overexposed (wasted dose, reduced contrast)
    /// </remarks>
    double CalculateExposureIndex(double meanPixelValue, double targetPixelValue);

    /// <summary>
    /// Generates a structured RDSR (Radiation Dose Structured Report) summary for a study.
    /// SWR-DM-044~046.
    /// </summary>
    /// <param name="studyInstanceUid">DICOM Study Instance UID for which to generate the summary.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the enriched <see cref="DoseRecord"/>
    /// with computed <see cref="DoseRecord.EsdMgy"/> and validated <see cref="DoseRecord.Ei"/> fields populated.
    /// Returns <see cref="ErrorCode.NotFound"/> when no dose record exists for the study.
    /// </returns>
    /// <remarks>
    /// DICOM SR file generation (IHE REM profile) is deferred to Phase 3.
    /// This method returns a fully populated <see cref="DoseRecord"/> with all cumulative
    /// dose metrics computed and ready for export. The DICOM SR object encoding step
    /// will wrap this structure in Phase 3 without altering the computation logic here.
    /// </remarks>
    Task<Result<DoseRecord>> GenerateRdsrSummaryAsync(
        string studyInstanceUid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the complete dose history for a patient within an optional date range.
    /// SWR-DM-051~052: cumulative dose assessment over time.
    /// </summary>
    /// <param name="patientId">Patient identifier to query.</param>
    /// <param name="from">Inclusive start of the date range. <see langword="null"/> means no lower bound.</param>
    /// <param name="until">Inclusive end of the date range. <see langword="null"/> means no upper bound.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing a read-only list of <see cref="DoseRecord"/>
    /// ordered by <see cref="DoseRecord.RecordedAt"/> ascending.
    /// Returns an empty list when no records match the criteria.
    /// </returns>
    Task<Result<IReadOnlyList<DoseRecord>>> GetDoseHistoryAsync(
        string patientId,
        DateTimeOffset? from = null,
        DateTimeOffset? until = null,
        CancellationToken cancellationToken = default);
}
