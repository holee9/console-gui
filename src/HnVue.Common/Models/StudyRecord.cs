namespace HnVue.Common.Models;

// @MX:NOTE StudyRecord record - DICOM study container with RIS accession number, body part exam code
/// <summary>
/// Represents a DICOM study associated with a patient.
/// </summary>
/// <param name="StudyInstanceUid">DICOM Study Instance UID (globally unique identifier).</param>
/// <param name="PatientId">ID of the patient this study belongs to.</param>
/// <param name="StudyDate">UTC date and time when the study was performed.</param>
/// <param name="Description">Optional free-text study description.</param>
/// <param name="AccessionNumber">Optional Radiology Information System (RIS) accession number.</param>
/// <param name="BodyPart">Optional DICOM body-part examined code (e.g., "CHEST", "HAND").</param>
public sealed record StudyRecord(
    string StudyInstanceUid,
    string PatientId,
    DateTimeOffset StudyDate,
    string? Description,
    string? AccessionNumber,
    string? BodyPart);
