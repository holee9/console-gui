using System.Collections.Concurrent;
using HnVue.Common.Abstractions;
using HnVue.Common.Results;

namespace HnVue.Security;

/// <summary>
/// In-memory sliding-window rate limiter.
/// STRIDE 'D' (Denial of Service) control per WBS 5.1.17.
/// </summary>
public sealed class RateLimitingService : IRateLimitingService
{
    private readonly ConcurrentDictionary<string, RateLimitCounter> _counters = new();
    private readonly int _defaultMaxAttempts;
    private readonly TimeSpan _defaultWindow;

    /// <summary>
    /// Creates a rate limiter with default limits (5 attempts per 5 minutes).
    /// </summary>
    public RateLimitingService() : this(5, TimeSpan.FromMinutes(5)) { }

    /// <summary>
    /// Creates a rate limiter with custom limits.
    /// </summary>
    public RateLimitingService(int defaultMaxAttempts, TimeSpan defaultWindow)
    {
        _defaultMaxAttempts = defaultMaxAttempts;
        _defaultWindow = defaultWindow;
    }

    /// <inheritdoc/>
    public Result CheckRateLimit(string operationType, string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationType);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var (maxAttempts, window) = GetPolicy(operationType);
        var counterKey = $"{operationType}:{key}";
        var now = DateTimeOffset.UtcNow;

        var counter = _counters.AddOrUpdate(
            counterKey,
            _ => new RateLimitCounter(now, 1),
            (_, existing) =>
            {
                if (now - existing.WindowStart > window)
                    return new RateLimitCounter(now, 1);
                return existing with { AttemptCount = existing.AttemptCount + 1 };
            });

        if (counter.AttemptCount > maxAttempts)
            return Result.Failure(ErrorCode.RateLimitExceeded,
                $"Rate limit exceeded for '{operationType}'. Max {maxAttempts} attempts per {window.TotalMinutes} minutes.");

        return Result.Success();
    }

    /// <inheritdoc/>
    public void Reset(string operationType, string key)
    {
        var counterKey = $"{operationType}:{key}";
        _counters.TryRemove(counterKey, out _);
    }

    private (int maxAttempts, TimeSpan window) GetPolicy(string operationType) => operationType switch
    {
        "LOGIN" => (10, TimeSpan.FromMinutes(5)),
        "EXPOSURE" => (30, TimeSpan.FromMinutes(1)),
        "PASSWORD_CHANGE" => (5, TimeSpan.FromMinutes(15)),
        "PIN_VERIFY" => (5, TimeSpan.FromMinutes(5)),
        _ => (_defaultMaxAttempts, _defaultWindow),
    };

    private sealed record RateLimitCounter(DateTimeOffset WindowStart, int AttemptCount);
}
