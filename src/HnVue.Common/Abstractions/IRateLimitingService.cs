using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

/// <summary>
/// Provides rate-limiting for security-sensitive operations.
/// Implements STRIDE 'D' (Denial of Service) control per WBS 5.1.17.
/// </summary>
public interface IRateLimitingService
{
    /// <summary>
    /// Checks whether the operation is allowed under the current rate limit.
    /// </summary>
    /// <param name="operationType">Category of the operation (e.g., "LOGIN", "EXPOSURE").</param>
    /// <param name="key">Unique key for the rate limit scope (e.g., user ID or IP).</param>
    /// <returns>Success if allowed; failure with <see cref="ErrorCode.RateLimitExceeded"/> if blocked.</returns>
    Result CheckRateLimit(string operationType, string key);

    /// <summary>
    /// Resets the rate limit counter for the given operation and key.
    /// </summary>
    void Reset(string operationType, string key);
}
