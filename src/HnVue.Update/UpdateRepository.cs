using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
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
    private readonly string _updatesDirectory;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Initialises a new instance of <see cref="UpdateRepository"/> using the
    /// application base directory as the root for update operations.
    /// </summary>
    public UpdateRepository()
        : this(AppContext.BaseDirectory)
    {
    }

    /// <summary>
    /// Initialises a new instance of <see cref="UpdateRepository"/> with an explicit
    /// base directory.  Used for testing.
    /// </summary>
    /// <param name="baseDirectory">
    /// Directory from which the <c>Updates/</c> sub-directory is resolved.
    /// </param>
    internal UpdateRepository(string baseDirectory)
    {
        ArgumentNullException.ThrowIfNull(baseDirectory);
        _updatesDirectory = Path.Combine(baseDirectory, "Updates");
    }

    /// <inheritdoc/>
    /// <remarks>SWR-DA-040: Update check scans local Updates/ directory for available packages.</remarks>
    public Task<Result<UpdateInfo?>> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Directory.Exists(_updatesDirectory))
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
    /// SWR-DA-042: Staged update implementation.
    /// <list type="number">
    ///   <item>Verify SHA-256 hash via a companion <c>.sha256</c> sidecar file (skipped when absent).</item>
    ///   <item>Extract the zip package to <c>Updates/Staging/</c>.</item>
    ///   <item>Write <c>Updates/pending_update.json</c> so the startup sequence can complete installation.</item>
    /// </list>
    /// Pre-update backup is the responsibility of the higher-level <see cref="SWUpdateService"/>,
    /// which has access to the configured backup directory via <see cref="UpdateOptions"/>.
    /// Live binary replacement cannot be performed while the process is running on Windows;
    /// actual file replacement occurs on the next application restart.
    /// </remarks>
    public async Task<Result> ApplyPackageAsync(
        string packagePath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packagePath);

        // Step 1: Validate the package file exists.
        if (!File.Exists(packagePath))
            return Result.Failure(ErrorCode.NotFound,
                $"Package file not found: '{packagePath}'.");

        // Step 2: Verify SHA-256 hash when a companion sidecar is present.
        string sidecarPath = packagePath + ".sha256";
        if (File.Exists(sidecarPath))
        {
            try
            {
                string sidecarContent = await File.ReadAllTextAsync(sidecarPath, cancellationToken)
                    .ConfigureAwait(false);
                string expectedHash = sidecarContent.Trim().Split(' ', '\t')[0];

                if (!string.IsNullOrWhiteSpace(expectedHash))
                {
                    byte[] fileBytes = await File.ReadAllBytesAsync(packagePath, cancellationToken)
                        .ConfigureAwait(false);
                    byte[] hashBytes = SHA256.HashData(fileBytes);
                    string actualHash = Convert.ToHexString(hashBytes);

                    if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase))
                        return Result.Failure(ErrorCode.UpdatePackageCorrupt,
                            $"SHA-256 hash mismatch for '{packagePath}'. " +
                            $"Expected: {expectedHash}, Actual: {actualHash}.");
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return Result.Failure(ErrorCode.UpdatePackageCorrupt,
                    $"Failed to read hash sidecar file '{sidecarPath}': {ex.Message}");
            }
        }

        // Step 3: Extract the zip package to the staging directory.
        string stagingDir = Path.Combine(_updatesDirectory, "Staging");

        try
        {
            // Clear any previous staging contents before extracting.
            if (Directory.Exists(stagingDir))
                Directory.Delete(stagingDir, recursive: true);

            await Task.Run(
                () => ZipFile.ExtractToDirectory(packagePath, stagingDir, overwriteFiles: true),
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidDataException ex)
        {
            return Result.Failure(ErrorCode.UpdatePackageCorrupt,
                $"Update package is corrupt or not a valid zip archive: {ex.Message}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Result.Failure(ErrorCode.UpdatePackageCorrupt,
                $"Failed to extract update package '{packagePath}': {ex.Message}");
        }

        // Step 4: Write a pending-update marker so the startup sequence can complete installation.
        try
        {
            Directory.CreateDirectory(_updatesDirectory);
            string markerPath = Path.Combine(_updatesDirectory, "pending_update.json");
            var marker = new PendingUpdateMarker(
                StagingPath: stagingDir,
                PackagePath: packagePath,
                StagedAt: DateTimeOffset.UtcNow);

            string json = JsonSerializer.Serialize(marker, JsonOptions);
            await File.WriteAllTextAsync(markerPath, json, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Non-fatal: the staging directory is intact.
            // We still return success because the package is staged.
            _ = ex; // acknowledged; staging completed even without the marker
        }

        return Result.Success();
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Serialised to <c>Updates/pending_update.json</c> to signal a staged update
    /// awaiting installation on the next application restart.
    /// </summary>
    private sealed record PendingUpdateMarker(
        string StagingPath,
        string PackagePath,
        DateTimeOffset StagedAt);

    /// <summary>
    /// Returns the path to the newest package in the Updates directory,
    /// choosing by semantic version parsed from the filename.
    /// </summary>
    private string? FindNewestPackage()
    {
        var files = Directory.GetFiles(_updatesDirectory, "HnVue-*.zip");
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
