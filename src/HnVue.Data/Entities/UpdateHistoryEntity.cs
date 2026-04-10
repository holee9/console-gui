using System.ComponentModel.DataAnnotations;

namespace HnVue.Data.Entities;

// @MX:ANCHOR: [AUTO] UpdateHistoryEntity - SW update installation tracking per SDS-DB-9xx
// @MX:REASON: Regulatory requirement for update audit trail (IEC 62304 §6.2.5, FDA 524B)
/// <summary>
/// EF Core entity that maps to the <c>UpdateHistories</c> table.
/// Tracks software update installation history for regulatory compliance.
/// </summary>
public sealed class UpdateHistoryEntity
{
    /// <summary>Unique identifier for the update record (primary key).</summary>
    [Key]
    public int UpdateId { get; set; }

    /// <summary>UTC timestamp when the update was applied.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Version before the update (e.g., "1.0.0").</summary>
    [Required]
    [MaxLength(32)]
    public string FromVersion { get; set; } = string.Empty;

    /// <summary>Version after the update (e.g., "1.1.0").</summary>
    [Required]
    [MaxLength(32)]
    public string ToVersion { get; set; } = string.Empty;

    /// <summary>Update installation status: Installed, Failed, RolledBack.</summary>
    [Required]
    [MaxLength(32)]
    public string Status { get; set; } = string.Empty;

    /// <summary>User or system that initiated the update installation.</summary>
    [Required]
    [MaxLength(128)]
    public string InstalledBy { get; set; } = string.Empty;

    /// <summary>SHA-256 hash of the update package for integrity verification.</summary>
    [Required]
    [MaxLength(128)]
    public string PackageHash { get; set; } = string.Empty;
}
