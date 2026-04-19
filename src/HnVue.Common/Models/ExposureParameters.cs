namespace HnVue.Common.Models;

// @MX:NOTE ExposureParameters record - Technique factors (kVp/mAs) for dose validation, IEC 60601-2-54 DAP calculation
/// <summary>
/// Describes the radiographic technique factors for a planned X-ray exposure.
/// Used by <c>IDoseService</c> to validate dose before execution.
/// </summary>
/// <param name="BodyPart">DICOM body-part examined code (e.g., "CHEST", "HAND").</param>
/// <param name="Kvp">Peak kilovoltage (kVp) setting for the exposure.</param>
/// <param name="Mas">Milliampere-second (mAs) value for the exposure.</param>
/// <param name="StudyInstanceUid">DICOM Study Instance UID of the associated study.</param>
/// <param name="DistanceCm">Source-to-image distance (SID) in cm. Default 100.0 cm per IEC 60601-2-54.</param>
/// <param name="FieldAreaCm2">X-ray field area at the image receptor in cm². Default 400.0 cm² (20×20cm).</param>
/// <param name="PatientId">
/// Patient identifier for cumulative DAP tracking. When provided, <c>ValidateExposureAsync</c>
/// will check cumulative DAP within the configured time window (default 24h).
/// Null means cumulative tracking is skipped (e.g., anonymous exposure).
/// </param>
public sealed record ExposureParameters(
    string BodyPart,
    double Kvp,
    double Mas,
    string StudyInstanceUid,
    double DistanceCm = 100.0,
    double FieldAreaCm2 = 400.0,
    string? PatientId = null);
