using System.IO;
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dicom;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using IDicomService = HnVue.Common.Abstractions.IDicomService;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Coverage round 3 -- focuses on LoggerMessage-generated code and remaining async paths.
/// Uses real loggers (<see cref="LoggerFactory.Create"/>) instead of <see cref="NullLogger{T}"/>
/// to exercise [<see cref="LoggerMessageAttribute"/>] source-generated code.
/// Target: HnVue.Dicom coverage from 82.7% to 85%+.
/// </summary>
[Trait("SWR", "SWR-DC-001")]
public sealed class DicomS14CoverageRound3Tests : IDisposable
{
    private readonly IDicomClient _mockClient;
    private readonly List<DicomRequest> _capturedRequests = [];
    private readonly List<string> _tempFiles = [];
    private readonly ILoggerFactory _loggerFactory;

    public DicomS14CoverageRound3Tests()
    {
        _mockClient = Substitute.For<IDicomClient>();
        _mockClient.AddRequestAsync(Arg.Any<DicomRequest>())
            .Returns(call =>
            {
                _capturedRequests.Add(call.Arg<DicomRequest>());
                return Task.CompletedTask;
            });

        // Use a REAL logger factory so that [LoggerMessage] source-generated
        // code (__LogPrintWarningStruct, __LogStoreWarningStruct, etc.) executes.
        _loggerFactory = LoggerFactory.Create(builder =>
            builder.SetMinimumLevel(LogLevel.Trace).AddDebug());
    }

    /// <summary>Disposes temporary DICOM files and the logger factory.</summary>
    public void Dispose()
    {
        foreach (var f in _tempFiles)
        {
            try { File.Delete(f); } catch { /* best effort */ }
        }

        _loggerFactory.Dispose();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private TestableDicomService CreateServiceWithRealLogger(DicomOptions? options = null)
    {
        var opts = Options.Create(options ?? CreateTestOptions());
        var logger = _loggerFactory.CreateLogger<DicomService>();
        return new TestableDicomService(opts, logger, _mockClient);
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

    private void SetupSendAsync(Action<DicomRequest> callback)
    {
        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var batch = _capturedRequests.ToList();
                _capturedRequests.Clear();
                foreach (var captured in batch)
                {
                    callback(captured);
                }

                return Task.CompletedTask;
            });

        // Re-wire AddRequestAsync after SendAsync override
        _mockClient.AddRequestAsync(Arg.Any<DicomRequest>())
            .Returns(call =>
            {
                _capturedRequests.Add(call.Arg<DicomRequest>());
                return Task.CompletedTask;
            });
    }

    private void SetupSendAsyncThrows(Exception exception)
    {
        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(_ => throw exception);
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
        var tempPath = Path.Combine(Path.GetTempPath(), $"s14_r3_{Guid.NewGuid():N}.dcm");
        await dicomFile.SaveAsync(tempPath);
        _tempFiles.Add(tempPath);
        return tempPath;
    }

