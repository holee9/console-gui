namespace HnVue.Common.Models;

/// <summary>
/// Describes an available software update package discovered by <c>ISWUpdateService</c>.
/// </summary>
/// <param name="Version">Semantic version string of the update (e.g., "2.1.0").</param>
/// <param name="ReleaseNotes">Optional human-readable release notes in plain text or Markdown.</param>
/// <param name="PackageUrl">URL from which the update package can be downloaded.</param>
/// <param name="Sha256Hash">Expected SHA-256 hash of the package file for integrity verification.</param>
public sealed record UpdateInfo(
    string Version,
    string? ReleaseNotes,
    string PackageUrl,
    string Sha256Hash);
