using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using HnVue.Common.Abstractions;
using HnVue.Common.Results;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace HnVue.Dicom;

/// <summary>
/// Resilient in-memory outbox for DICOM C-STORE operations.
/// Enqueued file paths are processed with exponential-backoff retry via Polly.
/// </summary>
/// <remarks>
/// This is a lightweight helper component, not a hosted background service.
/// Call <see cref="ProcessAsync"/> from the host's background loop or a dedicated task.
/// </remarks>
public sealed partial class DicomOutbox
{
    private readonly ConcurrentQueue<string> _pending = new();
    private readonly IDicomService _dicomService;
    private readonly ILogger<DicomOutbox> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    /// <summary>
    /// Initializes a new instance of <see cref="DicomOutbox"/>.
    /// </summary>
    /// <param name="dicomService">Service used to perform the actual C-STORE.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public DicomOutbox(IDicomService dicomService, ILogger<DicomOutbox> logger)
    {
        _dicomService = dicomService;
        _logger = logger;

        // Exponential backoff: 3 retries with 2 s, 4 s, 8 s delays.
        // Issue #26: Added DicomNetworkException and IOException to retry on transient network faults.
        _retryPolicy = Policy
            .Handle<InvalidOperationException>()
            .Or<FellowOakDicom.Network.DicomNetworkException>()
            .Or<IOException>()
            .Or<System.Net.Sockets.SocketException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, delay, attempt, _) =>
                    LogRetry(_logger, attempt, delay.TotalSeconds, exception));
    }

    /// <summary>Gets the current number of pending items in the outbox.</summary>
    public int Count => _pending.Count;

    /// <summary>
    /// Adds a DICOM file path to the outbox for eventual delivery.
    /// </summary>
    /// <param name="dicomFilePath">Absolute path to the DICOM file to store.</param>
    public ValueTask EnqueueAsync(string dicomFilePath)
    {
        if (string.IsNullOrWhiteSpace(dicomFilePath))
            throw new ArgumentException("File path must not be empty.", nameof(dicomFilePath));

        _pending.Enqueue(dicomFilePath);
        LogEnqueued(_logger, dicomFilePath, _pending.Count);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Processes all pending items, calling <see cref="IDicomService.StoreAsync"/>
    /// for each with Polly exponential-backoff retry.
    /// Items that still fail after all retries are logged and discarded (dead-letter).
    /// </summary>
    /// <param name="pacsAeTitle">Called AE title of the PACS to receive the files.</param>
    /// <param name="cancellationToken">Token to cancel processing.</param>
    public async Task ProcessAsync(string pacsAeTitle, CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested && _pending.TryDequeue(out var filePath))
        {
            LogProcessing(_logger, filePath);

            try
            {
                await _retryPolicy.ExecuteAsync(async ct =>
                {
                    var result = await _dicomService.StoreAsync(filePath, pacsAeTitle, ct).ConfigureAwait(false);
                    if (result.IsFailure)
                        throw new InvalidOperationException(result.ErrorMessage ?? "Store failed.");
                }, cancellationToken).ConfigureAwait(false);

                LogDelivered(_logger, filePath);
            }
            catch (OperationCanceledException)
            {
                // Re-queue and stop to preserve the item for the next processing cycle.
                _pending.Enqueue(filePath);
                LogCancelled(_logger, filePath);
                break;
            }
            catch (InvalidOperationException ex)
            {
                // Dead-letter: all retries exhausted.
                LogDeadLetter(_logger, ex, filePath);
            }
        }
    }

    // ── LoggerMessage definitions (CA1848 compliance) ─────────────────────────

    [LoggerMessage(Level = LogLevel.Warning, Message = "Outbox retry {Attempt}/3 after {DelaySecs}s.")]
    private static partial void LogRetry(ILogger logger, int attempt, double delaySecs, Exception ex);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Enqueued {FilePath}. Queue depth: {Depth}.")]
    private static partial void LogEnqueued(ILogger logger, string filePath, int depth);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Processing outbox item: {FilePath}.")]
    private static partial void LogProcessing(ILogger logger, string filePath);

    [LoggerMessage(Level = LogLevel.Information, Message = "Outbox delivered {FilePath}.")]
    private static partial void LogDelivered(ILogger logger, string filePath);

    [LoggerMessage(Level = LogLevel.Information, Message = "Processing cancelled; re-queued {FilePath}.")]
    private static partial void LogCancelled(ILogger logger, string filePath);

    [LoggerMessage(Level = LogLevel.Error, Message = "Outbox permanently failed for {FilePath}. Item discarded.")]
    private static partial void LogDeadLetter(ILogger logger, Exception ex, string filePath);
}
