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
}
