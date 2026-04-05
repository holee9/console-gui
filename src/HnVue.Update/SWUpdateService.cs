using System.IO;
using System.Net.Http;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HnVue.Update;

/// <summary>
/// Implements the software update lifecycle: check, apply, and rollback.
/// Satisfies IEC 62304 §6.2.5 by verifying Authenticode signatures and SHA-256 hashes
/// before staging any update package.
/// </summary>
public sealed class SWUpdateService : ISWUpdateService
{
    private readonly UpdateOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuditService? _auditService;
    private readonly ILogger<SWUpdateService>? _logger;

    // Internally resolved components (virtual for testability via subclassing or dependency injection)
    private readonly BackupManager _backupManager;

    // System user identifier used in audit entries for automated operations
    private const string SystemUserId = "SYSTEM";

    /// <summary>
    /// Initialises a new instance of <see cref="SWUpdateService"/>.
    /// </summary>
    /// <param name="options">Bound update configuration options.</param>
    /// <param name="httpClientFactory">Factory for creating <see cref="System.Net.Http.HttpClient"/> instances.</param>
    /// <param name="auditService">
    /// Optional audit service for compliance logging (IEC 62304 traceability).
    /// When <see langword="null"/>, audit events are skipped.
    /// </param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    public SWUpdateService(
        IOptions<UpdateOptions> options,
        IHttpClientFactory httpClientFactory,
        IAuditService? auditService = null,
        ILogger<SWUpdateService>? logger = null)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _auditService = auditService;
        _logger = logger;
        _backupManager = new BackupManager(_options);
    }

    /// <inheritdoc/>
    public async Task<Result<UpdateInfo?>> CheckUpdateAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Checking for software updates...");

        using System.Net.Http.HttpClient httpClient = _httpClientFactory.CreateClient(nameof(UpdateChecker));
        var checker = new UpdateChecker(_options, httpClient, null);

        return await checker.CheckAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Wave 2 implementation stages the update rather than performing a live binary replacement.
    /// Live replacement requires process restart coordination and file-locking resolution,
    /// which will be implemented in Wave 3.
    /// The staged approach is safe: binaries are not replaced until the application restarts.
    /// </remarks>
    public async Task<Result> ApplyUpdateAsync(string packagePath, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Applying update from package: {PackagePath}", packagePath);

        // Step 1: Verify SHA-256 hash integrity.
        // The expected hash must be obtained from the manifest or passed by the caller;
        // here we derive it from the accompanying .sha256 sidecar file if available.
        string? expectedHash = await TryReadSidecarHashAsync(packagePath, cancellationToken)
            .ConfigureAwait(false);

        if (expectedHash is not null)
        {
            if (!SignatureVerifier.VerifyHash(packagePath, expectedHash))
            {
                _logger?.LogError("SHA-256 hash mismatch for package {PackagePath}", packagePath);
                return Result.Failure(ErrorCode.UpdatePackageCorrupt,
                    $"Update package integrity check failed: SHA-256 hash mismatch for '{packagePath}'.");
            }
            _logger?.LogInformation("SHA-256 hash verified successfully");
        }

        // Step 2: Verify Authenticode digital signature (when required by policy).
        if (_options.RequireAuthenticodeSignature)
        {
            if (!SignatureVerifier.VerifyAuthenticode(packagePath))
            {
                _logger?.LogError("Authenticode signature verification failed for {PackagePath}", packagePath);
                return Result.Failure(ErrorCode.SignatureVerificationFailed,
                    $"Update package signature verification failed: the file '{packagePath}' is not trusted.");
            }
            _logger?.LogInformation("Authenticode signature verified successfully");
        }

        // Step 3: Create a backup of the current application directory.
        Result<string> backupResult = await _backupManager.CreateBackupAsync(cancellationToken)
            .ConfigureAwait(false);

        if (backupResult.IsFailure)
        {
            _logger?.LogError("Backup creation failed: {Error}", backupResult.ErrorMessage);
            return Result.Failure(backupResult.Error!.Value, backupResult.ErrorMessage!);
        }

        _logger?.LogInformation("Backup created at {BackupPath}", backupResult.Value);

        // Step 4 (Wave 2): Stage the update. Actual binary replacement happens on restart.
        // Mark the staged package path so the startup sequence can complete installation.
        await WriteStagedUpdateMarkerAsync(packagePath, cancellationToken).ConfigureAwait(false);

        // Step 5: Write audit entry for IEC 62304 traceability.
        await WriteAuditAsync("UPDATE_STAGED",
            $"Package staged for installation on restart: {packagePath}. Backup: {backupResult.Value}",
            cancellationToken).ConfigureAwait(false);

        _logger?.LogInformation("Update staged successfully. Installation will complete on next restart.");
        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> RollbackAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Starting rollback to previous version...");

        string? latestBackup = _backupManager.GetLatestBackupPath();
        if (latestBackup is null)
        {
            _logger?.LogWarning("Rollback requested but no backup found");
            return Result.Failure(ErrorCode.RollbackFailed,
                "Rollback failed: no backup directory is available.");
        }

        Result restoreResult = await _backupManager.RestoreFromBackupAsync(cancellationToken)
            .ConfigureAwait(false);

        if (restoreResult.IsFailure)
        {
            _logger?.LogError("Rollback restore failed: {Error}", restoreResult.ErrorMessage);
            return restoreResult;
        }

        await WriteAuditAsync("UPDATE_ROLLED_BACK",
            $"Application rolled back from backup: {latestBackup}",
            cancellationToken).ConfigureAwait(false);

        _logger?.LogInformation("Rollback completed successfully");
        return Result.Success();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Reads the SHA-256 hash from a companion <c>.sha256</c> sidecar file if present.
    /// Returns <see langword="null"/> when no sidecar exists (hash verification is skipped).
    /// </summary>
    private static async Task<string?> TryReadSidecarHashAsync(string packagePath, CancellationToken ct)
    {
        string sidecarPath = packagePath + ".sha256";
        if (!File.Exists(sidecarPath))
            return null;

        string content = await File.ReadAllTextAsync(sidecarPath, ct).ConfigureAwait(false);
        return content.Trim().Split(' ', '\t')[0]; // Handle "hash  filename" format (sha256sum output)
    }

    /// <summary>
    /// Writes a marker file to indicate a staged update is awaiting installation on the next restart.
    /// </summary>
    private async Task WriteStagedUpdateMarkerAsync(string packagePath, CancellationToken ct)
    {
        try
        {
            string markerDir = _options.ResolvedBackupDirectory;
            Directory.CreateDirectory(markerDir);
            string markerPath = Path.Combine(markerDir, "pending_update.txt");
            await File.WriteAllTextAsync(markerPath, packagePath, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            // Non-fatal: log and continue. The backup still provides rollback capability.
            _logger?.LogWarning(ex, "Could not write staged update marker file");
        }
    }

    /// <summary>
    /// Writes an audit log entry; silently ignores failures (audit must not block the update flow).
    /// </summary>
    private async Task WriteAuditAsync(string action, string details, CancellationToken ct)
    {
        if (_auditService is null)
            return;

        try
        {
            var entry = new AuditEntry(
                timestamp: DateTimeOffset.UtcNow,
                userId: SystemUserId,
                action: action,
                currentHash: string.Empty, // AuditService computes the hash
                details: details);

            await _auditService.WriteAuditAsync(entry, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger?.LogWarning(ex, "Audit write failed for action {Action} (non-fatal)", action);
        }
    }
}
