using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Update;

/// <summary>
/// EF Core implementation of <see cref="IUpdateRepository"/>.
/// Tracks software update history in the database.
/// CheckForUpdateAsync queries the update history for the latest applied version.
/// GetPackageInfoAsync and ApplyPackageAsync handle package metadata and installation tracking.
/// </summary>
public sealed class EfUpdateRepository(HnVueDbContext context) : IUpdateRepository
{
    /// <inheritdoc/>
    public async Task<Result<UpdateInfo?>> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var latest = await context.UpdateHistories
                .AsNoTracking()
                .Where(h => h.Status == "Installed")
                .OrderByDescending(h => h.Timestamp)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (latest is null)
                return Result.SuccessNullable<UpdateInfo?>(null);

            // Return info about the currently installed version
            var info = new UpdateInfo(
                Version: latest.ToVersion,
                ReleaseNotes: $"Updated from {latest.FromVersion}",
                PackageUrl: string.Empty,
                Sha256Hash: latest.PackageHash);

            return Result.SuccessNullable<UpdateInfo?>(info);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<UpdateInfo?>(ErrorCode.DatabaseError, ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<UpdateInfo>> GetPackageInfoAsync(string packagePath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packagePath);

        try
        {
            if (!System.IO.File.Exists(packagePath))
                return Result.Failure<UpdateInfo>(ErrorCode.NotFound, $"Package file not found: {packagePath}");

            // Compute SHA-256 hash for integrity verification
            using var stream = System.IO.File.OpenRead(packagePath);
            var hashBytes = await System.Security.Cryptography.SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
            var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();

            // Extract version from filename convention: HnVue-{version}.zip
            var fileName = System.IO.Path.GetFileNameWithoutExtension(packagePath);
            var version = fileName.Contains('-') ? fileName.Split('-')[^1] : "0.0.0";

            var info = new UpdateInfo(
                Version: version,
                ReleaseNotes: null,
                PackageUrl: packagePath,
                Sha256Hash: hash);

            return Result.Success(info);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<UpdateInfo>(ErrorCode.FileOperationFailed, ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result> ApplyPackageAsync(string packagePath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packagePath);

        try
        {
            if (!System.IO.File.Exists(packagePath))
                return Result.Failure(ErrorCode.NotFound, $"Package file not found: {packagePath}");

            // Get current version from history
            var latest = await context.UpdateHistories
                .AsNoTracking()
                .Where(h => h.Status == "Installed")
                .OrderByDescending(h => h.Timestamp)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            var currentVersion = latest?.ToVersion ?? "0.0.0";

            // Extract version from filename
            var fileName = System.IO.Path.GetFileNameWithoutExtension(packagePath);
            var newVersion = fileName.Contains('-') ? fileName.Split('-')[^1] : currentVersion;

            // Compute hash
            using var stream = System.IO.File.OpenRead(packagePath);
            var hashBytes = await System.Security.Cryptography.SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
            var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();

            // Record update history
            var historyEntry = new Data.Entities.UpdateHistoryEntity
            {
                Timestamp = DateTime.UtcNow,
                FromVersion = currentVersion,
                ToVersion = newVersion,
                Status = "Installed",
                InstalledBy = Environment.UserName,
                PackageHash = hash,
            };

            context.UpdateHistories.Add(historyEntry);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure(ErrorCode.DatabaseError, ex.InnerException?.Message ?? ex.Message);
        }
    }
}
