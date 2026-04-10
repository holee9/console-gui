namespace HnVue.Common.Models;

// @MX:NOTE SystemSettings - Root configuration aggregate with Dicom/Generator/Security nested classes
/// <summary>
/// Root settings object managed by <c>ISystemAdminService</c>.
/// Aggregates all subsystem configuration into a single serialisable document.
/// </summary>
public sealed class SystemSettings
{
    /// <summary>Gets or sets DICOM networking configuration.</summary>
    public DicomSettings Dicom { get; set; } = new();

    /// <summary>Gets or sets X-ray generator communication configuration.</summary>
    public GeneratorSettings Generator { get; set; } = new();

    /// <summary>Gets or sets security policy configuration.</summary>
    public SecuritySettings Security { get; set; } = new();
}

/// <summary>
/// DICOM networking parameters for PACS connectivity and local AE configuration.
/// </summary>
public sealed class DicomSettings
{
    /// <summary>Gets or sets the AE title of the remote PACS server.</summary>
    public string PacsAeTitle { get; set; } = string.Empty;

    /// <summary>Gets or sets the hostname or IP address of the PACS server.</summary>
    public string PacsHost { get; set; } = string.Empty;

    /// <summary>Gets or sets the DICOM TCP port of the PACS server (default 104).</summary>
    public int PacsPort { get; set; } = 104;

    /// <summary>Gets or sets the local AE title advertised by this console.</summary>
    public string LocalAeTitle { get; set; } = "HNVUE";
}

/// <summary>
/// Serial communication parameters for the X-ray generator interface.
/// </summary>
public sealed class GeneratorSettings
{
    /// <summary>Gets or sets the COM port name (e.g., "COM3") used to communicate with the generator.</summary>
    public string ComPort { get; set; } = string.Empty;

    /// <summary>Gets or sets the baud rate for serial communication. Default is 9600.</summary>
    public int BaudRate { get; set; } = 9600;

    /// <summary>Gets or sets the communication timeout in milliseconds. Default is 5000 ms.</summary>
    public int TimeoutMs { get; set; } = 5000;
}

/// <summary>
/// Security policy settings controlling session and account lockout behaviour.
/// </summary>
public sealed class SecuritySettings
{
    /// <summary>Gets or sets the idle session timeout in minutes. Default is 15 minutes.</summary>
    public int SessionTimeoutMinutes { get; set; } = 15;

    /// <summary>
    /// Gets or sets the maximum number of consecutive failed logins before an account is locked.
    /// Default is 5 attempts.
    /// </summary>
    public int MaxFailedLogins { get; set; } = 5;
}
