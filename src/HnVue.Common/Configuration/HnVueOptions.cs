namespace HnVue.Common.Configuration;

// @MX:NOTE HnVueOptions - Root config with Security (lockout, timeout) and Dicom (AE title, port) sections
/// <summary>
/// Root configuration options for the HnVue application.
/// Bind from the <c>"HnVue"</c> section of <c>appsettings.json</c> or environment variables.
/// </summary>
public sealed class HnVueOptions
{
    /// <summary>The configuration section name used to bind this options class.</summary>
    public const string SectionName = "HnVue";

    /// <summary>Gets or sets security policy options.</summary>
    public SecurityOptions Security { get; set; } = new();

    /// <summary>Gets or sets DICOM networking options.</summary>
    public DicomOptions Dicom { get; set; } = new();

    /// <summary>
    /// Gets or sets the base64-encoded 32-byte key for PHI column-level encryption (SWR-CS-080).
    /// Required when using column-level encryption for patient data.
    /// </summary>
    public string? PhiEncryptionKey { get; set; }

    /// <summary>Security policy configuration nested within <see cref="HnVueOptions"/>.</summary>
    public sealed class SecurityOptions
    {
        /// <summary>
        /// Gets or sets the idle session timeout in minutes after which the user is logged out.
        /// Default is 15 minutes.
        /// </summary>
        public int SessionTimeoutMinutes { get; set; } = 15;

        /// <summary>
        /// Gets or sets the maximum number of consecutive failed login attempts before
        /// the account is automatically locked. Default is 5.
        /// </summary>
        public int MaxFailedLoginAttempts { get; set; } = 5;

        /// <summary>
        /// Gets or sets the duration in minutes that an account remains locked after
        /// exceeding <see cref="MaxFailedLoginAttempts"/>. Default is 30 minutes.
        /// </summary>
        public int LockoutDurationMinutes { get; set; } = 30;
    }

    /// <summary>DICOM networking configuration nested within <see cref="HnVueOptions"/>.</summary>
    public sealed class DicomOptions
    {
        /// <summary>Gets or sets the local Application Entity (AE) title for this console. Default is "HNVUE".</summary>
        public string LocalAeTitle { get; set; } = "HNVUE";

        /// <summary>Gets or sets the TCP port on which the local DICOM SCP listens. Default is 104.</summary>
        public int ListenPort { get; set; } = 104;
    }

    /// <summary>
    /// Validates that all required configuration values are present and within acceptable ranges.
    /// Call at application startup to fail fast on missing or invalid configuration.
    /// </summary>
    /// <returns>An error message if validation fails; null if valid.</returns>
    public string? Validate()
    {
        if (Security.SessionTimeoutMinutes < 1)
            return "Security.SessionTimeoutMinutes must be at least 1.";
        if (Security.MaxFailedLoginAttempts < 1)
            return "Security.MaxFailedLoginAttempts must be at least 1.";
        if (Security.LockoutDurationMinutes < 1)
            return "Security.LockoutDurationMinutes must be at least 1.";
        if (string.IsNullOrWhiteSpace(Dicom.LocalAeTitle))
            return "Dicom.LocalAeTitle is required.";
        if (Dicom.LocalAeTitle.Length > 16)
            return "Dicom.LocalAeTitle must be 16 characters or fewer.";
        if (Dicom.ListenPort is < 1 or > 65535)
            return "Dicom.ListenPort must be between 1 and 65535.";
        return null;
    }
}
