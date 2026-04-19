using Polly;

namespace HnVue.Common.Abstractions;

// @MX:ANCHOR IRetryPolicyFactory - @MX:REASON: Polly resilience primitive, HTTP/DB retry policies, transient fault handling
/// <summary>
/// Creates pre-configured Polly retry policies for use in infrastructure adapters.
/// Centralising policy creation in Common ensures consistent resilience behaviour
/// across all modules without duplicating Polly configuration.
/// </summary>
public interface IRetryPolicyFactory
{
    /// <summary>
    /// Creates an exponential-backoff retry policy suitable for transient HTTP failures
    /// (e.g., DICOM or update-server communication).
    /// </summary>
    /// <param name="retryCount">Maximum number of retry attempts after the initial failure. Default is 3.</param>
    /// <returns>A Polly <see cref="IAsyncPolicy"/> configured for HTTP-style transient errors.</returns>
    IAsyncPolicy CreateHttpRetryPolicy(int retryCount = 3);

    /// <summary>
    /// Creates an exponential-backoff retry policy suitable for transient database errors
    /// (e.g., SQLite locked or connection pool exhausted).
    /// </summary>
    /// <param name="retryCount">Maximum number of retry attempts after the initial failure. Default is 3.</param>
    /// <returns>A Polly <see cref="IAsyncPolicy"/> configured for database transient errors.</returns>
    IAsyncPolicy CreateDatabaseRetryPolicy(int retryCount = 3);

    /// <summary>
    /// Creates a retry policy for DICOM network operations with extended timeout.
    /// Uses longer backoff intervals suitable for medical imaging network characteristics.
    /// </summary>
    /// <param name="retryCount">Maximum number of retry attempts. Default is 2.</param>
    /// <returns>A Polly <see cref="IAsyncPolicy"/> configured for DICOM retry.</returns>
    IAsyncPolicy CreateDicomRetryPolicy(int retryCount = 2);

    /// <summary>
    /// Creates a circuit-breaker policy that blocks calls after a threshold of consecutive failures.
    /// Prevents cascading failures when a dependent service is unavailable.
    /// </summary>
    /// <param name="failureThreshold">Number of consecutive failures before opening the circuit. Default is 5.</param>
    /// <param name="durationOfBreak">Duration to keep the circuit open before attempting recovery. Default is 30 seconds.</param>
    /// <returns>A Polly <see cref="IAsyncPolicy"/> configured as a circuit breaker.</returns>
    IAsyncPolicy CreateCircuitBreakerPolicy(int failureThreshold = 5, TimeSpan? durationOfBreak = null);
}
