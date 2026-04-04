namespace HnVue.Common.Models;

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
public sealed record DoseRecord(
    string DoseId,
    string StudyInstanceUid,
    double Dap,
    double Ei,
    double EffectiveDose,
    string BodyPart,
    DateTimeOffset RecordedAt);
