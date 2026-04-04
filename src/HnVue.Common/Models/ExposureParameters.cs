namespace HnVue.Common.Models;

/// <summary>
/// Describes the radiographic technique factors for a planned X-ray exposure.
/// Used by <c>IDoseService</c> to validate dose before execution.
/// </summary>
/// <param name="BodyPart">DICOM body-part examined code (e.g., "CHEST", "HAND").</param>
/// <param name="Kvp">Peak kilovoltage (kVp) setting for the exposure.</param>
/// <param name="Mas">Milliampere-second (mAs) value for the exposure.</param>
/// <param name="StudyInstanceUid">DICOM Study Instance UID of the associated study.</param>
public sealed record ExposureParameters(
    string BodyPart,
    double Kvp,
    double Mas,
    string StudyInstanceUid);
