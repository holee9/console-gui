using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

// @MX:ANCHOR IAuditService - @MX:REASON: Tamper-evident audit contract with 3 methods, IEC 62304 incident logging
/// <summary>
/// Defines tamper-evident audit log operations.
/// The implementation maintains a hash chain across all entries.
/// Implemented by the HnVue.Security module.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Appends a new entry to the tamper-evident audit log.
    /// The implementation is responsible for computing and chaining hashes.
    /// </summary>
    /// <param name="entry">The audit event to record.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A successful <see cref="Result"/>, or a failure with <see cref="ErrorCode.IncidentLogFailed"/>.</returns>
    Task<Result> WriteAuditAsync(
        AuditEntry entry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies that the hash chain across all stored audit entries is intact.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with <c>true</c> when the chain is valid;
    /// <c>false</c> when tampering is detected; or a failure when the check cannot be performed.
    /// </returns>
    Task<Result<bool>> VerifyChainIntegrityAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Detects and reports specific tampered entries in the audit chain.
    /// STRIDE 'T' (Tampering) control — provides granular evidence for incident response.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A list of entry IDs where tampering was detected, or an empty list if the chain is intact.
    /// </returns>
    Task<Result<IReadOnlyList<string>>> DetectTamperedEntriesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit log entries matching the provided filter criteria.
    /// </summary>
    /// <param name="filter">Query parameters to restrict the result set.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A successful <see cref="Result{T}"/> containing the matching entries, or a failure.</returns>
    Task<Result<IReadOnlyList<AuditEntry>>> GetAuditLogsAsync(
        AuditQueryFilter filter,
        CancellationToken cancellationToken = default);
}
