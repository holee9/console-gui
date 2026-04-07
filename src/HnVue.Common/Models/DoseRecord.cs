namespace HnVue.Common.Models;

// @MX:NOTE DoseRecord record - Regulatory dose tracking with IEC 62494 EI, DAP mGy·cm², mSv effective dose
/// <summary>
/// Records radiation dose information for a single exposure event.
/// Persisted for regulatory dose tracking and DLP calculation.
/// </summary>
/// <param name="DoseId">Unique identifier for this dose record.</param>
/// <param name="StudyInstanceUid">DICOM Study Instance UID of the associated study.</param>
/// <param name="Dap">Dose-area product in mGy·cm².</param>
/// <param name="Ei">Exposure index as defined by IEC 62494.</param>
/// <param name="EffectiveDose">Estimated effective dose in millisieverts (mSv).</param>
/// <param name="BodyPart">DICOM body-part examined code (e.g., "CHEST", "ABDOMEN").</param>
/// <param name="RecordedAt">UTC timestamp when the dose was recorded.</param>
/// <param name="PatientId">Patient identifier for dose history retrieval. Null for anonymous exposures.</param>
/// <param name="DapMgyCm2">
/// Dose-area product in mGy·cm² as stored in the RDSR summary computation path (SWR-DM-044~046).
/// Mirrors <see cref="Dap"/> when populated from the RDSR summary generator.
/// </param>
/// <param name="FieldAreaCm2">
/// X-ray field area at the image receptor in cm², required for ESD calculation (SWR-DM-042~043).
/// </param>
/// <param name="MeanPixelValue">
/// Mean pixel value from the acquired image (0–65535 for 16-bit), required for EI computation (SWR-DM-047~048).
/// </param>
/// <param name="EiTarget">
/// Target pixel value for the body part per IEC 62494-1, used to compute the Exposure Index.
/// </param>
/// <param name="EsdMgy">
/// Entrance surface dose in mGy, computed by <c>GenerateRdsrSummaryAsync</c> (SWR-DM-044~046).
/// Null until the RDSR summary has been generated.
/// </param>
public sealed record DoseRecord(
    string DoseId,
    string StudyInstanceUid,
    double Dap,
    double Ei,
    double EffectiveDose,
    string BodyPart,
    DateTimeOffset RecordedAt,
    string? PatientId = null,
    double DapMgyCm2 = 0.0,
    double FieldAreaCm2 = 0.0,
    double MeanPixelValue = 0.0,
    double EiTarget = 0.0,
    double? EsdMgy = null);
