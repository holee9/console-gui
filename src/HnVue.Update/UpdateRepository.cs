using System.IO;
using System.Text.Json;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Update;

/// <summary>
/// File-system-based implementation of <see cref="IUpdateRepository"/>.
/// Scans the <c>Updates/</c> sub-directory next to the application binary for
/// packages matching the naming convention <c>HnVue-{version}.zip</c>.
/// </summary>
public sealed class UpdateRepository : IUpdateRepository
{
    private static readonly string UpdatesDirectory = Path.Combine(
        AppContext.BaseDirectory, "Updates");

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc/>
    /// <remarks>SWR-DA-040: Update check scans local Updates/ directory for available packages.</remarks>
    public Task<Result<UpdateInfo?>> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Directory.Exists(UpdatesDirectory))
            {
                return Task.FromResult(Result.SuccessNullable<UpdateInfo?>(null));
            }

            var packagePath = FindNewestPackage();
            if (packagePath is null)
            {
                return Task.FromResult(Result.SuccessNullable<UpdateInfo?>(null));
            }

            var version = ExtractVersionFromPath(packagePath);
            var info = new UpdateInfo(
                Version: version,
                ReleaseNotes: null,
                PackageUrl: packagePath,
                Sha256Hash: string.Empty);

            return Task.FromResult(Result.SuccessNullable<UpdateInfo?>(info!));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Task.FromResult(
                Result.Failure<UpdateInfo?>(ErrorCode.UpdatePackageCorrupt,
                    $"Failed to scan update directory: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// SWR-DA-041: Package metadata is read from a companion JSON file if present,
    /// otherwise minimal metadata is derived from the package filename.
    /// </remarks>
    public async Task<Result<UpdateInfo>> GetPackageInfoAsync(
        string packagePath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packagePath);

        try
        {
            if (!File.Exists(packagePath))
                return Result.Failure<UpdateInfo>(ErrorCode.NotFound,
                    $"Package file not found: '{packagePath}'.");

            // Check for companion metadata file: HnVue-{version}.json
            var metaPath = Path.ChangeExtension(packagePath, ".json");
            if (File.Exists(metaPath))
            {
                await using var stream = File.OpenRead(metaPath);
                var info = await JsonSerializer
                    .DeserializeAsync<UpdateInfo>(stream, JsonOptions, cancellationToken)
                    .ConfigureAwait(false);

                if (info is not null)
                    return Result.Success(info);
            }

            // Fallback: derive info from filename only.
            var version = ExtractVersionFromPath(packagePath);
            return Result.Success(new UpdateInfo(
                Version: version,
                ReleaseNotes: null,
                PackageUrl: packagePath,
                Sha256Hash: string.Empty));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return Result.Failure<UpdateInfo>(ErrorCode.UpdatePackageCorrupt,
                $"Failed to read package metadata for '{packagePath}': {ex.Message}");
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// SWR-DA-042: Full in-place package application is deferred to a dedicated update
    /// service that manages backup and rollback. This method intentionally returns
    /// NotSupportedException to signal that callers must use the higher-level service.
    /// TODO: Implement via BackupService + ZipFile.ExtractToDirectory when update pipeline is ready.
    /// </remarks>
    public Task<Result> ApplyPackageAsync(string packagePath, CancellationToken cancellationToken = default)
    {
        // TODO: delegate to BackupService.CreateBackupAsync(), then ZipFile.ExtractToDirectory()
        return Task.FromResult(
            Result.Failure(ErrorCode.Unknown,
                "In-place package application is not yet supported. Use the update service."));
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the path to the newest package in the Updates directory,
    /// choosing by semantic version parsed from the filename.
    /// </summary>
    private static string? FindNewestPackage()
    {
        var files = Directory.GetFiles(UpdatesDirectory, "HnVue-*.zip");
        if (files.Length == 0)
            return null;

        // Sort by parsed version descending; fall back to string comparison.
        return files
            .OrderByDescending(f => ParseVersion(ExtractVersionFromPath(f)))
            .First();
    }

    private static string ExtractVersionFromPath(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path); // e.g. "HnVue-2.1.0"
        const string prefix = "HnVue-";
        return name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? name[prefix.Length..]
            : name;
    }

    private static Version ParseVersion(string version)
    {
        return Version.TryParse(version, out var v) ? v : new Version(0, 0, 0);
    }
}
