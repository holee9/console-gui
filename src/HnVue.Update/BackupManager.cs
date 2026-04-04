using System.IO;
using HnVue.Common.Results;
using Microsoft.Extensions.Logging;

namespace HnVue.Update;

/// <summary>
/// Manages application directory backups before applying a software update.
/// Backup folders are timestamped to allow selecting the most recent restore point.
/// </summary>
internal sealed class BackupManager
{
    private readonly UpdateOptions _options;
    private readonly ILogger<BackupManager>? _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="BackupManager"/>.
    /// </summary>
    /// <param name="options">Update configuration options.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    public BackupManager(UpdateOptions options, ILogger<BackupManager>? logger = null)
    {
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Creates a timestamped backup of the current application directory.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the backup directory path,
    /// or a failure when the copy operation fails.
    /// </returns>
    public async Task<Result<string>> CreateBackupAsync(CancellationToken ct = default)
    {
        try
        {
            string appDir = _options.ResolvedApplicationDirectory;
            string backupRoot = _options.ResolvedBackupDirectory;
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", System.Globalization.CultureInfo.InvariantCulture);
            string backupPath = Path.Combine(backupRoot, $"backup_{timestamp}");

            _logger?.LogInformation("Creating backup of {AppDir} to {BackupPath}", appDir, backupPath);

            Directory.CreateDirectory(backupPath);

            await CopyDirectoryAsync(appDir, backupPath, ct).ConfigureAwait(false);

            _logger?.LogInformation("Backup created successfully at {BackupPath}", backupPath);
            return Result.Success(backupPath);
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<string>(ErrorCode.OperationCancelled, "Backup operation was cancelled.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to create backup");
            return Result.Failure<string>(ErrorCode.RollbackFailed, $"Backup creation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Restores the application directory from the most recent backup.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result"/> on completion,
    /// or <see cref="ErrorCode.RollbackFailed"/> when no backup exists or the restore fails.
    /// </returns>
    public async Task<Result> RestoreFromBackupAsync(CancellationToken ct = default)
    {
        string? latestBackup = GetLatestBackupPath();

        if (latestBackup is null)
        {
            _logger?.LogWarning("No backup found for restore");
            return Result.Failure(ErrorCode.RollbackFailed, "No backup directory found to restore from.");
        }

        try
        {
            string appDir = _options.ResolvedApplicationDirectory;

            _logger?.LogInformation("Restoring from backup {BackupPath} to {AppDir}", latestBackup, appDir);

            await CopyDirectoryAsync(latestBackup, appDir, ct, overwrite: true).ConfigureAwait(false);

            _logger?.LogInformation("Restore completed successfully");
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            return Result.Failure(ErrorCode.OperationCancelled, "Restore operation was cancelled.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to restore from backup {BackupPath}", latestBackup);
            return Result.Failure(ErrorCode.RollbackFailed, $"Restore failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Returns the path of the most recent backup directory, or <see langword="null"/> when none exists.
    /// </summary>
    public string? GetLatestBackupPath()
    {
        string backupRoot = _options.ResolvedBackupDirectory;

        if (!Directory.Exists(backupRoot))
            return null;

        // Backup directories are named backup_yyyyMMdd_HHmmss; lexicographic sort gives chronological order.
        string[] backupDirs = Directory.GetDirectories(backupRoot, "backup_*");

        if (backupDirs.Length == 0)
            return null;

        Array.Sort(backupDirs, StringComparer.Ordinal);
        return backupDirs[^1]; // Most recent
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static async Task CopyDirectoryAsync(
        string sourceDir,
        string destinationDir,
        CancellationToken ct,
        bool overwrite = false)
    {
        Directory.CreateDirectory(destinationDir);

        foreach (string filePath in Directory.GetFiles(sourceDir))
        {
            ct.ThrowIfCancellationRequested();

            string fileName = Path.GetFileName(filePath);
            string destFile = Path.Combine(destinationDir, fileName);

            // Use async file copy to avoid blocking the thread pool for large files.
            await using FileStream sourceStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 81920, useAsync: true);
            await using FileStream destStream = new(destFile, overwrite ? FileMode.Create : FileMode.CreateNew,
                FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);

            await sourceStream.CopyToAsync(destStream, ct).ConfigureAwait(false);
        }

        // Recurse into subdirectories.
        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            ct.ThrowIfCancellationRequested();

            string subDirName = Path.GetFileName(subDir);
            string destSubDir = Path.Combine(destinationDir, subDirName);

            await CopyDirectoryAsync(subDir, destSubDir, ct, overwrite).ConfigureAwait(false);
        }
    }
}
