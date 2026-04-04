using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

/// <summary>
/// Defines low-level persistence operations for the tamper-evident audit log.
/// Implemented by the HnVue.Data module; consumed only by <c>IAuditService</c>.
/// </summary>
public interface IAuditRepository
{
    /// <summary>Appends a single audit entry to the persistent store.</summary>
    /// <param name="entry">The fully-populated <see cref="AuditEntry"/> to persist.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result> AppendAsync(
        AuditEntry entry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the hash of the most recently persisted audit entry.
    /// Used by the service layer to build the hash chain.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the latest hash string,
    /// or <see langword="null"/> when the log is empty.
    /// </returns>
    Task<Result<string?>> GetLastHashAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Queries stored audit entries matching the supplied filter.</summary>
    /// <param name="filter">Criteria to restrict the result set.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result<IReadOnlyList<AuditEntry>>> QueryAsync(
        AuditQueryFilter filter,
        CancellationToken cancellationToken = default);
}
