using System.IO;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Update;

/// <summary>
/// Implements software update lifecycle: check, verify, apply, and rollback.
/// All packages undergo hash verification before installation per IEC 62304 §6.2.5.
/// </summary>
public sealed class SWUpdateService : ISWUpdateService
{
    private readonly IUpdateRepository _updateRepository;
    private readonly BackupService _backupService;

    /// <summary>
    /// Initialises a new <see cref="SWUpdateService"/>.
    /// </summary>
    public SWUpdateService(IUpdateRepository updateRepository, BackupService backupService)
    {
        _updateRepository = updateRepository
            ?? throw new ArgumentNullException(nameof(updateRepository));
        _backupService = backupService
            ?? throw new ArgumentNullException(nameof(backupService));
    }

    /// <inheritdoc/>
    public async Task<Result<UpdateInfo?>> CheckUpdateAsync(
        CancellationToken cancellationToken = default)
    {
        return await _updateRepository.CheckForUpdateAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result> ApplyUpdateAsync(
        string packagePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packagePath);

        if (string.IsNullOrWhiteSpace(packagePath))
            return Result.Failure(ErrorCode.ValidationFailed, "Package path is required.");

        if (!File.Exists(packagePath))
            return Result.Failure(ErrorCode.UpdatePackageCorrupt,
                $"Package not found: '{packagePath}'.");

        // Retrieve expected hash for this package
        var infoResult = await _updateRepository.GetPackageInfoAsync(packagePath, cancellationToken)
            .ConfigureAwait(false);

        if (infoResult.IsFailure)
            return Result.Failure(infoResult.Error!.Value, infoResult.ErrorMessage!);

        // Verify package integrity
        var verifyResult = await CodeSignVerifier.VerifyHashAsync(
            packagePath, infoResult.Value.Sha256Hash, cancellationToken).ConfigureAwait(false);

        if (verifyResult.IsFailure)
            return verifyResult;

        // Create backup before applying
        var backupResult = await _backupService.CreateBackupAsync(cancellationToken).ConfigureAwait(false);
        if (backupResult.IsFailure)
            return Result.Failure(backupResult.Error!.Value, backupResult.ErrorMessage!);

        // Apply the update
        return await _updateRepository.ApplyPackageAsync(packagePath, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result> RollbackAsync(CancellationToken cancellationToken = default)
    {
        var backups = _backupService.ListBackups();
        if (backups.Count == 0)
            return Result.Failure(ErrorCode.RollbackFailed, "No backup available for rollback.");

        var latestBackup = backups[0];
        return await _backupService.RestoreAsync(latestBackup, cancellationToken).ConfigureAwait(false);
    }
}
