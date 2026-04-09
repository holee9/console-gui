namespace HnVue.Common.Models;

// @MX:NOTE AuditQueryFilter record - Audit log query parameters with UserId/date range/MaxResults
/// <summary>
/// Defines filter criteria for querying the audit log via <c>IAuditRepository</c>.
/// </summary>
/// <param name="UserId">Optional user ID to filter entries by. Null means all users.</param>
/// <param name="FromDate">Optional inclusive start of the date range. Null means no lower bound.</param>
/// <param name="ToDate">Optional inclusive end of the date range. Null means no upper bound.</param>
/// <param name="MaxResults">Maximum number of entries to return. Defaults to 100.</param>
public sealed record AuditQueryFilter(
    string? UserId = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
    int MaxResults = 100);
