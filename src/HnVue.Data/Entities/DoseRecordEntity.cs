using System.ComponentModel.DataAnnotations;

namespace HnVue.Data.Entities;

// @MX:ANCHOR: [AUTO] DoseRecordEntity - IEC 60601-2-54 dose records, regulatory audit data (non-deletable via cascade rules)
// @MX:REASON: Safety-critical radiation dose tracking, DeleteBehavior.Restrict in DbContext
// @MX:NOTE: [AUTO] DoseRecordEntity - Store DAP in mGy·cm², EI per IEC 62494, EffectiveDose in mSv for regulatory reporting
/// <summary>
/// EF Core entity that maps to the <c>DoseRecords</c> table.
/// Records radiation dose data for regulatory compliance.
/// </summary>
public sealed class DoseRecordEntity
{
    /// <summary>Unique identifier for the dose record (primary key).</summary>
    [Key]
    [MaxLength(64)]
    public string DoseId { get; set; } = string.Empty;

    /// <summary>DICOM Study Instance UID of the associated study (foreign key).</summary>
    [Required]
    [MaxLength(128)]
    public string StudyInstanceUid { get; set; } = string.Empty;

    /// <summary>Dose-area product in mGy·cm².</summary>
    public double Dap { get; set; }

    /// <summary>Exposure index as defined by IEC 62494.</summary>
    public double Ei { get; set; }

    /// <summary>Estimated effective dose in millisieverts.</summary>
    public double EffectiveDose { get; set; }

    /// <summary>DICOM body-part examined code.</summary>
    [Required]
    [MaxLength(64)]
    public string BodyPart { get; set; } = string.Empty;

    /// <summary>UTC ticks when the dose was recorded.</summary>
    public long RecordedAtTicks { get; set; }

    /// <summary>UTC offset minutes for <see cref="RecordedAtTicks"/>.</summary>
    public int RecordedAtOffsetMinutes { get; set; }

    /// <summary>Navigation: study this dose record belongs to.</summary>
    public StudyEntity? Study { get; set; }
}
