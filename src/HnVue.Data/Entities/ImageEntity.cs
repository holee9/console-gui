using System.ComponentModel.DataAnnotations;

namespace HnVue.Data.Entities;

// @MX:ANCHOR: [AUTO] ImageEntity - File path persistence for acquired X-ray images, regulatory traceability
// @MX:REASON: Links raw detector data to DICOM study, IEC 62304 audit trail requirement
/// <summary>
/// EF Core entity that maps to the <c>Images</c> table.
/// Stores the file path reference for acquired X-ray images.
/// </summary>
public sealed class ImageEntity
{
    /// <summary>Unique image identifier (primary key).</summary>
    [Key]
    [MaxLength(64)]
    public string ImageId { get; set; } = string.Empty;

    /// <summary>DICOM Study Instance UID of the owning study (foreign key).</summary>
    [Required]
    [MaxLength(128)]
    public string StudyInstanceUid { get; set; } = string.Empty;

    /// <summary>Absolute path to the stored image file.</summary>
    [Required]
    [MaxLength(512)]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>UTC ticks when the image was acquired.</summary>
    public long AcquiredAtTicks { get; set; }

    /// <summary>UTC offset minutes for <see cref="AcquiredAtTicks"/>.</summary>
    public int AcquiredAtOffsetMinutes { get; set; }

    /// <summary>Navigation: study this image belongs to.</summary>
    public StudyEntity? Study { get; set; }
}
