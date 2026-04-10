using System.IO;
using System.Net.Sockets;
using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Results;
using HnVue.Dicom;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.Core;
using Xunit;

// DicomNetworkException used fully qualified to avoid ambiguity with IDicomService.
using DicomNetworkException = FellowOakDicom.Network.DicomNetworkException;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Unit tests for <see cref="DicomOutbox"/> covering enqueue behavior,
/// processing logic, retry, and cancellation.
/// </summary>
public sealed class DicomOutboxTests
{
    private static (DicomOutbox Outbox, IDicomService MockService) CreateOutbox()
    {
        var mockService = Substitute.For<IDicomService>();
        var outbox = new DicomOutbox(mockService, NullLogger<DicomOutbox>.Instance);
        return (outbox, mockService);
    }

    // ── EnqueueAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task EnqueueAsync_AddsItemToOutbox()
    {
        var (outbox, _) = CreateOutbox();

        await outbox.EnqueueAsync("file1.dcm");

        outbox.Count.Should().Be(1);
    }

    [Fact]
    public async Task EnqueueAsync_MultipleItems_AllAdded()
    {
        var (outbox, _) = CreateOutbox();

        await outbox.EnqueueAsync("file1.dcm");
        await outbox.EnqueueAsync("file2.dcm");
        await outbox.EnqueueAsync("file3.dcm");

        outbox.Count.Should().Be(3);
    }

    [Fact]
    public void EnqueueAsync_EmptyPath_ThrowsArgumentException()
    {
        var (outbox, _) = CreateOutbox();

        var act = async () => await outbox.EnqueueAsync(string.Empty);

        act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void EnqueueAsync_WhitespacePath_ThrowsArgumentException()
    {
        var (outbox, _) = CreateOutbox();

        var act = async () => await outbox.EnqueueAsync("   ");

        act.Should().ThrowAsync<ArgumentException>();
    }

    // ── ProcessAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_EmptyOutbox_DoesNothing()
    {
        var (outbox, mockService) = CreateOutbox();

        await outbox.ProcessAsync("PACS");

        await mockService.DidNotReceive().StoreAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        outbox.Count.Should().Be(0);
    }

    [Fact]
    public async Task ProcessAsync_SuccessfulStore_ClearsItem()
    {
        var (outbox, mockService) = CreateOutbox();
        mockService.StoreAsync("file1.dcm", "PACS", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        await outbox.EnqueueAsync("file1.dcm");
        await outbox.ProcessAsync("PACS");

        outbox.Count.Should().Be(0);
        await mockService.Received(1).StoreAsync("file1.dcm", "PACS", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_MultipleItems_AllProcessed()
    {
        var (outbox, mockService) = CreateOutbox();
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        await outbox.EnqueueAsync("a.dcm");
        await outbox.EnqueueAsync("b.dcm");
        await outbox.ProcessAsync("PACS");

        outbox.Count.Should().Be(0);
        await mockService.Received(2).StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_PermanentFailure_DiscardsItem()
    {
        var (outbox, mockService) = CreateOutbox();
        // Always fail → retries exhausted → dead-letter discard
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DicomStoreFailed, "server down"));

        await outbox.EnqueueAsync("bad.dcm");

        // No exception should propagate; item should be discarded after retries.
        await outbox.ProcessAsync("PACS");

        outbox.Count.Should().Be(0);
        // Called once for the initial attempt plus up to 3 retries = 4 total.
        await mockService.Received(4).StoreAsync("bad.dcm", "PACS", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_Cancellation_RequeuesCurrentItem()
    {
        var (outbox, mockService) = CreateOutbox();
        using var cts = new CancellationTokenSource();

        // Cancel immediately when StoreAsync is called.
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result>(new OperationCanceledException()));

        await cts.CancelAsync();
        await outbox.EnqueueAsync("x.dcm");

        // Should not throw; item should be re-queued.
        await outbox.ProcessAsync("PACS", cts.Token);

        outbox.Count.Should().Be(1);
    }

    // ── Count property ────────────────────────────────────────────────────────

    [Fact]
    public void Count_InitiallyZero()
    {
        var (outbox, _) = CreateOutbox();
        outbox.Count.Should().Be(0);
    }

    // ── EnqueueAsync — Successful operation verification ────────────────────

    [Fact]
    [Trait("SWR", "SWR-DICOM-020")]
    public async Task EnqueueAsync_ValidPath_IncrementsCount()
    {
        var (outbox, _) = CreateOutbox();

        outbox.Count.Should().Be(0);
        await outbox.EnqueueAsync("scan001.dcm");
        outbox.Count.Should().Be(1);
        await outbox.EnqueueAsync("scan002.dcm");
        outbox.Count.Should().Be(2);
    }

    [Fact]
    [Trait("SWR", "SWR-DICOM-020")]
    public void EnqueueAsync_NullPath_ThrowsArgumentException()
    {
        var (outbox, _) = CreateOutbox();

        var act = async () => await outbox.EnqueueAsync(null!);

        act.Should().ThrowAsync<ArgumentException>();
    }

    // ── Retry policy behavior (exponential backoff) ─────────────────────────

    [Fact]
    [Trait("SWR", "SWR-DICOM-020")]
    public async Task ProcessAsync_TransientFailure_ThenSuccess_RetriesAndSucceeds()
    {
        var (outbox, mockService) = CreateOutbox();

        // Fail once, then succeed. Polly should retry and ultimately deliver.
        var callCount = 0;
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                callCount++;
                return callCount <= 1
                    ? Result.Failure(ErrorCode.DicomStoreFailed, "transient")
                    : Result.Success();
            });

        await outbox.EnqueueAsync("retry.dcm");
        await outbox.ProcessAsync("PACS");

        outbox.Count.Should().Be(0);
        callCount.Should().BeGreaterThanOrEqualTo(2);
        await mockService.ReceivedWithAnyArgs().StoreAsync(default!, default!, default);
    }

    [Fact]
    [Trait("SWR", "SWR-DICOM-020")]
    public async Task ProcessAsync_AlwaysFail_ExhaustsThreeRetries()
    {
        var (outbox, mockService) = CreateOutbox();

        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DicomStoreFailed, "permanent"));

        await outbox.EnqueueAsync("fail.dcm");
        await outbox.ProcessAsync("PACS");

        // 1 initial + 3 retries = 4 calls
        await mockService.Received(4).StoreAsync("fail.dcm", "PACS", Arg.Any<CancellationToken>());
        outbox.Count.Should().Be(0); // dead-lettered
    }

