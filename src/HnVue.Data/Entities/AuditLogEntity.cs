using System.ComponentModel.DataAnnotations;

namespace HnVue.Data.Entities;

/// <summary>
/// EF Core entity that maps to the <c>AuditLogs</c> table.
/// Stores tamper-evident audit entries for IEC 62304 compliance.
/// </summary>
public sealed class AuditLogEntity
{
    /// <summary>Unique identifier for the audit entry (primary key).</summary>
    [Key]
    [MaxLength(36)]
    public string EntryId { get; set; } = string.Empty;

    /// <summary>UTC ticks of the event timestamp.</summary>
    public long TimestampTicks { get; set; }

    /// <summary>UTC offset minutes for <see cref="TimestampTicks"/>.</summary>
    public int TimestampOffsetMinutes { get; set; }

    /// <summary>ID of the user who performed the audited action.</summary>
    [Required]
    [MaxLength(64)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>Short code identifying the audited action (e.g., "LOGIN").</summary>
    [Required]
    [MaxLength(64)]
    public string Action { get; set; } = string.Empty;

    /// <summary>Optional free-text details or JSON payload.</summary>
    public string? Details { get; set; }

    /// <summary>SHA-256 hash of the preceding audit entry; null for the first entry.</summary>
    [MaxLength(64)]
    public string? PreviousHash { get; set; }

    /// <summary>SHA-256 hash of this entry, computed over all other fields.</summary>
    [Required]
    [MaxLength(64)]
    public string CurrentHash { get; set; } = string.Empty;
}
