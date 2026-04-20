using System.IO;
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dicom;
using IDicomService = HnVue.Common.Abstractions.IDicomService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Round 2 coverage tests for S14 targeting uncovered lines in AsyncStorePipeline
/// (cancellation path, DisposeCoreAsync body), DicomService (transient retry success,
/// retry exhaustion paths), and DicomStoreScu (nonexistent file, error messages).
/// Target: HnVue.Dicom coverage from 81.4% to 85%+.
/// </summary>
[Trait("SWR", "SWR-DC-001")]
public sealed class DicomS14CoverageRound2Tests : IDisposable
{
    private readonly List<string> _tempFiles = [];

    /// <summary>Disposes temporary DICOM files created during tests.</summary>
    public void Dispose()
    {
        foreach (var f in _tempFiles)
        {
            try { File.Delete(f); } catch { /* best effort cleanup */ }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  AsyncStorePipeline — Cancellation path (lines 219-224)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that when a cancellation is requested during ProcessItemAsync,
    /// the item status is reverted to Pending so callers know it was not sent.
    /// Covers AsyncStorePipeline lines 219-224 (OperationCanceledException handler).
    /// </summary>
    [Fact]
    public async Task Pipeline_CancelDuringProcessing_RevertsToPending()
    {
        var mockService = Substitute.For<IDicomService>();
        var pipeline = new AsyncStorePipeline(
            mockService, "PACS", NullLogger<AsyncStorePipeline>.Instance);

        // Block StoreAsync until we cancel, then throw OperationCanceledException
        var blockTcs = new TaskCompletionSource<bool>();
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                await blockTcs.Task;
                call.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.Success();
            });

        using var cts = new CancellationTokenSource();

        await pipeline.EnqueueAsync("cancel-test.dcm", "uid-cancel-1");
        await pipeline.StartAsync(cts.Token);

        // Give the consumer time to pick up the item and enter StoreAsync
        await Task.Delay(200);

        // Now cancel the pipeline
        cts.Cancel();
        blockTcs.SetResult(true); // Release the blocked StoreAsync

        // Give time for cancellation to propagate
        await Task.Delay(300);

        // The item should be reverted to Pending (not Sent, not Failed)
        var status = pipeline.GetStatus("uid-cancel-1");
        status.Should().Be(StoreStatus.Pending);

        await pipeline.DisposeAsync();
    }

    /// <summary>
    /// Verifies that DisposeAsync after StartAsync drains the channel gracefully.
    /// Covers AsyncStorePipeline lines 259-277 (DisposeCoreAsync body).
    /// </summary>
    [Fact]
    public async Task Pipeline_DisposeAfterStart_CompletesGracefully()
    {
        var mockService = Substitute.For<IDicomService>();
        var pipeline = new AsyncStorePipeline(
            mockService, "PACS", NullLogger<AsyncStorePipeline>.Instance);

        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        await pipeline.EnqueueAsync("dispose-test.dcm", "uid-dispose-1");
        await pipeline.StartAsync(cts.Token);

        // Wait for the item to be processed
        await WaitForStatusAsync(pipeline, "uid-dispose-1", StoreStatus.Sent, cts.Token);

        // Dispose should complete without throwing
        await pipeline.DisposeAsync();

        pipeline.GetStatus("uid-dispose-1").Should().Be(StoreStatus.Sent);
    }

    /// <summary>
    /// Verifies StopAsync returns immediately when pipeline was never started.
    /// Covers the early-return path in StopAsync.
    /// </summary>
    [Fact]
    public async Task Pipeline_StopWhenNotStarted_ReturnsWithoutError()
    {
        var mockService = Substitute.For<IDicomService>();
        var pipeline = new AsyncStorePipeline(
            mockService, "PACS", NullLogger<AsyncStorePipeline>.Instance);

        // Should not throw
        var act = async () => await pipeline.StopAsync();
        await act.Should().NotThrowAsync();

        await pipeline.DisposeAsync();
    }

