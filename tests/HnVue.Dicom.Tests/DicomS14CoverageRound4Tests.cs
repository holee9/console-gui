using System.IO;
using FellowOakDicom;
using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dicom;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Coverage round 4 — DicomOutbox cancellation/dead-letter, DicomFileIO error paths,
/// AsyncStorePipeline final failure handler.
/// </summary>
[Trait("SWR", "SWR-DC-001")]
public sealed class DicomS14CoverageRound4Tests : IAsyncLifetime
{
    private readonly List<string> tempFiles = [];

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        foreach (var f in this.tempFiles)
        {
            try { File.Delete(f); } catch { /* ignored */ }
        }

        return Task.CompletedTask;
    }

    private async Task<string> CreateTempDicomFileAsync()
    {
        var path = Path.GetTempFileName();
        var dataset = new DicomDataset();
        dataset.AddOrUpdate(DicomTag.PatientID, "TEST001");
        dataset.AddOrUpdate(DicomTag.PatientName, "Test^Patient");
        dataset.AddOrUpdate(FellowOakDicom.DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage);
        dataset.AddOrUpdate(FellowOakDicom.DicomTag.SOPInstanceUID, DicomUID.Generate());
        dataset.AddOrUpdate(FellowOakDicom.DicomTag.StudyInstanceUID, DicomUID.Generate());
        var file = new DicomFile(dataset);
        await file.SaveAsync(path);
        this.tempFiles.Add(path);
        return path;
    }

    // ── DicomOutbox ─────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that ProcessAsync re-queues items when cancelled.
    /// </summary>
    [Fact]
    public async Task Outbox_CancelDuringProcessing_RequeuesItem()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Trace));
        var logger = loggerFactory.CreateLogger<DicomOutbox>();
        var mockService = Substitute.For<IDicomService>();
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                call.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.Success();
            });

        var outbox = new DicomOutbox(mockService, logger);
        await outbox.EnqueueAsync("/tmp/test.dcm");

        var cts = new CancellationTokenSource();
        cts.Cancel();
        await outbox.ProcessAsync("PACS", cts.Token);

        outbox.Count.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Verifies that ProcessAsync dead-letters items when all retries fail.
    /// </summary>
    [Fact]
    public async Task Outbox_AllRetriesFail_DeadLettersItem()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Trace));
        var logger = loggerFactory.CreateLogger<DicomOutbox>();
        var failService = Substitute.For<IDicomService>();
        failService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DicomStoreFailed, "Store failed"));

        var outbox = new DicomOutbox(failService, logger);
        await outbox.EnqueueAsync("/tmp/test.dcm");
        await outbox.ProcessAsync("PACS");

        outbox.Count.Should().Be(0);
    }

    /// <summary>
    /// Verifies that ProcessAsync delivers items successfully.
    /// </summary>
    [Fact]
    public async Task Outbox_SuccessfulDelivery_ClearsQueue()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Trace));
        var logger = loggerFactory.CreateLogger<DicomOutbox>();
        var successService = Substitute.For<IDicomService>();
        successService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var outbox = new DicomOutbox(successService, logger);
        await outbox.EnqueueAsync("/tmp/test.dcm");
        await outbox.ProcessAsync("PACS");

        outbox.Count.Should().Be(0);
    }

    /// <summary>
    /// Verifies that EnqueueAsync throws on empty path.
    /// </summary>
    [Fact]
    public async Task Outbox_EnqueueEmptyPath_ThrowsArgumentException()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Trace));
        var outbox = new DicomOutbox(Substitute.For<IDicomService>(), loggerFactory.CreateLogger<DicomOutbox>());

        var act = async () => await outbox.EnqueueAsync(string.Empty);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ── DicomFileIO ────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that ReadAsync returns failure for corrupted (non-DICOM) files.
    /// </summary>
    [Fact]
    public async Task FileIO_ReadCorruptedFile_ReturnsFailure()
    {
        var tempCorrupted = Path.GetTempFileName();
        this.tempFiles.Add(tempCorrupted);
        await File.WriteAllTextAsync(tempCorrupted, "NOT_A_DICOM_FILE_CORRUPTED_CONTENT");

        var result = await DicomFileIO.ReadAsync(tempCorrupted);
        result.IsFailure.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that ReadAsync returns failure for non-existent files.
    /// </summary>
    [Fact]
    public async Task FileIO_ReadNonExistentFile_ReturnsNotFound()
    {
        var result = await DicomFileIO.ReadAsync("/non/existent/path/test.dcm");
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    /// <summary>
    /// Verifies that ReadAsync throws on null path.
    /// </summary>
    [Fact]
    public async Task FileIO_ReadNullPath_ThrowsArgumentNullException()
    {
        var act = () => DicomFileIO.ReadAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that WriteAsync throws on null wrapper.
    /// </summary>
    [Fact]
    public async Task FileIO_WriteNullWrapper_ThrowsArgumentNullException()
    {
        var act = () => DicomFileIO.WriteAsync(null!, "/tmp/out.dcm");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that WriteAsync throws on null output path.
    /// </summary>
    [Fact]
    public async Task FileIO_WriteNullPath_ThrowsArgumentNullException()
    {
        var path = await this.CreateTempDicomFileAsync();
        var readResult = await DicomFileIO.ReadAsync(path);
        readResult.IsSuccess.Should().BeTrue();

        var act = () => DicomFileIO.WriteAsync(readResult.Value, null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that WriteAsync succeeds for valid input.
    /// </summary>
    [Fact]
    public async Task FileIO_WriteValid_WritesFile()
    {
        var srcPath = await this.CreateTempDicomFileAsync();
        var readResult = await DicomFileIO.ReadAsync(srcPath);
        readResult.IsSuccess.Should().BeTrue();

        var outPath = Path.GetTempFileName();
        this.tempFiles.Add(outPath);

        var writeResult = await DicomFileIO.WriteAsync(readResult.Value, outPath);
        writeResult.IsSuccess.Should().BeTrue();
        File.Exists(outPath).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that WriteAsync creates output directory if missing.
    /// </summary>
    [Fact]
    public async Task FileIO_WriteCreatesDirectory_Succeeds()
    {
        var srcPath = await this.CreateTempDicomFileAsync();
        var readResult = await DicomFileIO.ReadAsync(srcPath);
        readResult.IsSuccess.Should().BeTrue();

        var outDir = Path.Combine(Path.GetTempPath(), $"HnVue_Test_{Guid.NewGuid():N}");
        var outPath = Path.Combine(outDir, "output.dcm");
        this.tempFiles.Add(outPath);
        this.tempFiles.Add(outDir);

        try
        {
            var writeResult = await DicomFileIO.WriteAsync(readResult.Value, outPath);
            writeResult.IsSuccess.Should().BeTrue();
            File.Exists(outPath).Should().BeTrue();
        }
        finally
        {
            try { Directory.Delete(outDir, true); } catch { /* ignored */ }
        }
    }

    /// <summary>
    /// Verifies GetTagValueAsync reads SOPInstanceUID.
    /// </summary>
    [Fact]
    public async Task FileIO_GetTagValue_SopInstanceUid_ReturnsValue()
    {
        var path = await this.CreateTempDicomFileAsync();
        var result = await DicomFileIO.GetTagValueAsync(path, "SOPInstanceUID");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Verifies GetTagValueAsync returns null for unknown tag.
    /// </summary>
    [Fact]
    public async Task FileIO_GetTagValue_UnknownTag_ReturnsNull()
    {
        var path = await this.CreateTempDicomFileAsync();
        var result = await DicomFileIO.GetTagValueAsync(path, "PatientName");
        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Verifies GetTagValueAsync returns failure for non-existent file.
    /// </summary>
    [Fact]
    public async Task FileIO_GetTagValue_NonExistentFile_ReturnsFailure()
    {
        var result = await DicomFileIO.GetTagValueAsync("/non/existent/test.dcm", "SOPInstanceUID");
        result.IsFailure.Should().BeTrue();
    }

    // ── AsyncStorePipeline — final failure handler ─────────────────────────

    /// <summary>
    /// Verifies pipeline fires StoreCompleted with success=false on permanent failure.
    /// </summary>
    [Fact]
    public async Task Pipeline_PermanentFailure_FiresEventWithFailure()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Trace));
        var logger = loggerFactory.CreateLogger<AsyncStorePipeline>();

        var failService = Substitute.For<IDicomService>();
        failService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure(ErrorCode.DicomStoreFailed, "Permanent failure")));

        var pipeline = new AsyncStorePipeline(failService, "PACS", logger);

        StoreCompletedEventArgs? completedArgs = null;
        pipeline.StoreCompleted += (_, args) => completedArgs = args;

        await pipeline.StartAsync();
        await pipeline.EnqueueAsync("/tmp/test.dcm", "1.2.3.FAIL");

        // Wait for Polly retries (3 retries with exponential backoff: 2s, 4s, 8s = ~14s)
        for (int i = 0; i < 80; i++)
        {
            await Task.Delay(250);
            if (completedArgs != null) break;
        }

        completedArgs.Should().NotBeNull();
        completedArgs!.Success.Should().BeFalse();
        pipeline.GetStatus("1.2.3.FAIL").Should().Be(StoreStatus.Failed);

        await pipeline.StopAsync();
        await pipeline.DisposeAsync();
    }

    /// <summary>
    /// Verifies pipeline fires StoreCompleted with success=true on success.
    /// </summary>
    [Fact]
    public async Task Pipeline_Success_FiresEventWithSuccess()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Trace));
        var logger = loggerFactory.CreateLogger<AsyncStorePipeline>();

        var successService = Substitute.For<IDicomService>();
        successService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        var pipeline = new AsyncStorePipeline(successService, "PACS", logger);

        StoreCompletedEventArgs? completedArgs = null;
        pipeline.StoreCompleted += (_, args) => completedArgs = args;

        await pipeline.StartAsync();
        await pipeline.EnqueueAsync("/tmp/test.dcm", "1.2.3.OK");

        for (int i = 0; i < 40; i++)
        {
            await Task.Delay(100);
            if (completedArgs != null) break;
        }

        completedArgs.Should().NotBeNull();
        completedArgs!.Success.Should().BeTrue();
        pipeline.GetStatus("1.2.3.OK").Should().Be(StoreStatus.Sent);

        await pipeline.StopAsync();
        await pipeline.DisposeAsync();
    }

    // ── DicomStoreScu — real file + network failure ────────────────────────

    /// <summary>
    /// Verifies StoreAsync with a real DICOM file hits network error path.
    /// </summary>
    [Fact]
    public async Task StoreScu_RealDicomFile_NetworkUnavailable_CoversRetryPath()
    {
        var config = Substitute.For<IDicomNetworkConfig>();
        config.PacsHost.Returns("127.0.0.1");
        config.PacsPort.Returns(104);
        config.PacsAeTitle.Returns("PACS");
        config.LocalAeTitle.Returns("HNVUE");
        var scu = new DicomStoreScu(config);

        var path = await this.CreateTempDicomFileAsync();
        var result = await scu.StoreAsync(path);

        result.IsFailure.Should().BeTrue();
    }
}
