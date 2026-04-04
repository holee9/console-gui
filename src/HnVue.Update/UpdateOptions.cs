using System.IO;

namespace HnVue.Update;

/// <summary>
/// Configuration options for the software update module.
/// Bound from the "SWUpdate" section of appsettings.json.
/// </summary>
public sealed class UpdateOptions
{
    /// <summary>
    /// Gets or sets the base URL of the update server REST API.
    /// Example: <c>https://update.hnvue.com/api/v1</c>
    /// </summary>
    public string UpdateServerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the currently installed version string (semantic version).
    /// Used to determine whether a newer update is available.
    /// </summary>
    public string CurrentVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the directory where application backups are stored before applying an update.
    /// Defaults to <c>%APPDATA%\HnVue\backup</c> when left empty.
    /// </summary>
    public string BackupDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the directory that contains the application binaries to be updated.
    /// Defaults to the current process directory when left empty.
    /// </summary>
    public string ApplicationDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether Authenticode digital-signature verification
    /// is required before applying an update package.
    /// Set to <see langword="false"/> only in development/test environments.
    /// </summary>
    public bool RequireAuthenticodeSignature { get; set; } = true;

    /// <summary>
    /// Resolves the effective backup directory, substituting a sensible default when the property is empty.
    /// </summary>
    internal string ResolvedBackupDirectory =>
        string.IsNullOrWhiteSpace(BackupDirectory)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "HnVue",
                "backup")
            : BackupDirectory;

    /// <summary>
    /// Resolves the effective application directory, substituting the current process directory when empty.
    /// </summary>
    internal string ResolvedApplicationDirectory =>
        string.IsNullOrWhiteSpace(ApplicationDirectory)
            ? AppContext.BaseDirectory
            : ApplicationDirectory;
}
