using System.Collections.Concurrent;
using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dicom;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.Core;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Unit tests for <see cref="AsyncStorePipeline"/> covering enqueue, processing,
/// status tracking, retry logic, completion events, and lifecycle management.
/// </summary>
public sealed class AsyncStorePipelineTests
{
    private static (AsyncStorePipeline Pipeline, IDicomService MockService) CreatePipeline(int capacity = 100)
    {
        var mockService = Substitute.For<IDicomService>();
        var pipeline = new AsyncStorePipeline(
            mockService,
            "PACS",
            NullLogger<AsyncStorePipeline>.Instance,
            capacity);
        return (pipeline, mockService);
    }

    // ── Construction validation ────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullService_ThrowsArgumentNullException()
    {
        var act = () => new AsyncStorePipeline(null!, "PACS", NullLogger<AsyncStorePipeline>.Instance);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_EmptyAeTitle_ThrowsArgumentException()
    {
        var mockService = Substitute.For<IDicomService>();
        var act = () => new AsyncStorePipeline(mockService, "", NullLogger<AsyncStorePipeline>.Instance);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ZeroCapacity_ThrowsArgumentOutOfRangeException()
    {
        var mockService = Substitute.For<IDicomService>();
        var act = () => new AsyncStorePipeline(mockService, "PACS", NullLogger<AsyncStorePipeline>.Instance, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── Enqueue ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EnqueueAsync_SingleItem_StatusIsPending()
    {
        var (pipeline, mockService) = CreatePipeline();
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        await pipeline.EnqueueAsync("file1.dcm", "1.2.3.4.5");

        pipeline.GetStatus("1.2.3.4.5").Should().Be(StoreStatus.Pending);
    }

    [Fact]
    public async Task EnqueueAsync_DuplicateSopUid_ThrowsArgumentException()
    {
        var (pipeline, _) = CreatePipeline();

        await pipeline.EnqueueAsync("file1.dcm", "1.2.3.4.5");

        var act = async () => await pipeline.EnqueueAsync("file2.dcm", "1.2.3.4.5");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task EnqueueAsync_EmptyFilePath_ThrowsArgumentException()
    {
        var (pipeline, _) = CreatePipeline();

        var act = async () => await pipeline.EnqueueAsync("", "1.2.3.4.5");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task EnqueueAsync_EmptySopUid_ThrowsArgumentException()
    {
        var (pipeline, _) = CreatePipeline();

        var act = async () => await pipeline.EnqueueAsync("file.dcm", "");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ── Enqueue + Process single item ──────────────────────────────────────────

    [Fact]
    public async Task StartAsync_ProcessesSingleItem_StatusBecomesSent()
    {
        var (pipeline, mockService) = CreatePipeline();
        mockService.StoreAsync("file1.dcm", "PACS", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await pipeline.EnqueueAsync("file1.dcm", "1.2.3.4.5");
        await pipeline.StartAsync(cts.Token);

        // Wait for processing to complete
        await WaitForStatusAsync(pipeline, "1.2.3.4.5", StoreStatus.Sent, cts.Token);

        pipeline.GetStatus("1.2.3.4.5").Should().Be(StoreStatus.Sent);
        await mockService.Received(1).StoreAsync("file1.dcm", "PACS", Arg.Any<CancellationToken>());

        await pipeline.StopAsync(cts.Token);
    }

    // ── Multiple items in sequence ─────────────────────────────────────────────

    [Fact]
    public async Task StartAsync_ProcessesMultipleItems_AllBecomeSent()
    {
        var (pipeline, mockService) = CreatePipeline();
        mockService.StoreAsync(Arg.Any<string>(), "PACS", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        await pipeline.EnqueueAsync("file1.dcm", "uid-1");
        await pipeline.EnqueueAsync("file2.dcm", "uid-2");
        await pipeline.EnqueueAsync("file3.dcm", "uid-3");

        await pipeline.StartAsync(cts.Token);

        await WaitForStatusAsync(pipeline, "uid-1", StoreStatus.Sent, cts.Token);
        await WaitForStatusAsync(pipeline, "uid-2", StoreStatus.Sent, cts.Token);
        await WaitForStatusAsync(pipeline, "uid-3", StoreStatus.Sent, cts.Token);

        pipeline.GetStatus("uid-1").Should().Be(StoreStatus.Sent);
        pipeline.GetStatus("uid-2").Should().Be(StoreStatus.Sent);
        pipeline.GetStatus("uid-3").Should().Be(StoreStatus.Sent);

        await pipeline.StopAsync(cts.Token);
    }

    // ── Status tracking (Pending -> Sending -> Sent) ───────────────────────────

    [Fact]
    public async Task GetStatus_UnknownUid_ReturnsNull()
    {
        var (pipeline, _) = CreatePipeline();

        pipeline.GetStatus("nonexistent").Should().BeNull();
    }

    [Fact]
    public async Task GetAllPending_ReturnsOnlyNonSentItems()
    {
        var (pipeline, mockService) = CreatePipeline();

        // Control the timing: first call succeeds, second blocks until we signal.
        var tcs = new TaskCompletionSource<bool>();
        var callCount = 0;
        mockService.StoreAsync(Arg.Any<string>(), "PACS", Arg.Any<CancellationToken>())
            .Returns(async callInfo =>
            {
                callCount++;
                if (callCount == 1)
                    return Result.Success();
                // Second call waits so item stays in non-Sent state
                await tcs.Task;
                return Result.Success();
            });

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        await pipeline.EnqueueAsync("file1.dcm", "uid-1");
        await pipeline.EnqueueAsync("file2.dcm", "uid-2");

        await pipeline.StartAsync(cts.Token);

        // Wait for first item to be Sent
        await WaitForStatusAsync(pipeline, "uid-1", StoreStatus.Sent, cts.Token);

        // At this point uid-2 is either Pending or Sending (blocked)
        var pending = pipeline.GetAllPending();
        pending.Should().ContainSingle(i => i.SopInstanceUid == "uid-2");

        tcs.SetResult(true);
        await pipeline.StopAsync(cts.Token);
    }

    // ── Retry on transient failure ─────────────────────────────────────────────

    [Fact]
    public async Task ProcessItem_TransientFailure_ThenSuccess_RetriesAndSucceeds()
    {
        var (pipeline, mockService) = CreatePipeline();
        var callCount = 0;
        mockService.StoreAsync("retry.dcm", "PACS", Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                callCount++;
                return callCount <= 1
                    ? Result.Failure(ErrorCode.DicomStoreFailed, "transient")
                    : Result.Success();
            });

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        await pipeline.EnqueueAsync("retry.dcm", "uid-retry");
        await pipeline.StartAsync(cts.Token);

        await WaitForStatusAsync(pipeline, "uid-retry", StoreStatus.Sent, cts.Token);

        pipeline.GetStatus("uid-retry").Should().Be(StoreStatus.Sent);
        callCount.Should().BeGreaterThanOrEqualTo(2);

        await pipeline.StopAsync(cts.Token);
    }

    // ── Final failure after max retries ────────────────────────────────────────

    [Fact]
    [Trait("Category", "Slow")]
    public async Task ProcessItem_AlwaysFail_MarksAsFailed()
    {
        var (pipeline, mockService) = CreatePipeline();
        mockService.StoreAsync(Arg.Any<string>(), "PACS", Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DicomStoreFailed, "permanent failure"));

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));

        await pipeline.EnqueueAsync("fail.dcm", "uid-fail");
        await pipeline.StartAsync(cts.Token);

        await WaitForStatusAsync(pipeline, "uid-fail", StoreStatus.Failed, cts.Token);

        pipeline.GetStatus("uid-fail").Should().Be(StoreStatus.Failed);

        // Item is NOT discarded -- remains in status map
        var pending = pipeline.GetAllPending();
        pending.Should().Contain(i => i.SopInstanceUid == "uid-fail");

        // 1 initial + 3 retries = 4 total calls
        await mockService.Received(4).StoreAsync("fail.dcm", "PACS", Arg.Any<CancellationToken>());

        await pipeline.StopAsync(cts.Token);
    }

    // ── StoreCompleted event firing ────────────────────────────────────────────

    [Fact]
    public async Task ProcessItem_Success_FiresStoreCompletedEvent()
    {
        var (pipeline, mockService) = CreatePipeline();
        mockService.StoreAsync(Arg.Any<string>(), "PACS", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var events = new List<StoreCompletedEventArgs>();
        pipeline.StoreCompleted += (_, e) => events.Add(e);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        await pipeline.EnqueueAsync("event.dcm", "uid-event");
        await pipeline.StartAsync(cts.Token);

        await WaitForStatusAsync(pipeline, "uid-event", StoreStatus.Sent, cts.Token);

        events.Should().ContainSingle(e => e.Success && e.FilePath == "event.dcm");

        await pipeline.StopAsync(cts.Token);
    }

    [Fact]
    [Trait("Category", "Slow")]
    public async Task ProcessItem_Failure_FiresStoreCompletedEvent()
    {
        var (pipeline, mockService) = CreatePipeline();
        mockService.StoreAsync(Arg.Any<string>(), "PACS", Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DicomStoreFailed, "permanent"));

        var events = new List<StoreCompletedEventArgs>();
        pipeline.StoreCompleted += (_, e) => events.Add(e);

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));

        await pipeline.EnqueueAsync("failevent.dcm", "uid-failevent");
        await pipeline.StartAsync(cts.Token);

        await WaitForStatusAsync(pipeline, "uid-failevent", StoreStatus.Failed, cts.Token);

        var evt = events.Should().ContainSingle(e => !e.Success && e.FilePath == "failevent.dcm").Subject;
        evt.ErrorMessage.Should().Contain("permanent");
        evt.Attempts.Should().Be(4); // 1 initial + 3 retries

        await pipeline.StopAsync(cts.Token);
    }

    [Fact]
    public async Task ProcessItem_Success_EventContainsAttemptsAndDuration()
    {
        var (pipeline, mockService) = CreatePipeline();
        mockService.StoreAsync(Arg.Any<string>(), "PACS", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var events = new List<StoreCompletedEventArgs>();
        pipeline.StoreCompleted += (_, e) => events.Add(e);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        await pipeline.EnqueueAsync("metrics.dcm", "uid-metrics");
        await pipeline.StartAsync(cts.Token);

        await WaitForStatusAsync(pipeline, "uid-metrics", StoreStatus.Sent, cts.Token);

        var evt = events.Should().ContainSingle().Subject;
        evt.Attempts.Should().Be(1);
        evt.Duration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);

        await pipeline.StopAsync(cts.Token);
    }

    // ── StopAsync drains remaining items ───────────────────────────────────────

    [Fact]
    public async Task StopAsync_DrainsRemainingItems()
    {
        var (pipeline, mockService) = CreatePipeline();

        // Make StoreAsync succeed with a small delay to simulate work
        mockService.StoreAsync(Arg.Any<string>(), "PACS", Arg.Any<CancellationToken>())
            .Returns(async callInfo =>
            {
                await Task.Delay(50);
                return Result.Success();
            });

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Enqueue 5 items, start, then stop immediately
        for (int i = 0; i < 5; i++)
            await pipeline.EnqueueAsync($"drain-{i}.dcm", $"uid-drain-{i}");

        await pipeline.StartAsync(cts.Token);
        await pipeline.StopAsync(cts.Token);

        // All items should be processed
        await mockService.Received(5).StoreAsync(Arg.Any<string>(), "PACS", Arg.Any<CancellationToken>());
    }

    // ── Channel capacity limit ─────────────────────────────────────────────────

    [Fact]
    public async Task EnqueueAsync_ExceedsCapacity_BlocksUntilSpaceAvailable()
    {
        const int capacity = 2;
        var (pipeline, mockService) = CreatePipeline(capacity);

        // Block processing so the channel fills up
        var blockTcs = new TaskCompletionSource<bool>();
        mockService.StoreAsync(Arg.Any<string>(), "PACS", Arg.Any<CancellationToken>())
            .Returns(async callInfo =>
            {
                await blockTcs.Task;
                return Result.Success();
            });

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        await pipeline.EnqueueAsync("cap-0.dcm", "uid-cap-0");
        await pipeline.EnqueueAsync("cap-1.dcm", "uid-cap-1");

        // Channel is now full. Start the consumer so it can drain eventually.
        await pipeline.StartAsync(cts.Token);

        // This third enqueue should block until the consumer reads an item.
        var enqueueTask = pipeline.EnqueueAsync("cap-2.dcm", "uid-cap-2");

        // Release the block so processing can proceed
        blockTcs.SetResult(true);

        await enqueueTask; // Should complete now

        // Wait for all items to be processed
        await WaitForStatusAsync(pipeline, "uid-cap-2", StoreStatus.Sent, cts.Token);

        pipeline.GetStatus("uid-cap-0").Should().Be(StoreStatus.Sent);
        pipeline.GetStatus("uid-cap-1").Should().Be(StoreStatus.Sent);
        pipeline.GetStatus("uid-cap-2").Should().Be(StoreStatus.Sent);

        await pipeline.StopAsync(cts.Token);
    }

    // ── Start twice throws ─────────────────────────────────────────────────────

    [Fact]
    public async Task StartAsync_CalledTwice_ThrowsInvalidOperationException()
    {
        var (pipeline, mockService) = CreatePipeline();
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        await pipeline.StartAsync(cts.Token);

        var act = async () => await pipeline.StartAsync(cts.Token);
        await act.Should().ThrowAsync<InvalidOperationException>();

        await pipeline.StopAsync(cts.Token);
    }

    // ── Enqueue after dispose throws ───────────────────────────────────────────

    [Fact]
    public async Task EnqueueAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var (pipeline, mockService) = CreatePipeline();
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        await pipeline.DisposeAsync();

        var act = async () => await pipeline.EnqueueAsync("disposed.dcm", "uid-disposed");
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    // ── GetStatus for nonexistent UID ──────────────────────────────────────────

    [Fact]
    public void GetStatus_NeverEnqueued_ReturnsNull()
    {
        var (pipeline, _) = CreatePipeline();
        pipeline.GetStatus("does-not-exist").Should().BeNull();
    }

    // ── PendingCount reflects non-sent items ───────────────────────────────────

    [Fact]
    public async Task PendingCount_ReflectsNonSentItems()
    {
        var (pipeline, mockService) = CreatePipeline();
        mockService.StoreAsync(Arg.Any<string>(), "PACS", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        await pipeline.EnqueueAsync("count1.dcm", "uid-count1");
        await pipeline.EnqueueAsync("count2.dcm", "uid-count2");

        pipeline.PendingCount.Should().Be(2);

        await pipeline.StartAsync(cts.Token);

        await WaitForStatusAsync(pipeline, "uid-count1", StoreStatus.Sent, cts.Token);
        await WaitForStatusAsync(pipeline, "uid-count2", StoreStatus.Sent, cts.Token);

        pipeline.PendingCount.Should().Be(0);

        await pipeline.StopAsync(cts.Token);
    }

    // ── Helper ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Polls <see cref="AsyncStorePipeline.GetStatus"/> until the expected status is reached
    /// or the cancellation token fires.
    /// </summary>
    private static async Task WaitForStatusAsync(
        AsyncStorePipeline pipeline,
        string sopInstanceUid,
        StoreStatus expected,
        CancellationToken ct)
    {
        var timeout = Task.Delay(TimeSpan.FromMinutes(1), ct);
        while (pipeline.GetStatus(sopInstanceUid) != expected)
        {
            if (timeout.IsCompleted)
                throw new TimeoutException($"Timed out waiting for status {expected} on {sopInstanceUid}.");

            await Task.Delay(50, ct).ConfigureAwait(false);
        }
    }
}
