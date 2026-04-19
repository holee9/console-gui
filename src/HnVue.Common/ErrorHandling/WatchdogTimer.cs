namespace HnVue.Common.ErrorHandling;

/// <summary>
/// Watchdog timer that invokes a callback when a deadline is exceeded.
/// Implements WP-T1-ERR watchdog for detecting stalled operations.
/// </summary>
public sealed class WatchdogTimer : IDisposable
{
    private readonly TimeSpan _timeout;
    private readonly Action _onTimeout;
    private readonly CancellationTokenSource _cts = new();
    private Task? _watchdogTask;

    /// <summary>
    /// Creates a watchdog timer.
    /// </summary>
    /// <param name="timeout">Maximum allowed duration before the watchdog fires.</param>
    /// <param name="onTimeout">Callback invoked when the timeout is exceeded.</param>
    public WatchdogTimer(TimeSpan timeout, Action onTimeout)
    {
        _timeout = timeout;
        _onTimeout = onTimeout;
    }

    /// <summary>Indicates whether the watchdog has been triggered.</summary>
    public bool IsTriggered { get; private set; }

    /// <summary>
    /// Starts the watchdog timer. Call <see cref="Stop"/> when the operation completes.
    /// </summary>
    public void Start()
    {
        IsTriggered = false;
        _watchdogTask = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(_timeout, _cts.Token).ConfigureAwait(false);
                IsTriggered = true;
                _onTimeout();
            }
            catch (OperationCanceledException)
            {
                // Normal — operation completed before timeout.
            }
        });
    }

    /// <summary>
    /// Stops the watchdog timer. Call this when the monitored operation completes successfully.
    /// </summary>
    public void Stop()
    {
        _cts.Cancel();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
