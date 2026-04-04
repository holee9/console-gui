namespace HnVue.Common.Models;

/// <summary>
/// Represents a single tamper-evident audit log entry.
/// Each entry is linked to the previous via <see cref="PreviousHash"/> to form a hash chain,
/// fulfilling IEC 62304 and regulatory audit-trail requirements.
/// </summary>
/// <param name="EntryId">Unique identifier for this log entry (defaults to a new GUID string).</param>
/// <param name="Timestamp">UTC timestamp when the event occurred.</param>
/// <param name="UserId">Identifier of the user who performed the action.</param>
/// <param name="Action">Short code or name describing the audited action (e.g., "LOGIN", "EXPOSE").</param>
/// <param name="Details">Optional free-text details or JSON payload associated with the action.</param>
/// <param name="PreviousHash">SHA-256 hash of the preceding audit entry; null for the first entry.</param>
/// <param name="CurrentHash">SHA-256 hash of this entry, computed over all other fields.</param>
public sealed record AuditEntry(
    string EntryId,
    DateTimeOffset Timestamp,
    string UserId,
    string Action,
    string? Details,
    string? PreviousHash,
    string? CurrentHash)
{
    /// <summary>
    /// Initialises an <see cref="AuditEntry"/> with a generated <see cref="EntryId"/>.
    /// </summary>
    public AuditEntry(
        DateTimeOffset timestamp,
        string userId,
        string action,
        string? details = null,
        string? previousHash = null,
        string? currentHash = null)
        : this(Guid.NewGuid().ToString(), timestamp, userId, action, details, previousHash, currentHash)
    {
    }
}
