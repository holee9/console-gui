using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading.Channels;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace HnVue.Dicom;

/// <summary>
/// Channel-based async pipeline for DICOM C-STORE operations with per-item status tracking,
/// Polly retry, and completion events. Coexists alongside <see cref="DicomOutbox"/>.
/// </summary>
public sealed partial class AsyncStorePipeline : IAsyncDisposable
{
    private readonly Channel<StoreItem> _channel;
    private readonly ConcurrentDictionary<string, StoreItem> _statusMap = new(StringComparer.Ordinal);
    private readonly IDicomService _dicomService;
    private readonly ILogger<AsyncStorePipeline> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly string _pacsAeTitle;

    private Task? _consumerTask;
    private CancellationTokenSource? _consumerCts;
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="AsyncStorePipeline"/>.
    /// </summary>
    /// <param name="dicomService">Service used to perform C-STORE operations.</param>
    /// <param name="pacsAeTitle">Called AE title of the destination PACS.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="capacity">Maximum number of items the channel can hold (default: 100).</param>
    public AsyncStorePipeline(
        IDicomService dicomService,
        string pacsAeTitle,
        ILogger<AsyncStorePipeline> logger,
        int capacity = 100)
    {
        ArgumentNullException.ThrowIfNull(dicomService);
        ArgumentException.ThrowIfNullOrWhiteSpace(pacsAeTitle);
        ArgumentNullException.ThrowIfNull(logger);

        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");

        _dicomService = dicomService;
        _pacsAeTitle = pacsAeTitle;
        _logger = logger;

        _channel = Channel.CreateBounded<StoreItem>(
            new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false,
            });

