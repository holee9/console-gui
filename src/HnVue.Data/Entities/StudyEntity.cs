using System.ComponentModel.DataAnnotations;

namespace HnVue.Data.Entities;

/// <summary>
/// EF Core entity that maps to the <c>Studies</c> table in the encrypted SQLite database.
/// </summary>
public sealed class StudyEntity
{
    /// <summary>DICOM Study Instance UID (primary key).</summary>
    [Key]
    [MaxLength(128)]
    public string StudyInstanceUid { get; set; } = string.Empty;

    /// <summary>ID of the owning patient (foreign key).</summary>
    [Required]
    [MaxLength(64)]
    public string PatientId { get; set; } = string.Empty;

    /// <summary>UTC ticks of study date/time.</summary>
    public long StudyDateTicks { get; set; }

    /// <summary>UTC offset minutes for <see cref="StudyDateTicks"/>.</summary>
    public int StudyDateOffsetMinutes { get; set; }

    /// <summary>Optional free-text study description.</summary>
    [MaxLength(256)]
    public string? Description { get; set; }

    /// <summary>Optional RIS accession number.</summary>
    [MaxLength(64)]
    public string? AccessionNumber { get; set; }

    /// <summary>Optional DICOM body-part examined code.</summary>
    [MaxLength(64)]
    public string? BodyPart { get; set; }

    /// <summary>Navigation: patient this study belongs to.</summary>
    public PatientEntity? Patient { get; set; }

    /// <summary>Navigation: images in this study.</summary>
    public ICollection<ImageEntity> Images { get; set; } = [];

    /// <summary>Navigation: dose records for this study.</summary>
    public ICollection<DoseRecordEntity> DoseRecords { get; set; } = [];
}
