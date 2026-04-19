using System.Net.Sockets;
using HnVue.Common.Abstractions;
using Polly;

namespace HnVue.Common.ErrorHandling;

/// <summary>
/// Default implementation of <see cref="IRetryPolicyFactory"/> using Polly resilience primitives.
/// Centralizes retry policy configuration for consistent behavior across all modules.
/// </summary>
public sealed class RetryPolicyFactory : IRetryPolicyFactory
{
    /// <inheritdoc/>
    public IAsyncPolicy CreateHttpRetryPolicy(int retryCount = 3)
    {
        return Policy
            .Handle<SocketException>()
            .Or<TaskCanceledException>(ex => !ex.CancellationToken.IsCancellationRequested)
            .Or<InvalidOperationException>(ex => ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase))
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 200));
    }

    /// <inheritdoc/>
    public IAsyncPolicy CreateDatabaseRetryPolicy(int retryCount = 3)
    {
        return Policy
            .Handle<InvalidOperationException>(ex =>
                ex.Message.Contains("locked", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("busy", StringComparison.OrdinalIgnoreCase))
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 100));
    }

    /// <inheritdoc/>
    public IAsyncPolicy CreateDicomRetryPolicy(int retryCount = 2)
    {
        return Policy
            .Handle<TimeoutException>()
            .Or<SocketException>()
            .Or<OperationCanceledException>(ex => !ex.CancellationToken.IsCancellationRequested)
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(retryAttempt * 5));
    }

    /// <inheritdoc/>
    public IAsyncPolicy CreateCircuitBreakerPolicy(int failureThreshold = 5, TimeSpan? durationOfBreak = null)
    {
        var breakDuration = durationOfBreak ?? TimeSpan.FromSeconds(30);
        return Policy
            .Handle<Exception>(ex => ex is not OperationCanceledException)
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: failureThreshold,
                durationOfBreak: breakDuration);
    }
}
