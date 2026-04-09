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
    /// Validates the update options configuration.
    /// Throws <see cref="InvalidOperationException"/> for invalid configuration.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when UpdateServerUrl is null, empty, or uses HTTP instead of HTTPS.
    /// Also thrown when RequireAuthenticodeSignature is disabled in production environment.
    /// </exception>
    public void Validate()
    {
        // Check for null or empty URL
        if (string.IsNullOrWhiteSpace(UpdateServerUrl))
        {
            throw new InvalidOperationException(
                "UpdateServerUrl cannot be null, empty, or whitespace.");
        }

        // Enforce HTTPS
        if (!UpdateServerUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            // Check if it's HTTP (not HTTPS)
            if (UpdateServerUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "UpdateServerUrl must use HTTPS for secure communication. " +
                    $"HTTP is not allowed. Current value: '{UpdateServerUrl}'");
            }

            // URL doesn't start with either http:// or https://
            throw new InvalidOperationException(
                "UpdateServerUrl must be a valid HTTPS URL. " +
                $"Current value: '{UpdateServerUrl}'");
        }

        // Enforce Authenticode signature requirement in production
        if (!RequireAuthenticodeSignature && IsProductionEnvironment())
        {
            throw new InvalidOperationException(
                "RequireAuthenticodeSignature cannot be disabled in production environment. " +
                "This is a safety-critical medical device software (IEC 62304 compliance).");
        }
    }

    /// <summary>
    /// Determines whether the current environment is production.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the ASPNETCORE_ENVIRONMENT or DOTNET_ENVIRONMENT
    /// environment variable is set to "Production"; otherwise, <see langword="false"/>.
    /// </returns>
    private static bool IsProductionEnvironment()
    {
        string? env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                     Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

        return string.Equals(env, "Production", StringComparison.OrdinalIgnoreCase);
    }

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
