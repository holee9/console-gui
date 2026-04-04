using System.IO;
using HnVue.Common.Results;

namespace HnVue.Update;

/// <summary>
/// Provides pre-update backup and rollback capabilities.
/// Creates a timestamped backup of the application directory before applying updates,
/// enabling rollback if the update fails.
/// </summary>
/// <remarks>
/// IEC 62304 §6.2.6: rollback capability for software updates.
/// </remarks>
public sealed class BackupService
{
    private readonly string _applicationDirectory;
    private readonly string _backupBaseDirectory;

    /// <summary>
    /// Initialises a new <see cref="BackupService"/>.
    /// </summary>
    /// <param name="applicationDirectory">Directory containing the installed application.</param>
    /// <param name="backupBaseDirectory">Parent directory where backup snapshots are stored.</param>
    public BackupService(string applicationDirectory, string backupBaseDirectory)
    {
        ArgumentNullException.ThrowIfNull(applicationDirectory);
        ArgumentNullException.ThrowIfNull(backupBaseDirectory);

        _applicationDirectory = applicationDirectory;
        _backupBaseDirectory = backupBaseDirectory;
    }

    /// <summary>
    /// Creates a timestamped backup snapshot of the application directory.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the backup directory path,
    /// or a failure if the backup cannot be created.
    /// </returns>
    public async Task<Result<string>> CreateBackupAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_applicationDirectory))
            return Result.Failure<string>(
                ErrorCode.ValidationFailed,
                $"Application directory does not exist: '{_applicationDirectory}'.");

        try
        {
            var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", System.Globalization.CultureInfo.InvariantCulture);
            var backupDir = Path.Combine(_backupBaseDirectory, $"backup_{timestamp}");

            await Task.Run(
                () => CopyDirectory(_applicationDirectory, backupDir),
                cancellationToken).ConfigureAwait(false);

            return Result.Success(backupDir);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(
                ErrorCode.Unknown,
                $"Backup failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Restores the application from the specified backup directory.
    /// </summary>
    /// <param name="backupPath">Path to a backup snapshot created by <see cref="CreateBackupAsync"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<Result> RestoreAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(backupPath);

        if (!Directory.Exists(backupPath))
            return Result.Failure(
                ErrorCode.NotFound,
                $"Backup directory not found: '{backupPath}'.");

        try
        {
            await Task.Run(() =>
            {
                // Clear current install and copy backup over
                if (Directory.Exists(_applicationDirectory))
                    Directory.Delete(_applicationDirectory, recursive: true);

                CopyDirectory(backupPath, _applicationDirectory);
            }, cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result.Failure(ErrorCode.RollbackFailed, $"Restore failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Returns all available backup snapshots sorted from newest to oldest.
    /// </summary>
    public IReadOnlyList<string> ListBackups()
    {
        if (!Directory.Exists(_backupBaseDirectory))
            return Array.Empty<string>();

        return Directory
            .GetDirectories(_backupBaseDirectory, "backup_*")
            .OrderByDescending(x => x)
            .ToList();
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);

        foreach (var file in Directory.GetFiles(source))
        {
            var destFile = Path.Combine(destination, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite: true);
        }

        foreach (var dir in Directory.GetDirectories(source))
        {
            var destSubDir = Path.Combine(destination, Path.GetFileName(dir));
            CopyDirectory(dir, destSubDir);
        }
    }
}
