using System.ComponentModel.DataAnnotations;

namespace HnVue.Data.Entities;

/// <summary>
/// EF Core entity that maps to the <c>SystemSettings</c> table.
/// Stores system configuration as a single-row settings table.
/// </summary>
public sealed class SystemSettingsEntity
{
    /// <summary>Singleton primary key (always 1).</summary>
    [Key]
    public int Id { get; set; } = 1;

    // ── DICOM Settings ────────────────────────────────────────────────────────

    /// <summary>AE title of the remote PACS server.</summary>
    [Required]
    [MaxLength(64)]
    public string PacsAeTitle { get; set; } = string.Empty;

    /// <summary>Hostname or IP address of the PACS server.</summary>
    [Required]
    [MaxLength(256)]
    public string PacsHost { get; set; } = string.Empty;

    /// <summary>DICOM TCP port of the PACS server.</summary>
    public int PacsPort { get; set; } = 104;

    /// <summary>Local AE title advertised by this console.</summary>
    [Required]
    [MaxLength(64)]
    public string LocalAeTitle { get; set; } = "HNVUE";

    // ── Generator Settings ────────────────────────────────────────────────────

    /// <summary>COM port name for generator communication.</summary>
    [Required]
    [MaxLength(16)]
    public string ComPort { get; set; } = string.Empty;

    /// <summary>Baud rate for serial communication.</summary>
    public int BaudRate { get; set; } = 9600;

    /// <summary>Communication timeout in milliseconds.</summary>
    public int TimeoutMs { get; set; } = 5000;

    // ── Security Settings ─────────────────────────────────────────────────────

    /// <summary>Idle session timeout in minutes.</summary>
    public int SessionTimeoutMinutes { get; set; } = 15;

    /// <summary>Maximum consecutive failed logins before account lockout.</summary>
    public int MaxFailedLogins { get; set; } = 5;
}
