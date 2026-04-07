using System.ComponentModel.DataAnnotations;

namespace HnVue.Data.Entities;

/// <summary>
/// EF Core entity that maps to the <c>Patients</c> table in the encrypted SQLite database.
/// </summary>
// @MX:TODO PatientEntity — Missing: AES-256-GCM encryption for Name, DateOfBirth, CreatedBy fields (SWR-CS-080). HIPAA/GDPR requirement.
public sealed class PatientEntity
{
    /// <summary>DICOM-compatible patient ID (primary key).</summary>
    [Key]
    [MaxLength(64)]
    public string PatientId { get; set; } = string.Empty;

    /// <summary>Patient name in DICOM PN format.</summary>
    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Date of birth stored as ISO-8601 string; null when unknown.</summary>
    [MaxLength(10)]
    public string? DateOfBirth { get; set; }

    /// <summary>Patient sex: M, F, O, or null.</summary>
    [MaxLength(1)]
    public string? Sex { get; set; }

    /// <summary>Whether the patient was registered under emergency workflow.</summary>
    public bool IsEmergency { get; set; }

    /// <summary>UTC ticks of creation timestamp.</summary>
    public long CreatedAtTicks { get; set; }

    /// <summary>UTC offset minutes for <see cref="CreatedAtTicks"/>.</summary>
    public int CreatedAtOffsetMinutes { get; set; }

    /// <summary>User ID of the operator who registered the patient.</summary>
    [Required]
    [MaxLength(64)]
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>Navigation: studies belonging to this patient.</summary>
    public ICollection<StudyEntity> Studies { get; set; } = [];
}
