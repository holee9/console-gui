using System.ComponentModel.DataAnnotations;

namespace HnVue.Data.Entities;

/// <summary>
/// EF Core entity that maps to the <c>Incidents</c> table.
/// Records radiation safety incidents for IEC 62304 traceability.
/// </summary>
public sealed class IncidentEntity
{
    /// <summary>Unique identifier for the incident (GUID string, primary key).</summary>
    [Key]
    [MaxLength(64)]
    public string IncidentId { get; set; } = string.Empty;

    /// <summary>UTC ticks when the incident occurred.</summary>
    public long OccurredAtTicks { get; set; }

    /// <summary>UTC offset minutes for <see cref="OccurredAtTicks"/>.</summary>
    public int OccurredAtOffsetMinutes { get; set; }

    /// <summary>Identifier of the user who reported the incident.</summary>
    [Required]
    [MaxLength(64)]
    public string ReportedByUserId { get; set; } = string.Empty;

    /// <summary>Severity level stored as integer (0=Critical, 1=High, 2=Medium, 3=Low).</summary>
    public int SeverityValue { get; set; }

    /// <summary>Domain category code (e.g., "DOSE_EXCEEDED", "HARDWARE_FAULT").</summary>
    [Required]
    [MaxLength(64)]
    public string Category { get; set; } = string.Empty;

    /// <summary>Free-text description of the incident.</summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>Free-text resolution notes; null until resolved.</summary>
    public string? Resolution { get; set; }

    /// <summary>Whether the incident has been resolved.</summary>
    public bool IsResolved { get; set; }

    /// <summary>UTC ticks when the incident was resolved; null if open.</summary>
    public long? ResolvedAtTicks { get; set; }

    /// <summary>UTC offset minutes for <see cref="ResolvedAtTicks"/>.</summary>
    public int? ResolvedAtOffsetMinutes { get; set; }

    /// <summary>Identifier of the user who resolved the incident; null if open.</summary>
    [MaxLength(64)]
    public string? ResolvedByUserId { get; set; }
}
