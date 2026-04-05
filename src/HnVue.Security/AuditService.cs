using System.Security.Cryptography;
using System.Text;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using Microsoft.Extensions.Options;

namespace HnVue.Security;

/// <summary>
/// Implements tamper-evident audit logging using an HMAC-SHA256 hash chain.
/// Each entry references the hash of the preceding entry, forming a chain that
/// detects any modification or deletion of historical records.
/// </summary>
public sealed class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;
    private readonly byte[] _hmacKey;

    /// <summary>
    /// Initialises a new <see cref="AuditService"/> with the specified repository and HMAC key options.
    /// </summary>
    /// <param name="auditRepository">Repository for persisting audit entries.</param>
    /// <param name="options">HMAC key configuration. The key must not be empty.</param>
    /// <exception cref="ArgumentException">Thrown when <see cref="AuditOptions.HmacKey"/> is null or empty.</exception>
    public AuditService(IAuditRepository auditRepository, IOptions<AuditOptions> options)
    {
        _auditRepository = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
        var opts = options?.Value ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(opts.HmacKey))
            throw new ArgumentException(
                "AuditOptions.HmacKey must be configured. " +
                "Set 'Security:AuditHmacKey' in configuration or the HNVUE_AUDIT_HMAC_KEY environment variable.",
                nameof(options));

        _hmacKey = Encoding.UTF8.GetBytes(opts.HmacKey);
    }

    /// <inheritdoc/>
    public async Task<Result> WriteAuditAsync(
        AuditEntry entry,
        CancellationToken cancellationToken = default)
    {
        var lastHashResult = await _auditRepository.GetLastHashAsync(cancellationToken).ConfigureAwait(false);
        // A database-level failure is fatal; NotFound simply means the log is empty (null previous hash).
        if (lastHashResult.IsFailure && lastHashResult.Error != ErrorCode.NotFound)
            return Result.Failure(ErrorCode.IncidentLogFailed, "Failed to retrieve last audit hash.");

        var previousHash = lastHashResult.IsSuccess ? lastHashResult.Value : null;

        var payload = BuildPayload(entry.EntryId, entry.Timestamp, entry.UserId, entry.Action, entry.Details, previousHash);
        var currentHash = ComputeHmacInternal(payload, _hmacKey);

        var newEntry = new AuditEntry(
            entry.EntryId,
            entry.Timestamp,
            entry.UserId,
            entry.Action,
            entry.Details,
            previousHash,
            currentHash);

        var appendResult = await _auditRepository.AppendAsync(newEntry, cancellationToken).ConfigureAwait(false);
        return appendResult.IsFailure
            ? Result.Failure(ErrorCode.IncidentLogFailed, appendResult.ErrorMessage ?? "Failed to append audit entry.")
            : Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> VerifyChainIntegrityAsync(
        CancellationToken cancellationToken = default)
    {
        // Query all entries with a high limit to retrieve the full log.
        var queryResult = await _auditRepository
            .QueryAsync(new AuditQueryFilter(MaxResults: int.MaxValue), cancellationToken)
            .ConfigureAwait(false);

        if (queryResult.IsFailure)
            return Result.Failure<bool>(ErrorCode.IncidentLogFailed, "Failed to retrieve audit log for verification.");

        var entries = queryResult.Value;
        string? expectedPreviousHash = null;

        foreach (var entry in entries)
        {
            // Verify the PreviousHash links correctly to the prior entry.
            if (entry.PreviousHash != expectedPreviousHash)
                return Result.Success(false);

            // Recompute the HMAC for this entry and compare to stored hash.
            var payload = BuildPayload(entry.EntryId, entry.Timestamp, entry.UserId, entry.Action, entry.Details, entry.PreviousHash);
            var recomputedHash = ComputeHmacInternal(payload, _hmacKey);
            if (!string.Equals(recomputedHash, entry.CurrentHash, StringComparison.Ordinal))
                return Result.Success(false);

            expectedPreviousHash = entry.CurrentHash;
        }

        return Result.Success(true);
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<AuditEntry>>> GetAuditLogsAsync(
        AuditQueryFilter filter,
        CancellationToken cancellationToken = default)
    {
        return await _auditRepository.QueryAsync(filter, cancellationToken).ConfigureAwait(false);
    }

    // ── Internal helpers (internal for SecurityService reuse) ─────────────────────

    /// <summary>
    /// Computes an HMAC-SHA256 over the given payload using the specified key.
    /// Returns a lowercase hex string.
    /// </summary>
    /// <param name="payload">String payload to hash.</param>
    /// <param name="key">HMAC key bytes.</param>
    /// <returns>Lowercase hex-encoded HMAC-SHA256 digest.</returns>
    internal static string ComputeHmacInternal(string payload, byte[] key)
    {
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string BuildPayload(
        string entryId,
        DateTimeOffset timestamp,
        string userId,
        string action,
        string? details,
        string? previousHash)
        => $"{entryId}|{timestamp:O}|{userId}|{action}|{details}|{previousHash}";
}
