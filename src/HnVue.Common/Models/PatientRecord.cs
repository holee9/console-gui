namespace HnVue.Common.Models;

// @MX:NOTE PatientRecord record - PHI container with data minimisation, DICOM PN format for name field
/// <summary>
/// Represents a patient demographic record as stored in the local database.
/// Minimal PHI is retained in accordance with data minimisation principles.
/// </summary>
/// <param name="PatientId">DICOM-compatible patient ID (e.g., hospital MRN).</param>
/// <param name="Name">Patient name in DICOM PN format (family^given^middle^prefix^suffix).</param>
/// <param name="DateOfBirth">Patient date of birth; null when unknown or not provided.</param>
/// <param name="Sex">Patient sex as a single character: M, F, O, or null when unknown.</param>
/// <param name="IsEmergency">True when the patient was registered under emergency workflow.</param>
/// <param name="CreatedAt">UTC timestamp when the record was first created.</param>
/// <param name="CreatedBy">User ID of the operator who registered the patient.</param>
public sealed record PatientRecord(
    string PatientId,
    string Name,
    DateOnly? DateOfBirth,
    string? Sex,
    bool IsEmergency,
    DateTimeOffset CreatedAt,
    string CreatedBy);