    // ── Dead-letter logging when retries exhausted ──────────────────────────

    [Fact]
    [Trait("SWR", "SWR-DICOM-020")]
    public async Task ProcessAsync_DeadLetter_LogsErrorAndDiscards()
    {
        var mockService = Substitute.For<IDicomService>();
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DicomStoreFailed, "server unreachable"));

        var logger = new SpyLogger();
        var outbox = new DicomOutbox(mockService, logger);

        await outbox.EnqueueAsync("dead.dcm");
        await outbox.ProcessAsync("PACS");

        outbox.Count.Should().Be(0);
        logger.HasLogged(LogLevel.Error, "permanently failed").Should().BeTrue();
    }

    // ── Multiple items processed sequentially ───────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-DICOM-020")]
    public async Task ProcessAsync_MixedSuccessAndFailure_ProcessesAllItems()
    {
        var (outbox, mockService) = CreateOutbox();

        var callIndex = 0;
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                callIndex++;
                // First item succeeds, second always fails (dead-lettered), third succeeds.
                return callIndex switch
                {
                    2 or 3 or 4 or 5 => Result.Failure(ErrorCode.DicomStoreFailed, "fail"),
                    _ => Result.Success(),
                };
            });

        await outbox.EnqueueAsync("ok1.dcm");
        await outbox.EnqueueAsync("bad.dcm");
        await outbox.EnqueueAsync("ok2.dcm");
        await outbox.ProcessAsync("PACS");

        // ok1 succeeds (1 call), bad fails (4 calls = 1 + 3 retries), ok2 succeeds (1 call)
        outbox.Count.Should().Be(0);
    }

    // ── Network-specific exception handling ─────────────────────────────────
    // Polly retries on DicomNetworkException, IOException, and SocketException,
    // but the outer catch only handles InvalidOperationException and OperationCanceledException.
    // When all retries exhaust for non-InvalidOperationException types, the exception propagates.

    [Fact]
    [Trait("SWR", "SWR-DICOM-020")]
    public async Task ProcessAsync_DicomNetworkException_RetriesAndPropagates()
    {
        // DicomNetworkException is retried by Polly but not caught after retries exhaust,
        // so it propagates from ProcessAsync.
        var mockService = Substitute.For<IDicomService>();
        var callCount = 0;
        Func<CallInfo, Task<Result>> throwNetwork = _ =>
        {
            callCount++;
            throw new DicomNetworkException("connection refused");
        };
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(throwNetwork);

        var logger = new SpyLogger();
        var outbox = new DicomOutbox(mockService, logger);

        await outbox.EnqueueAsync("netfail.dcm");

        var act = async () => await outbox.ProcessAsync("PACS");

        // Polly retries 3 times (4 total calls), then the DicomNetworkException propagates.
        await act.Should().ThrowAsync<DicomNetworkException>();
        callCount.Should().Be(4);
    }

    [Fact]
    [Trait("SWR", "SWR-DICOM-020")]
    public async Task ProcessAsync_SocketException_RetriesAndPropagates()
    {
        var mockService = Substitute.For<IDicomService>();
        var callCount = 0;
        Func<CallInfo, Task<Result>> throwSocket = _ =>
        {
            callCount++;
            throw new SocketException((int)SocketError.ConnectionRefused);
        };
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(throwSocket);

        var logger = new SpyLogger();
        var outbox = new DicomOutbox(mockService, logger);

        await outbox.EnqueueAsync("sockfail.dcm");

        var act = async () => await outbox.ProcessAsync("PACS");

        await act.Should().ThrowAsync<SocketException>();
        callCount.Should().Be(4);
    }

    [Fact]
    [Trait("SWR", "SWR-DICOM-020")]
    public async Task ProcessAsync_IOException_RetriesAndPropagates()
    {
        var mockService = Substitute.For<IDicomService>();
        var callCount = 0;
        Func<CallInfo, Task<Result>> throwIO = _ =>
        {
            callCount++;
            throw new IOException("disk error");
        };
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(throwIO);

        var logger = new SpyLogger();
        var outbox = new DicomOutbox(mockService, logger);

        await outbox.EnqueueAsync("iofail.dcm");

        var act = async () => await outbox.ProcessAsync("PACS");

        await act.Should().ThrowAsync<IOException>();
        callCount.Should().Be(4);
    }

    [Fact]
    [Trait("SWR", "SWR-DICOM-020")]
    public async Task ProcessAsync_DicomNetworkException_ThenSuccess_Recovers()
    {
        var (outbox, mockService) = CreateOutbox();
        var callCount = 0;
        Func<CallInfo, Task<Result>> recover = _ =>
        {
            callCount++;
            if (callCount <= 2)
                throw new DicomNetworkException("timeout");
            return Task.FromResult(Result.Success());
        };
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(recover);

        await outbox.EnqueueAsync("recover.dcm");
        await outbox.ProcessAsync("PACS");

        outbox.Count.Should().Be(0);
        callCount.Should().Be(3);
    }

    // ── Cancellation during retry ───────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-DICOM-020")]
    public async Task ProcessAsync_PreCancelledToken_DoesNotProcessItems()
    {
        var mockService = Substitute.For<IDicomService>();

        var logger = new SpyLogger();
        var outbox = new DicomOutbox(mockService, logger);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await outbox.EnqueueAsync("cancel.dcm");

        // With pre-cancelled token, the while loop condition (!cancellationToken.IsCancellationRequested)
        // is immediately false, so no processing occurs.
        await outbox.ProcessAsync("PACS", cts.Token);

        // Item remains in queue since ProcessAsync never dequeued it.
        outbox.Count.Should().Be(1);
        await mockService.DidNotReceive().StoreAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ── Spy logger for verifying log output ─────────────────────────────────

    private sealed class SpyLogger : ILogger<DicomOutbox>
    {
        private readonly List<(LogLevel Level, string Message)> _entries = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _entries.Add((logLevel, formatter(state, exception)));
        }

        public bool HasLogged(LogLevel level, string substring) =>
            _entries.Any(e => e.Level == level && e.Message.Contains(substring, StringComparison.OrdinalIgnoreCase));
    }
}