        // Exponential backoff: 3 retries with 2 s, 4 s, 8 s delays.
        _retryPolicy = Policy
            .Handle<InvalidOperationException>()
            .Or<FellowOakDicom.Network.DicomNetworkException>()
            .Or<IOException>()
            .Or<SocketException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, delay, attempt, _) =>
                    LogRetry(_logger, attempt, delay.TotalSeconds, exception));
    }

    /// <summary>
    /// Raised when a C-STORE operation completes (success or final failure).
    /// </summary>
    public event EventHandler<StoreCompletedEventArgs>? StoreCompleted;

    /// <summary>Gets the number of items currently waiting in the channel.</summary>
    public int PendingCount => _statusMap.Count(kvp => kvp.Value.Status is StoreStatus.Pending or StoreStatus.Sending or StoreStatus.Retrying);

    /// <summary>
    /// Gets the current status of a store item by its SOP Instance UID.
    /// </summary>
    /// <param name="sopInstanceUid">The SOP Instance UID to look up.</param>
    /// <returns>The current status, or <see langword="null"/> if the UID is not tracked.</returns>
    public StoreStatus? GetStatus(string sopInstanceUid)
    {
        return _statusMap.TryGetValue(sopInstanceUid, out var item) ? item.Status : null;
    }

    /// <summary>
    /// Returns all items that have not yet been successfully sent.
    /// Includes items in Pending, Sending, Retrying, and Failed states.
    /// </summary>
    public IReadOnlyList<StoreItem> GetAllPending()
    {
        return _statusMap.Values
            .Where(i => i.Status != StoreStatus.Sent)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Starts the background consumer task that reads from the channel and performs C-STORE.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for pipeline shutdown.</param>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed != 0, this);

        if (_consumerTask is not null)
            throw new InvalidOperationException("Pipeline is already running.");

        _consumerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _consumerTask = Task.Run(() => ConsumeLoopAsync(_consumerCts.Token), _consumerCts.Token);

        LogStarted(_logger);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Signals the channel to complete and waits for all remaining items to be drained.
    /// </summary>
    /// <param name="cancellationToken">Token to abort the drain wait.</param>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed != 0, this);

        if (_consumerTask is null)
            return;

        _channel.Writer.TryComplete();
        LogStopping(_logger, _statusMap.Count);

        try
        {
            await _consumerTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown.
        }
        finally
        {
            await DisposeCoreAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Enqueues a DICOM file for asynchronous C-STORE transmission.
    /// </summary>
    /// <param name="filePath">Absolute path to the DICOM file.</param>
    /// <param name="sopInstanceUid">SOP Instance UID of the DICOM object.</param>
    /// <exception cref="ArgumentException">Thrown when arguments are empty or the SOP UID is already tracked.</exception>
    public async ValueTask EnqueueAsync(string filePath, string sopInstanceUid)
    {
        ObjectDisposedException.ThrowIf(_disposed != 0, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(sopInstanceUid);

        var item = new StoreItem(filePath, sopInstanceUid);

        if (!_statusMap.TryAdd(sopInstanceUid, item))
            throw new ArgumentException($"SOP Instance UID '{sopInstanceUid}' is already enqueued.", nameof(sopInstanceUid));

        await _channel.Writer.WriteAsync(item).ConfigureAwait(false);
        LogEnqueued(_logger, filePath, sopInstanceUid);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        await DisposeCoreAsync().ConfigureAwait(false);
    }

    // ── Consumer loop ────────────────────────────────────────────────────────

    private async Task ConsumeLoopAsync(CancellationToken cancellationToken)
    {
        await foreach (var item in _channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            await ProcessItemAsync(item, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ProcessItemAsync(StoreItem item, CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        UpdateStatus(item.SopInstanceUid, StoreStatus.Sending);

        string? lastError = null;
        var totalAttempts = 0;

        try
        {
            await _retryPolicy.ExecuteAsync(async ct =>
            {
                totalAttempts++;
                UpdateStatus(item.SopInstanceUid, StoreStatus.Retrying, totalAttempts);

                var result = await _dicomService.StoreAsync(item.FilePath, _pacsAeTitle, ct).ConfigureAwait(false);
                if (result.IsFailure)
                    throw new InvalidOperationException(result.ErrorMessage ?? "C-STORE failed.");
            }, cancellationToken).ConfigureAwait(false);

            UpdateStatus(item.SopInstanceUid, StoreStatus.Sent, totalAttempts);
            LogDelivered(_logger, item.FilePath, item.SopInstanceUid);

            RaiseStoreCompleted(item, success: true, errorMessage: null, totalAttempts, startedAt);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Pipeline is shutting down. Re-mark as Pending so callers know it was not sent.
            UpdateStatus(item.SopInstanceUid, StoreStatus.Pending, totalAttempts);
            LogCancelled(_logger, item.FilePath, item.SopInstanceUid);
        }
        catch (Exception ex)
        {
            lastError = ex.Message;
            UpdateStatus(item.SopInstanceUid, StoreStatus.Failed, totalAttempts);
            LogFinalFailure(_logger, ex, item.FilePath, item.SopInstanceUid, totalAttempts);

            RaiseStoreCompleted(item, success: false, errorMessage: lastError, totalAttempts, startedAt);
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void UpdateStatus(string sopInstanceUid, StoreStatus newStatus, int? attempts = null)
    {
        if (_statusMap.TryGetValue(sopInstanceUid, out var current))
        {
            var updated = current with
            {
                Status = newStatus,
                Attempts = attempts ?? current.Attempts,
                LastAttemptAt = DateTimeOffset.UtcNow,
            };
            _statusMap[sopInstanceUid] = updated;
        }
    }

    private void RaiseStoreCompleted(
        StoreItem item, bool success, string? errorMessage, int attempts, DateTimeOffset startedAt)
    {
        var duration = DateTimeOffset.UtcNow - startedAt;
        var args = new StoreCompletedEventArgs(item.FilePath, success, errorMessage, attempts, duration);
        StoreCompleted?.Invoke(this, args);
    }

    private async ValueTask DisposeCoreAsync()
    {
        _channel.Writer.TryComplete();

        if (_consumerTask is not null)
        {
            try
            {
                await _consumerTask.ConfigureAwait(false);
            }
            catch
            {
                // Swallow exceptions during disposal.
            }
        }

        _consumerCts?.Dispose();
        _consumerCts = null;
        _consumerTask = null;
    }

    // ── LoggerMessage definitions (CA1848 compliance) ────────────────────────

    [LoggerMessage(Level = LogLevel.Debug, Message = "AsyncStorePipeline started.")]
    private static partial void LogStarted(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "AsyncStorePipeline stopping. Remaining items: {Remaining}.")]
    private static partial void LogStopping(ILogger logger, int remaining);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Enqueued {FilePath} (SOP: {SopUid}).")]
    private static partial void LogEnqueued(ILogger logger, string filePath, string sopUid);

    [LoggerMessage(Level = LogLevel.Information, Message = "Delivered {FilePath} (SOP: {SopUid}).")]
    private static partial void LogDelivered(ILogger logger, string filePath, string sopUid);

    [LoggerMessage(Level = LogLevel.Information, Message = "Processing cancelled; item re-pending {FilePath} (SOP: {SopUid}).")]
    private static partial void LogCancelled(ILogger logger, string filePath, string sopUid);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Pipeline retry {Attempt}/3 after {DelaySecs}s.")]
    private static partial void LogRetry(ILogger logger, int attempt, double delaySecs, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Pipeline permanently failed for {FilePath} (SOP: {SopUid}) after {Attempts} attempts.")]
    private static partial void LogFinalFailure(ILogger logger, Exception ex, string filePath, string sopUid, int attempts);
}