    /// <summary>
    /// Verifies StoreCompleted fires with success=false when the service always fails.
    /// Covers RaiseStoreCompleted failure path.
    /// </summary>
    [Fact]
    [Trait("Category", "Slow")]
    public async Task Pipeline_StoreFailure_FiresStoreCompletedWithFailure()
    {
        var mockService = Substitute.For<IDicomService>();
        var pipeline = new AsyncStorePipeline(
            mockService, "PACS", NullLogger<AsyncStorePipeline>.Instance);

        // Always fail so Polly retries exhaust
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DicomStoreFailed, "permanent failure"));

        var events = new List<StoreCompletedEventArgs>();
        pipeline.StoreCompleted += (_, e) => events.Add(e);

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));

        await pipeline.EnqueueAsync("fail-event.dcm", "uid-fail-evt");
        await pipeline.StartAsync(cts.Token);

        await WaitForStatusAsync(pipeline, "uid-fail-evt", StoreStatus.Failed, cts.Token);

        var evt = events.Should().ContainSingle(e => !e.Success).Subject;
        evt.FilePath.Should().Be("fail-event.dcm");
        evt.ErrorMessage.Should().Contain("permanent failure");
        evt.Attempts.Should().Be(4); // 1 initial + 3 retries

        await pipeline.StopAsync(cts.Token);
    }

    /// <summary>
    /// Verifies that multiple event subscribers all receive the StoreCompleted event.
    /// </summary>
    [Fact]
    public async Task Pipeline_MultipleEventSubscribers_AllReceiveEvent()
    {
        var mockService = Substitute.For<IDicomService>();
        var pipeline = new AsyncStorePipeline(
            mockService, "PACS", NullLogger<AsyncStorePipeline>.Instance);

        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var subscriber1Events = new List<StoreCompletedEventArgs>();
        var subscriber2Events = new List<StoreCompletedEventArgs>();
        pipeline.StoreCompleted += (_, e) => subscriber1Events.Add(e);
        pipeline.StoreCompleted += (_, e) => subscriber2Events.Add(e);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        await pipeline.EnqueueAsync("multi-evt.dcm", "uid-multi-evt");
        await pipeline.StartAsync(cts.Token);

        await WaitForStatusAsync(pipeline, "uid-multi-evt", StoreStatus.Sent, cts.Token);

        subscriber1Events.Should().ContainSingle(e => e.Success && e.FilePath == "multi-evt.dcm");
        subscriber2Events.Should().ContainSingle(e => e.Success && e.FilePath == "multi-evt.dcm");

        await pipeline.StopAsync(cts.Token);
    }

    /// <summary>
    /// Verifies DisposeAsync while the consumer is actively processing an item.
    /// Covers DisposeCoreAsync lines 259-277 with an active consumer task.
    /// </summary>
    [Fact]
    public async Task Pipeline_DisposeDuringActiveProcessing_TerminatesGracefully()
    {
        var mockService = Substitute.For<IDicomService>();
        var pipeline = new AsyncStorePipeline(
            mockService, "PACS", NullLogger<AsyncStorePipeline>.Instance);

        // Make StoreAsync block so we can dispose while it's running
        var blockTcs = new TaskCompletionSource<bool>();
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                await blockTcs.Task;
                return Result.Success();
            });

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        await pipeline.EnqueueAsync("active-dispose.dcm", "uid-active-dispose");
        await pipeline.StartAsync(cts.Token);

        // Give the consumer time to pick up the item
        await Task.Delay(200);

        // Now dispose while processing is blocked
        var disposeTask = pipeline.DisposeAsync();

        // Release the blocked processing
        blockTcs.SetResult(true);

        // Dispose should complete without throwing
        await disposeTask;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  DicomService — StoreAsync transient retry paths
    // ═══════════════════════════════════════════════════════════════════════

    private sealed class TestableDicomService : DicomService
    {
        private readonly IDicomClient _client;

        public TestableDicomService(
            IOptions<DicomOptions> options,
            ILogger<DicomService> logger,
            IDicomClient client)
            : base(options, logger)
        {
            _client = client;
        }

        internal override IDicomClient CreateClient(
            string host, int port, string callingAeTitle, string calledAeTitle)
            => _client;
    }

    private static DicomOptions CreateTestOptions() => new()
    {
        LocalAeTitle = "HNVUE",
        PacsHost = "127.0.0.1",
        PacsPort = 104,
        PacsAeTitle = "TESTPACS",
        MwlHost = "127.0.0.1",
        MwlPort = 104,
        MwlAeTitle = "TESTMWL",
        PrinterHost = "127.0.0.1",
        PrinterPort = 104,
        PrinterAeTitle = "TESTPRINTER",
        StoreRetryCount = 2,
        StoreRetryDelayMs = 1,
    };

    private TestableDicomService CreateService(
        IDicomClient mockClient, DicomOptions? options = null)
    {
        return new TestableDicomService(
            Options.Create(options ?? CreateTestOptions()),
            NullLogger<DicomService>.Instance,
            mockClient);
    }

    private async Task<string> CreateTempDicomFileAsync()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
            { DicomTag.PatientID, "TEST001" },
            { DicomTag.PatientName, "Test^Patient" },
        };
        var dicomFile = new DicomFile(dataset);
        var tempPath = Path.Combine(Path.GetTempPath(), $"dicom_s14_r2_{Guid.NewGuid():N}.dcm");
        await dicomFile.SaveAsync(tempPath);
        _tempFiles.Add(tempPath);
        return tempPath;
    }

    /// <summary>
    /// Verifies StoreAsync retries on transient C-STORE failure (0xA700) and succeeds
    /// on the second attempt. Covers GetUserFriendlyStatus + IsTransientError + retry loop.
    /// </summary>
    [Fact]
    public async Task StoreAsync_TransientA700Failure_ThenSuccess_RetriesAndSucceeds()
    {
        var mockClient = Substitute.For<IDicomClient>();
        var capturedRequests = new List<DicomRequest>();
        var sendCallCount = 0;

        mockClient.AddRequestAsync(Arg.Any<DicomRequest>())
            .Returns(call =>
            {
                capturedRequests.Add(call.Arg<DicomRequest>());
                return Task.CompletedTask;
            });

        mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                sendCallCount++;
                var batch = capturedRequests.ToList();
                capturedRequests.Clear();

                foreach (var req in batch)
                {
                    if (req is DicomCStoreRequest cStore)
                    {
                        if (sendCallCount == 1)
                        {
                            // First attempt: transient failure (0xA700 StorageOutOfResources)
                            var failStatus = DicomStatus.StorageStorageOutOfResources;
                            cStore.OnResponseReceived?.Invoke(cStore,
                                new DicomCStoreResponse(cStore, failStatus));
                        }
                        else
                        {
                            // Second attempt: success
                            cStore.OnResponseReceived?.Invoke(cStore,
                                new DicomCStoreResponse(cStore, DicomStatus.Success));
                        }
                    }
                }

                return Task.CompletedTask;
            });

        var svc = CreateService(mockClient);
        var tempFile = await CreateTempDicomFileAsync();

        var result = await svc.StoreAsync(tempFile, "TESTPACS", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        sendCallCount.Should().Be(2);
    }

    /// <summary>
    /// Verifies StoreAsync exhausts retries on persistent transient failure.
    /// Covers IsTransientError returning true through all retries.
    /// </summary>
    [Fact]
    public async Task StoreAsync_AllRetriesExhaustedOnTransientError_ReturnsFailure()
    {
        var mockClient = Substitute.For<IDicomClient>();
        var capturedRequests = new List<DicomRequest>();
        var sendCallCount = 0;

        mockClient.AddRequestAsync(Arg.Any<DicomRequest>())
            .Returns(call =>
            {
                capturedRequests.Add(call.Arg<DicomRequest>());
                return Task.CompletedTask;
            });

        mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                sendCallCount++;
                var batch = capturedRequests.ToList();
                capturedRequests.Clear();

                foreach (var req in batch)
                {
                    if (req is DicomCStoreRequest cStore)
                    {
                        // Always return transient failure (0xFE00 Cancel — mapped to "일시적 오류")
                        var failStatus = DicomStatus.Cancel;
                        cStore.OnResponseReceived?.Invoke(cStore,
                            new DicomCStoreResponse(cStore, failStatus));
                    }
                }

                return Task.CompletedTask;
            });

        var svc = CreateService(mockClient);
        var tempFile = await CreateTempDicomFileAsync();

        var result = await svc.StoreAsync(tempFile, "TESTPACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Contain("일시적");
        // With StoreRetryCount=2: attempt 0 (transient, retry), attempt 1 (transient, retry),
        // attempt 2 (transient, no more retries)
        sendCallCount.Should().Be(3);
    }

    /// <summary>
    /// Verifies StoreAsync handles IOException and exhausts retries.
    /// Covers IOException catch block with retry exhaustion.
    /// </summary>
    [Fact]
    public async Task StoreAsync_IOExceptionExhaustsRetries_ReturnsFileReadError()
    {
        var mockClient = Substitute.For<IDicomClient>();
        var sendCallCount = 0;

        mockClient.AddRequestAsync(Arg.Any<DicomRequest>())
            .Returns(call => Task.CompletedTask);

        mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                sendCallCount++;
                throw new IOException("Disk read error");
            });

        var svc = CreateService(mockClient);
        var tempFile = await CreateTempDicomFileAsync();

        var result = await svc.StoreAsync(tempFile, "TESTPACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        result.ErrorMessage.Should().Contain("파일 읽기 오류");
        // StoreRetryCount=2: attempt 0 (retry), attempt 1 (retry), attempt 2 (exhausted)
        sendCallCount.Should().Be(3);
    }

    /// <summary>
    /// Verifies StoreAsync handles DicomNetworkException and exhausts retries.
    /// Covers DicomNetworkException catch block with retry exhaustion.
    /// </summary>
    [Fact]
    public async Task StoreAsync_DicomNetworkExceptionExhaustsRetries_ReturnsConnectionFailed()
    {
        var mockClient = Substitute.For<IDicomClient>();
        var sendCallCount = 0;

        mockClient.AddRequestAsync(Arg.Any<DicomRequest>())
            .Returns(call => Task.CompletedTask);

        mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                sendCallCount++;
                throw new DicomNetworkException("Connection refused");
            });

        var svc = CreateService(mockClient);
        var tempFile = await CreateTempDicomFileAsync();

        var result = await svc.StoreAsync(tempFile, "TESTPACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        result.ErrorMessage.Should().Contain("네트워크 오류");
        sendCallCount.Should().Be(3);
    }

    /// <summary>
    /// Verifies StoreAsync handles SocketException wrapped in a generic Exception.
    /// Covers the inner SocketException detection path (lines 133-147).
    /// </summary>
    [Fact]
    public async Task StoreAsync_SocketExceptionWrappedInGenericExhaustsRetries_ReturnsConnectionFailed()
    {
        var mockClient = Substitute.For<IDicomClient>();
        var sendCallCount = 0;

        mockClient.AddRequestAsync(Arg.Any<DicomRequest>())
            .Returns(call => Task.CompletedTask);

        mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                sendCallCount++;
                // SocketException wrapped in a generic Exception
                var inner = new System.Net.Sockets.SocketException(10061);
                throw new Exception("Send failed", inner);
            });

        var svc = CreateService(mockClient);
        var tempFile = await CreateTempDicomFileAsync();

        var result = await svc.StoreAsync(tempFile, "TESTPACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        result.ErrorMessage.Should().Contain("연결할 수 없습니다");
        sendCallCount.Should().Be(3);
    }

    /// <summary>
    /// Verifies StoreAsync returns specific error for OperationCanceledException.
    /// Covers the cancellation path.
    /// </summary>
    [Fact]
    public async Task StoreAsync_OperationCancelled_ReturnsCancelledError()
    {
        var mockClient = Substitute.For<IDicomClient>();

        mockClient.AddRequestAsync(Arg.Any<DicomRequest>())
            .Returns(call => Task.CompletedTask);

        mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                throw new OperationCanceledException("User cancelled");
            });

        var svc = CreateService(mockClient);
        var tempFile = await CreateTempDicomFileAsync();

        var result = await svc.StoreAsync(tempFile, "TESTPACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.OperationCancelled);
        result.ErrorMessage.Should().Contain("취소");
    }

    /// <summary>
    /// Verifies StoreAsync with retry count 0 returns failure immediately on transient error.
    /// Covers the no-retry path for transient errors.
    /// </summary>
    [Fact]
    public async Task StoreAsync_NoRetryTransientFailure_ReturnsImmediately()
    {
        var mockClient = Substitute.For<IDicomClient>();
        var capturedRequests = new List<DicomRequest>();

        mockClient.AddRequestAsync(Arg.Any<DicomRequest>())
            .Returns(call =>
            {
                capturedRequests.Add(call.Arg<DicomRequest>());
                return Task.CompletedTask;
            });

        mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var batch = capturedRequests.ToList();
                capturedRequests.Clear();

                foreach (var req in batch)
                {
                    if (req is DicomCStoreRequest cStore)
                    {
                        var failStatus = DicomStatus.StorageStorageOutOfResources;
                        cStore.OnResponseReceived?.Invoke(cStore,
                            new DicomCStoreResponse(cStore, failStatus));
                    }
                }

                return Task.CompletedTask;
            });

        var options = CreateTestOptions();
        options.StoreRetryCount = 0; // No retries
        var svc = CreateService(mockClient, options);
        var tempFile = await CreateTempDicomFileAsync();

        var result = await svc.StoreAsync(tempFile, "TESTPACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Contain("일시적");
    }

    /// <summary>
    /// Verifies StoreAsync handles an unexpected non-transient failure without retry.
    /// Covers the non-transient immediate return path.
    /// </summary>
    [Fact]
    public async Task StoreAsync_NonTransientCStoreFailure_ReturnsImmediatelyNoRetry()
    {
        var mockClient = Substitute.For<IDicomClient>();
        var capturedRequests = new List<DicomRequest>();
        var sendCallCount = 0;

        mockClient.AddRequestAsync(Arg.Any<DicomRequest>())
            .Returns(call =>
            {
                capturedRequests.Add(call.Arg<DicomRequest>());
                return Task.CompletedTask;
            });

        mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                sendCallCount++;
                var batch = capturedRequests.ToList();
                capturedRequests.Clear();

                foreach (var req in batch)
                {
                    if (req is DicomCStoreRequest cStore)
                    {
                        // StorageCannotUnderstand (0xC000) — not transient, no retry
                        var failStatus = DicomStatus.StorageCannotUnderstand;
                        cStore.OnResponseReceived?.Invoke(cStore,
                            new DicomCStoreResponse(cStore, failStatus));
                    }
                }

                return Task.CompletedTask;
            });

        var svc = CreateService(mockClient);
        var tempFile = await CreateTempDicomFileAsync();

        var result = await svc.StoreAsync(tempFile, "TESTPACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Contain("오류");
        // Non-transient: should NOT retry
        sendCallCount.Should().Be(1);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  DicomStoreScu — Error message coverage (GetUserFriendlyStatus branches)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies DicomStoreScu.StoreAsync returns failure for nonexistent file.
    /// Covers the file-existence check path.
    /// </summary>
    [Fact]
    public async Task DicomStoreScu_NonexistentFile_ReturnsFailureWithKoreanMessage()
    {
        var config = Substitute.For<IDicomNetworkConfig>();
        config.PacsHost.Returns("127.0.0.1");
        config.PacsPort.Returns(104);
        config.LocalAeTitle.Returns("HNVUE");
        config.PacsAeTitle.Returns("TESTPACS");

        var scu = new DicomStoreScu(config);

        var result = await scu.StoreAsync("/nonexistent/path/test.dcm", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        result.ErrorMessage.Should().Contain("DICOM 파일을 찾을 수 없습니다");
    }

    /// <summary>
    /// Verifies DicomStoreScu.StoreAsync returns failure for null file path.
    /// </summary>
    [Fact]
    public async Task DicomStoreScu_NullFilePath_ThrowsArgumentNullException()
    {
        var config = Substitute.For<IDicomNetworkConfig>();
        config.PacsHost.Returns("127.0.0.1");
        config.PacsPort.Returns(104);
        config.LocalAeTitle.Returns("HNVUE");
        config.PacsAeTitle.Returns("TESTPACS");

        var scu = new DicomStoreScu(config);

        var act = async () => await scu.StoreAsync(null!, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies DicomStoreScu handles a real DICOM file when network is unavailable.
    /// Covers IOException/DicomNetworkException paths with actual file I/O.
    /// </summary>
    [Fact]
    public async Task DicomStoreScu_RealDicomFile_NetworkUnavailable_ReturnsNetworkError()
    {
        var config = Substitute.For<IDicomNetworkConfig>();
        config.PacsHost.Returns("127.0.0.1");
        config.PacsPort.Returns(104);
        config.LocalAeTitle.Returns("HNVUE");
        config.PacsAeTitle.Returns("TESTPACS");

        var scu = new DicomStoreScu(config);
        var tempFile = await CreateTempDicomFileAsync();

        var result = await scu.StoreAsync(tempFile, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        // Network is unavailable — should get a connection error or similar
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  DicomService — Generic exception with non-Socket base (line 149-150)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies StoreAsync handles a generic exception that is not a SocketException
    /// at the base level. Covers the fallback error path (lines 149-150).
    /// </summary>
    [Fact]
    public async Task StoreAsync_GenericExceptionNonSocket_ReturnsStoreFailed()
    {
        var mockClient = Substitute.For<IDicomClient>();

        mockClient.AddRequestAsync(Arg.Any<DicomRequest>())
            .Returns(call => Task.CompletedTask);

        mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                throw new Exception("Some unexpected error");
            });

        var svc = CreateService(mockClient);
        var tempFile = await CreateTempDicomFileAsync();

        var result = await svc.StoreAsync(tempFile, "TESTPACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        result.ErrorMessage.Should().Contain("C-STORE 실패");
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Helpers
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Polls GetStatus until the expected status is reached or timeout.
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