    // ════════════════════════════════════════════════════════════════════════════
    //  PrintAsync failure paths with REAL logger
    //  Target: __LogPrintWarningStruct (46 lines, 0% -> covered)
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that PrintAsync calls <c>LogPrintWarning("N-CREATE")</c> when the
    /// Basic Film Session N-CREATE response is not Success.
    /// Exercises the <c>__LogPrintWarningStruct</c> source-generated code (46 lines).
    /// </summary>
    [Fact]
    public async Task PrintAsync_FilmSessionCreateFails_LogsPrintWarningNCreate()
    {
        var svc = CreateServiceWithRealLogger();
        var tempFile = await CreateTempDicomFileAsync();

        SetupSendAsync(req =>
        {
            if (req is DicomNCreateRequest nCreate)
            {
                // FilmSession N-CREATE fails
                nCreate.OnResponseReceived?.Invoke(nCreate,
                    new DicomNCreateResponse(nCreate, DicomStatus.ProcessingFailure));
            }
        });

        var result = await svc.PrintAsync(tempFile, "TESTPRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
        result.ErrorMessage.Should().Contain("N-CREATE");
    }

    /// <summary>
    /// Verifies that PrintAsync calls <c>LogPrintWarning("N-CREATE FilmBox")</c> when
    /// FilmSession N-CREATE succeeds but FilmBox N-CREATE fails.
    /// Exercises <c>__LogPrintWarningStruct</c> with a different phase argument.
    /// </summary>
    [Fact]
    public async Task PrintAsync_FilmBoxCreateFails_LogsPrintWarningFilmBox()
    {
        var svc = CreateServiceWithRealLogger();
        var tempFile = await CreateTempDicomFileAsync();
        var requestIndex = 0;

        SetupSendAsync(req =>
        {
            requestIndex++;
            if (req is DicomNCreateRequest nCreate)
            {
                if (requestIndex == 1)
                {
                    // FilmSession N-CREATE succeeds
                    nCreate.OnResponseReceived?.Invoke(nCreate,
                        new DicomNCreateResponse(nCreate, DicomStatus.Success));
                }
                else
                {
                    // FilmBox N-CREATE fails
                    nCreate.OnResponseReceived?.Invoke(nCreate,
                        new DicomNCreateResponse(nCreate, DicomStatus.ProcessingFailure));
                }
            }
        });

        var result = await svc.PrintAsync(tempFile, "TESTPRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    /// <summary>
    /// Verifies that PrintAsync calls <c>LogPrintWarning("N-ACTION")</c> when
    /// FilmSession and FilmBox succeed but N-ACTION Print fails.
    /// Exercises <c>__LogPrintWarningStruct</c> with the N-ACTION phase argument.
    /// </summary>
    [Fact]
    public async Task PrintAsync_NActionPrintFails_LogsPrintWarningNAction()
    {
        var svc = CreateServiceWithRealLogger();
        var tempFile = await CreateTempDicomFileAsync();

        SetupSendAsync(req =>
        {
            if (req is DicomNCreateRequest nCreate)
            {
                nCreate.OnResponseReceived?.Invoke(nCreate,
                    new DicomNCreateResponse(nCreate, DicomStatus.Success));
            }
            else if (req is DicomNSetRequest nSet)
            {
                nSet.OnResponseReceived?.Invoke(nSet,
                    new DicomNSetResponse(nSet, DicomStatus.Success));
            }
            else if (req is DicomNActionRequest nAction)
            {
                // N-ACTION Print fails
                nAction.OnResponseReceived?.Invoke(nAction,
                    new DicomNActionResponse(nAction, DicomStatus.ProcessingFailure));
            }
        });

        var result = await svc.PrintAsync(tempFile, "TESTPRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
        result.ErrorMessage.Should().Contain("N-ACTION");
    }

    /// <summary>
    /// Verifies that PrintAsync calls <c>LogPrintWarning("N-SET FilmBox")</c> when
    /// N-SET for the FilmBox fails.
    /// Exercises <c>__LogPrintWarningStruct</c> with the N-SET phase argument.
    /// </summary>
    [Fact]
    public async Task PrintAsync_SetFilmBoxFails_LogsPrintWarningNSet()
    {
        var svc = CreateServiceWithRealLogger();
        var tempFile = await CreateTempDicomFileAsync();
        var requestIndex = 0;

        SetupSendAsync(req =>
        {
            requestIndex++;
            if (req is DicomNCreateRequest nCreate)
            {
                // Both N-CREATEs succeed
                nCreate.OnResponseReceived?.Invoke(nCreate,
                    new DicomNCreateResponse(nCreate, DicomStatus.Success));
            }
            else if (req is DicomNSetRequest nSet)
            {
                // N-SET fails
                nSet.OnResponseReceived?.Invoke(nSet,
                    new DicomNSetResponse(nSet, DicomStatus.ProcessingFailure));
            }
        });

        var result = await svc.PrintAsync(tempFile, "TESTPRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    /// <summary>
    /// Verifies that PrintAsync succeeds through the full path with real logger,
    /// exercising <c>LogPrintSuccess</c> source-generated code.
    /// </summary>
    [Fact]
    public async Task PrintAsync_FullSuccess_WithRealLogger_LogsPrintSuccess()
    {
        var svc = CreateServiceWithRealLogger();
        var tempFile = await CreateTempDicomFileAsync();
        var nGetPollCount = 0;

        SetupSendAsync(req =>
        {
            if (req is DicomNCreateRequest nCreate)
            {
                nCreate.OnResponseReceived?.Invoke(nCreate,
                    new DicomNCreateResponse(nCreate, DicomStatus.Success));
            }
            else if (req is DicomNSetRequest nSet)
            {
                nSet.OnResponseReceived?.Invoke(nSet,
                    new DicomNSetResponse(nSet, DicomStatus.Success));
            }
            else if (req is DicomNActionRequest nAction)
            {
                nAction.OnResponseReceived?.Invoke(nAction,
                    new DicomNActionResponse(nAction, DicomStatus.Success));
            }
            else if (req is DicomNGetRequest nGet)
            {
                nGetPollCount++;
                var statusDataset = new DicomDataset
                {
                    { DicomTag.ExecutionStatus, "DONE" },
                };
                var response = new DicomNGetResponse(nGet, DicomStatus.Success);
                response.Dataset = statusDataset;
                nGet.OnResponseReceived?.Invoke(nGet, response);
            }
        });

        var result = await svc.PrintAsync(tempFile, "TESTPRINTER", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Verifies PrintAsync handles IOException with real logger,
    /// exercising <c>LogPrintIoError</c> source-generated code.
    /// </summary>
    [Fact]
    public async Task PrintAsync_IOException_WithRealLogger_ReturnsPrintFailed()
    {
        var svc = CreateServiceWithRealLogger();
        var tempFile = await CreateTempDicomFileAsync();

        SetupSendAsyncThrows(new IOException("Disk error"));

        var result = await svc.PrintAsync(tempFile, "TESTPRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
        result.ErrorMessage.Should().Contain("I/O error");
    }

    /// <summary>
    /// Verifies PrintAsync handles DicomNetworkException with real logger,
    /// exercising <c>LogNetworkError</c> source-generated code for Print context.
    /// </summary>
    [Fact]
    public async Task PrintAsync_DicomNetworkException_WithRealLogger_ReturnsConnectionFailed()
    {
        var svc = CreateServiceWithRealLogger();
        var tempFile = await CreateTempDicomFileAsync();

        SetupSendAsyncThrows(new DicomNetworkException("Connection refused"));

        var result = await svc.PrintAsync(tempFile, "TESTPRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    /// <summary>
    /// Verifies PrintAsync handles SocketException (as base exception of generic Exception)
    /// with real logger, exercising <c>LogConnectionError</c> source-generated code.
    /// </summary>
    [Fact]
    public async Task PrintAsync_SocketExceptionAsBase_WithRealLogger_ReturnsConnectionFailed()
    {
        var svc = CreateServiceWithRealLogger();
        var tempFile = await CreateTempDicomFileAsync();

        var socketEx = new System.Net.Sockets.SocketException(10061);
        SetupSendAsyncThrows(new Exception("Send failed", socketEx));

        var result = await svc.PrintAsync(tempFile, "TESTPRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    /// <summary>
    /// Verifies PrintAsync handles generic Exception (non-Socket) with real logger.
    /// </summary>
    [Fact]
    public async Task PrintAsync_GenericException_WithRealLogger_ReturnsPrintFailed()
    {
        var svc = CreateServiceWithRealLogger();
        var tempFile = await CreateTempDicomFileAsync();

        SetupSendAsyncThrows(new Exception("Unexpected error"));

        var result = await svc.PrintAsync(tempFile, "TESTPRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    // ════════════════════════════════════════════════════════════════════════════
    //  StoreAsync with real logger -- exercises LogStoreWarning/LogStoreSuccess
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies StoreAsync calls <c>LogStoreWarning</c> with real logger when
    /// C-STORE response is non-success and non-transient.
    /// Exercises <c>__LogStoreWarningStruct</c> source-generated code.
    /// </summary>
    [Fact]
    public async Task StoreAsync_NonTransientFailure_WithRealLogger_LogsStoreWarning()
    {
        var svc = CreateServiceWithRealLogger();
        var tempFile = await CreateTempDicomFileAsync();

        SetupSendAsync(req =>
        {
            if (req is DicomCStoreRequest cStore)
            {
                cStore.OnResponseReceived?.Invoke(cStore,
                    new DicomCStoreResponse(cStore, DicomStatus.StorageCannotUnderstand));
            }
        });

        var result = await svc.StoreAsync(tempFile, "TESTPACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    /// <summary>
    /// Verifies StoreAsync calls <c>LogStoreSuccess</c> with real logger on success.
    /// Exercises <c>__LogStoreSuccessStruct</c> source-generated code.
    /// </summary>
    [Fact]
    public async Task StoreAsync_Success_WithRealLogger_LogsStoreSuccess()
    {
        var svc = CreateServiceWithRealLogger();
        var tempFile = await CreateTempDicomFileAsync();

        SetupSendAsync(req =>
        {
            if (req is DicomCStoreRequest cStore)
            {
                cStore.OnResponseReceived?.Invoke(cStore,
                    new DicomCStoreResponse(cStore, DicomStatus.Success));
            }
        });

        var result = await svc.StoreAsync(tempFile, "TESTPACS", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Verifies StoreAsync handles IOException with real logger,
    /// exercising <c>LogStoreIoError</c> source-generated code.
    /// </summary>
    [Fact]
    public async Task StoreAsync_IOException_WithRealLogger_LogsStoreIoError()
    {
        var svc = CreateServiceWithRealLogger();
        var tempFile = await CreateTempDicomFileAsync();

        SetupSendAsyncThrows(new IOException("Disk error"));

        var result = await svc.StoreAsync(tempFile, "TESTPACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    /// <summary>
    /// Verifies StoreAsync handles DicomNetworkException with real logger,
    /// exercising <c>LogNetworkError</c> source-generated code for C-STORE context.
    /// </summary>
    [Fact]
    public async Task StoreAsync_DicomNetworkException_WithRealLogger_LogsNetworkError()
    {
        var svc = CreateServiceWithRealLogger();
        var tempFile = await CreateTempDicomFileAsync();

        SetupSendAsyncThrows(new DicomNetworkException("Connection refused"));

        var result = await svc.StoreAsync(tempFile, "TESTPACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    /// <summary>
    /// Verifies StoreAsync handles SocketException (as base exception) with real logger,
    /// exercising <c>LogConnectionError</c> source-generated code for C-STORE context.
    /// </summary>
    [Fact]
    public async Task StoreAsync_SocketExceptionAsBase_WithRealLogger_LogsConnectionError()
    {
        var svc = CreateServiceWithRealLogger();
        var tempFile = await CreateTempDicomFileAsync();

        var socketEx = new System.Net.Sockets.SocketException(10061);
        SetupSendAsyncThrows(new Exception("Send failed", socketEx));

        var result = await svc.StoreAsync(tempFile, "TESTPACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    // ════════════════════════════════════════════════════════════════════════════
    //  QueryWorklistAsync with real logger
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies QueryWorklistAsync exercises <c>LogQuerySuccess</c> with real logger.
    /// </summary>
    [Fact]
    public async Task QueryWorklistAsync_Success_WithRealLogger_LogsQuerySuccess()
    {
        var svc = CreateServiceWithRealLogger();

        SetupSendAsync(req =>
        {
            if (req is DicomCFindRequest cFind)
            {
                // Return one pending result
                var responseDataset = new DicomDataset
                {
                    { DicomTag.AccessionNumber, "ACC001" },
                    { DicomTag.PatientID, "PAT001" },
                    { DicomTag.PatientName, "Test^Patient" },
                    { DicomTag.StudyDate, "20260420" },
                };
                var pending = new DicomCFindResponse(cFind, DicomStatus.Pending);
                pending.Dataset = responseDataset;
                cFind.OnResponseReceived?.Invoke(cFind, pending);

                // Final success response
                cFind.OnResponseReceived?.Invoke(cFind,
                    new DicomCFindResponse(cFind, DicomStatus.Success));
            }
        });

        var query = new WorklistQuery(AeTitle: "TESTMWL", PatientId: null, DateFrom: null, DateTo: null);
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    /// <summary>
    /// Verifies QueryWorklistAsync exercises <c>LogNetworkError</c> with real logger
    /// on DicomNetworkException.
    /// </summary>
    [Fact]
    public async Task QueryWorklistAsync_DicomNetworkException_WithRealLogger_ReturnsConnectionFailed()
    {
        var svc = CreateServiceWithRealLogger();

        SetupSendAsyncThrows(new DicomNetworkException("Connection refused"));

        var query = new WorklistQuery(AeTitle: "TESTMWL", PatientId: null, DateFrom: null, DateTo: null);
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    /// <summary>
    /// Verifies QueryWorklistAsync exercises <c>LogConnectionError</c> with real logger
    /// on SocketException.
    /// </summary>
    [Fact]
    public async Task QueryWorklistAsync_SocketException_WithRealLogger_ReturnsConnectionFailed()
    {
        var svc = CreateServiceWithRealLogger();

        SetupSendAsyncThrows(new System.Net.Sockets.SocketException(10061));

        var query = new WorklistQuery(AeTitle: "TESTMWL", PatientId: null, DateFrom: null, DateTo: null);
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    /// <summary>
    /// Verifies QueryWorklistAsync exercises <c>LogUnexpectedQueryError</c> with real logger
    /// on generic Exception.
    /// </summary>
    [Fact]
    public async Task QueryWorklistAsync_GenericException_WithRealLogger_ReturnsConnectionFailed()
    {
        var svc = CreateServiceWithRealLogger();

        SetupSendAsyncThrows(new Exception("Unexpected query error"));

        var query = new WorklistQuery(AeTitle: "TESTMWL", PatientId: null, DateFrom: null, DateTo: null);
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    // ════════════════════════════════════════════════════════════════════════════
    //  SendRdsrAsync with real logger -- exercises LogRdsrSuccess/LogRdsrWarning
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies SendRdsrAsync exercises <c>LogRdsrSuccess</c> with real logger.
    /// </summary>
    [Fact]
    public async Task SendRdsrAsync_Success_WithRealLogger_LogsRdsrSuccess()
    {
        var svc = CreateServiceWithRealLogger();

        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var doseRecord = new DoseRecord(
            DoseId: "DOSE-001", StudyInstanceUid: "1.2.3.4.5",
            Dap: 5.0, Ei: 1000.0, EffectiveDose: 0.05,
            BodyPart: "CHEST", RecordedAt: DateTimeOffset.UtcNow,
            PatientId: "PAT001", DapMgyCm2: 5.0,
            FieldAreaCm2: 400.0, MeanPixelValue: 500.0,
            EiTarget: 500.0, EsdMgy: 0.02);

        var patientInfo = new RdsrPatientInfo("PAT001", "Test^Patient", "19900101", "M");
        var studyInfo = new RdsrStudyInfo("1.2.3.4.5", "20260420", "120000", "ACC001");

        var result = await svc.SendRdsrAsync(doseRecord, patientInfo, studyInfo, "PACS");

        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Verifies SendRdsrAsync exercises <c>LogRdsrWarning</c> with real logger when
    /// the C-STORE response indicates failure.
    /// </summary>
    [Fact]
    public async Task SendRdsrAsync_FailureResponse_WithRealLogger_LogsRdsrWarning()
    {
        var svc = CreateServiceWithRealLogger();

        SetupSendAsync(req =>
        {
            if (req is DicomCStoreRequest cStore)
            {
                cStore.OnResponseReceived?.Invoke(cStore,
                    new DicomCStoreResponse(cStore, DicomStatus.ProcessingFailure));
            }
        });

        var doseRecord = new DoseRecord(
            DoseId: "DOSE-001", StudyInstanceUid: "1.2.3.4.5",
            Dap: 5.0, Ei: 1000.0, EffectiveDose: 0.05,
            BodyPart: "CHEST", RecordedAt: DateTimeOffset.UtcNow,
            PatientId: "PAT001", DapMgyCm2: 5.0,
            FieldAreaCm2: 400.0, MeanPixelValue: 500.0,
            EiTarget: 500.0, EsdMgy: 0.02);

        var patientInfo = new RdsrPatientInfo("PAT001", "Test^Patient", "19900101", "M");
        var studyInfo = new RdsrStudyInfo("1.2.3.4.5", "20260420", "120000", "ACC001");

        var result = await svc.SendRdsrAsync(doseRecord, patientInfo, studyInfo, "PACS");

        result.IsFailure.Should().BeTrue();
    }

    // ════════════════════════════════════════════════════════════════════════════
    //  AsyncStorePipeline with real logger -- exercises LoggerMessage code
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies AsyncStorePipeline processes an item successfully with real logger,
    /// exercising <c>LogStarted</c>, <c>LogEnqueued</c>, <c>LogDelivered</c> source-generated code.
    /// </summary>
    [Fact]
    public async Task Pipeline_SuccessWithRealLogger_LogsAllStages()
    {
        var mockService = Substitute.For<IDicomService>();
        var logger = _loggerFactory.CreateLogger<AsyncStorePipeline>();
        var pipeline = new AsyncStorePipeline(mockService, "PACS", logger);

        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        await pipeline.EnqueueAsync("test.dcm", "uid-real-logger-1");
        await pipeline.StartAsync(cts.Token);

        // Wait for the item to be processed
        var deadline = DateTime.UtcNow.AddSeconds(3);
        while (pipeline.GetStatus("uid-real-logger-1") != StoreStatus.Sent && DateTime.UtcNow < deadline)
        {
            await Task.Delay(50, cts.Token);
        }

        pipeline.GetStatus("uid-real-logger-1").Should().Be(StoreStatus.Sent);

        await pipeline.StopAsync(cts.Token);
    }

    /// <summary>
    /// Verifies AsyncStorePipeline final failure path with real logger,
    /// exercising <c>LogFinalFailure</c> source-generated code.
    /// </summary>
    [Fact]
    [Trait("Category", "Slow")]
    public async Task Pipeline_FinalFailureWithRealLogger_LogsFinalFailure()
    {
        var mockService = Substitute.For<IDicomService>();
        var logger = _loggerFactory.CreateLogger<AsyncStorePipeline>();
        var pipeline = new AsyncStorePipeline(mockService, "PACS", logger);

        // Always fail so Polly retries exhaust (3 retries + 1 initial = 4 attempts)
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DicomStoreFailed, "permanent failure"));

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));

        await pipeline.EnqueueAsync("fail-real.dcm", "uid-fail-real");
        await pipeline.StartAsync(cts.Token);

        // Wait for failure
        var deadline = DateTime.UtcNow.AddSeconds(60);
        while (pipeline.GetStatus("uid-fail-real") != StoreStatus.Failed && DateTime.UtcNow < deadline)
        {
            await Task.Delay(100, cts.Token);
        }

        pipeline.GetStatus("uid-fail-real").Should().Be(StoreStatus.Failed);

        await pipeline.StopAsync(cts.Token);
    }

    /// <summary>
    /// Verifies AsyncStorePipeline cancellation path with real logger,
    /// exercising <c>LogCancelled</c> source-generated code.
    /// </summary>
    [Fact]
    public async Task Pipeline_CancelledWithRealLogger_LogsCancelled()
    {
        var mockService = Substitute.For<IDicomService>();
        var logger = _loggerFactory.CreateLogger<AsyncStorePipeline>();
        var pipeline = new AsyncStorePipeline(mockService, "PACS", logger);

        var blockTcs = new TaskCompletionSource<bool>();
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                await blockTcs.Task;
                call.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.Success();
            });

        using var cts = new CancellationTokenSource();

        await pipeline.EnqueueAsync("cancel-real.dcm", "uid-cancel-real");
        await pipeline.StartAsync(cts.Token);

        // Give consumer time to pick up the item
        await Task.Delay(200);

        cts.Cancel();
        blockTcs.SetResult(true);

        await Task.Delay(300);

        pipeline.GetStatus("uid-cancel-real").Should().Be(StoreStatus.Pending);

        await pipeline.DisposeAsync();
    }

    // ════════════════════════════════════════════════════════════════════════════
    //  GetPrintJobStatusAsync with real logger -- exercises remaining paths
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies GetPrintJobStatusAsync exercises <c>LogPrintStatusComplete</c> with real logger.
    /// </summary>
    [Fact]
    public async Task GetPrintJobStatusAsync_DoneStatus_WithRealLogger_ReturnsDone()
    {
        var svc = CreateServiceWithRealLogger();

        SetupSendAsync(req =>
        {
            if (req is DicomNGetRequest nGet)
            {
                var statusDataset = new DicomDataset
                {
                    { DicomTag.ExecutionStatus, "DONE" },
                };
                var response = new DicomNGetResponse(nGet, DicomStatus.Success);
                response.Dataset = statusDataset;
                nGet.OnResponseReceived?.Invoke(nGet, response);
            }
        });

        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", "PRINTER", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(PrintJobStatus.Done);
    }

    /// <summary>
    /// Verifies GetPrintJobStatusAsync exercises <c>LogPrintStatusPollWarning</c> with real logger.
    /// </summary>
    [Fact]
    public async Task GetPrintJobStatusAsync_NGetFails_WithRealLogger_LogsPollWarning()
    {
        var svc = CreateServiceWithRealLogger();

        SetupSendAsync(req =>
        {
            if (req is DicomNGetRequest nGet)
            {
                nGet.OnResponseReceived?.Invoke(nGet,
                    new DicomNGetResponse(nGet, DicomStatus.ProcessingFailure));
            }
        });

        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    // ════════════════════════════════════════════════════════════════════════════
    //  CreateFilmBoxAsync / SetFilmBoxAsync with real logger
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies CreateFilmBoxAsync exercises <c>LogFilmBoxCreated</c> with real logger.
    /// </summary>
    [Fact]
    public async Task CreateFilmBoxAsync_Success_WithRealLogger_LogsFilmBoxCreated()
    {
        var svc = CreateServiceWithRealLogger();

        SetupSendAsync(req =>
        {
            if (req is DicomNCreateRequest nCreate)
            {
                nCreate.OnResponseReceived?.Invoke(nCreate,
                    new DicomNCreateResponse(nCreate, DicomStatus.Success));
            }
        });

        var filmSessionUid = DicomUID.Generate();
        var result = await svc.CreateFilmBoxAsync(filmSessionUid, "PRINTER", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Verifies SetFilmBoxAsync exercises <c>LogFilmBoxSet</c> with real logger.
    /// </summary>
    [Fact]
    public async Task SetFilmBoxAsync_Success_WithRealLogger_LogsFilmBoxSet()
    {
        var svc = CreateServiceWithRealLogger();

        SetupSendAsync(req =>
        {
            if (req is DicomNSetRequest nSet)
            {
                nSet.OnResponseReceived?.Invoke(nSet,
                    new DicomNSetResponse(nSet, DicomStatus.Success));
            }
        });

        var filmBoxUid = DicomUID.Generate();
        var result = await svc.SetFilmBoxAsync(filmBoxUid, "PRINTER", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    // ── Testable subclass ───────────────────────────────────────────────────────

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
}
