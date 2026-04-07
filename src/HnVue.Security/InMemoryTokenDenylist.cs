using System.Collections.Concurrent;

namespace HnVue.Security;

/// <summary>
/// In-memory token denylist implementation using a thread-safe dictionary.
/// Entries automatically expire after their TTL.
/// SWR-CS-077: Concurrent session handling and token revocation.
/// </summary>
internal sealed class InMemoryTokenDenylist : ITokenDenylist
{
    // @MX:NOTE DenylistEntries - ConcurrentDictionary for thread-safe JTI revocation tracking with expiration
    private readonly ConcurrentDictionary<string, DateTimeOffset> _denylist = new();
    private readonly TimeSpan _defaultTtl;

    /// <summary>
    /// Initializes a new instance with the default TTL.
    /// </summary>
    /// <param name="defaultTtl">Default time-to-live for revocation entries (typically token expiry duration).</param>
    public InMemoryTokenDenylist(TimeSpan defaultTtl)
    {
        _defaultTtl = defaultTtl;
    }

    /// <inheritdoc/>
    public Task RevokeAsync(string jti, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
            throw new ArgumentException("JTI cannot be null, empty, or whitespace.", nameof(jti));

        var expiryTime = DateTimeOffset.UtcNow.Add(ttl ?? _defaultTtl);
        _denylist.AddOrUpdate(jti, expiryTime, (_, _) => expiryTime);

        // Periodic cleanup of expired entries (lazy cleanup on next revoke/check).
        CleanupExpiredEntries();

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
            throw new ArgumentException("JTI cannot be null, empty, or whitespace.", nameof(jti));

        if (!_denylist.TryGetValue(jti, out var expiryTime))
            return Task.FromResult(false);

        // Lazy expiration check.
        if (DateTimeOffset.UtcNow >= expiryTime)
        {
            _denylist.TryRemove(jti, out _);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    // @MX:NOTE CleanupExpiredEntries - Lazy cleanup strategy to prevent unbounded growth of denylist
    private void CleanupExpiredEntries()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var kvp in _denylist)
        {
            if (now >= kvp.Value)
                _denylist.TryRemove(kvp.Key, out _);
        }
    }
}
