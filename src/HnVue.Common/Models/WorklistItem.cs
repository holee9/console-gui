namespace HnVue.Common.Models;

/// <summary>
/// Represents a single entry returned by a DICOM Modality Worklist (MWL) query.
/// </summary>
/// <param name="AccessionNumber">RIS accession number uniquely identifying the order.</param>
/// <param name="PatientId">Hospital patient identifier.</param>
/// <param name="PatientName">Patient name in DICOM PN format.</param>
/// <param name="StudyDate">Scheduled study date; null when not specified by the worklist server.</param>
/// <param name="BodyPart">DICOM body-part examined code; null when not specified.</param>
/// <param name="RequestedProcedure">Description of the requested imaging procedure; null when not specified.</param>
public sealed record WorklistItem(
    string AccessionNumber,
    string PatientId,
    string PatientName,
    DateOnly? StudyDate,
    string? BodyPart,
    string? RequestedProcedure);
