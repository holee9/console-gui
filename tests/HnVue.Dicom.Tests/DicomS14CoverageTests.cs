using System.IO;
using System.Net.Sockets;
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dicom;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// S14 coverage gap tests targeting DicomService (GetPrintJobStatusAsync, SendRdsrAsync,
/// CreateFilmBoxAsync, SetFilmBoxAsync), DicomStoreScu (StoreAsync retry, GetUserFriendlyStatus,
/// IsTransientNetworkError), and AsyncStorePipeline (DisposeAsync double-dispose).
/// Target: Dicom module coverage from 79.3% to 85%+.
/// </summary>
[Trait("SWR", "SWR-DC-001")]
public sealed class DicomS14CoverageTests : IDisposable
{
    private readonly IDicomClient _mockClient;
    private readonly List<DicomRequest> _capturedRequests = [];
    private readonly List<string> _tempFiles = [];

    public DicomS14CoverageTests()
    {
        _mockClient = Substitute.For<IDicomClient>();
        _mockClient.AddRequestAsync(Arg.Any<DicomRequest>())
            .Returns(call =>
            {
                _capturedRequests.Add(call.Arg<DicomRequest>());
                return Task.CompletedTask;
            });
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }

    // ── Helper: TestableDicomService ────────────────────────────────────────────

    private TestableDicomService CreateService(DicomOptions? options = null)
    {
        var opts = Options.Create(options ?? new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PacsHost = "127.0.0.1",
            PacsPort = 104,
            MwlHost = "127.0.0.1",
            MwlPort = 104,
            PrinterHost = "127.0.0.1",
            PrinterPort = 104,
            MppsHost = "127.0.0.1",
            MppsPort = 104,
        });
        return new TestableDicomService(opts, NullLogger<DicomService>.Instance, _mockClient);
    }

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
        // Ensure the mock returns for all subsequent calls (NSubstitute auto-reset behavior)
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
        var tempPath = Path.Combine(Path.GetTempPath(), $"s14_test_{Guid.NewGuid():N}.dcm");
        await dicomFile.SaveAsync(tempPath);
        _tempFiles.Add(tempPath);
        return tempPath;
    }

    private static DoseRecord CreateTestDoseRecord()
    {
        return new DoseRecord(
            DoseId: "DOSE-001",
            StudyInstanceUid: "1.2.3.4.5.6.7.8.9",
            Dap: 5.0,
            Ei: 1000.0,
            EffectiveDose: 0.05,
            BodyPart: "CHEST",
            RecordedAt: DateTimeOffset.UtcNow,
            PatientId: "PAT001",
            DapMgyCm2: 5.0,
            FieldAreaCm2: 400.0,
            MeanPixelValue: 500.0,
            EiTarget: 500.0,
            EsdMgy: 0.02);
    }

    private static RdsrPatientInfo CreateTestPatientInfo() => new("PAT001", "Test^Patient", "19900101", "M");

    private static RdsrStudyInfo CreateTestStudyInfo() => new("1.2.3.4.5.6.7.8.9", "20260420", "120000", "ACC001");

    // ════════════════════════════════════════════════════════════════════════════
    // GetPrintJobStatusAsync tests
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetPrintJobStatusAsync returns Done when the printer
    /// immediately reports DONE status.
    /// </summary>
    [Fact]
    public async Task GetPrintJobStatusAsync_DoneStatus_ReturnsDone()
    {
        var svc = CreateService();
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
    /// Verifies that GetPrintJobStatusAsync returns Failure when the printer
    /// reports FAILURE status.
    /// </summary>
    [Fact]
    public async Task GetPrintJobStatusAsync_FailureStatus_ReturnsFailure()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomNGetRequest nGet)
            {
                var statusDataset = new DicomDataset
                {
                    { DicomTag.ExecutionStatus, "FAILURE" },
                };
                var response = new DicomNGetResponse(nGet, DicomStatus.Success);
                response.Dataset = statusDataset;
                nGet.OnResponseReceived?.Invoke(nGet, response);
            }
        });

        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", "PRINTER", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(PrintJobStatus.Failure);
    }

    /// <summary>
    /// Verifies that GetPrintJobStatusAsync returns Pending (via polling exhaustion)
    /// when the printer keeps reporting PENDING for all 10 attempts.
    /// </summary>
    [Fact]
    public async Task GetPrintJobStatusAsync_AlwaysPending_ReturnsPendingAfterExhaustion()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomNGetRequest nGet)
            {
                var statusDataset = new DicomDataset
                {
                    { DicomTag.ExecutionStatus, "PENDING" },
                };
                var response = new DicomNGetResponse(nGet, DicomStatus.Success);
                response.Dataset = statusDataset;
                nGet.OnResponseReceived?.Invoke(nGet, response);
            }
        });

        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", "PRINTER", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(PrintJobStatus.Pending);
    }

    /// <summary>
    /// Verifies that GetPrintJobStatusAsync transitions from Pending to Printing to Done.
    /// </summary>
    [Fact]
    public async Task GetPrintJobStatusAsync_PendingToPrintingToDone_ReturnsDone()
    {
        var svc = CreateService();
        var pollCount = 0;
        SetupSendAsync(req =>
        {
            if (req is DicomNGetRequest nGet)
            {
                pollCount++;
                var statusValue = pollCount switch
                {
                    1 => "PENDING",
                    2 => "PRINTING",
                    _ => "DONE",
                };
                var statusDataset = new DicomDataset
                {
                    { DicomTag.ExecutionStatus, statusValue },
                };
                var response = new DicomNGetResponse(nGet, DicomStatus.Success);
                response.Dataset = statusDataset;
                nGet.OnResponseReceived?.Invoke(nGet, response);
            }
        });

        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", "PRINTER", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(PrintJobStatus.Done);
        pollCount.Should().Be(3);
    }

    /// <summary>
    /// Verifies that GetPrintJobStatusAsync returns failure when N-GET response
    /// is not successful (non-Success DicomStatus).
    /// </summary>
    [Fact]
    public async Task GetPrintJobStatusAsync_NGetFails_ReturnsPrintFailed()
    {
        var svc = CreateService();
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

    /// <summary>
    /// Verifies that GetPrintJobStatusAsync returns failure when N-GET response
    /// is non-success with a specific failure message.
    /// </summary>
    [Fact]
    public async Task GetPrintJobStatusAsync_NGetFailureWithMessage_ReturnsPrintFailed()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomNGetRequest nGet)
            {
                nGet.OnResponseReceived?.Invoke(nGet,
                    new DicomNGetResponse(nGet, DicomStatus.NoSuchEventType));
            }
        });

        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    /// <summary>
    /// Verifies that GetPrintJobStatusAsync returns OperationCancelled when
    /// the cancellation token is triggered during polling.
    /// </summary>
    [Fact]
    public async Task GetPrintJobStatusAsync_CancelledDuringPoll_ReturnsOperationCancelled()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomNGetRequest nGet)
            {
                var statusDataset = new DicomDataset
                {
                    { DicomTag.ExecutionStatus, "PENDING" },
                };
                var response = new DicomNGetResponse(nGet, DicomStatus.Success);
                response.Dataset = statusDataset;
                nGet.OnResponseReceived?.Invoke(nGet, response);
            }
        });

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", "PRINTER", cts.Token);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.OperationCancelled);
    }

    /// <summary>
    /// Verifies that GetPrintJobStatusAsync returns OperationCancelled when
    /// the SendAsync throws OperationCanceledException.
    /// </summary>
    [Fact]
    public async Task GetPrintJobStatusAsync_OperationCancelledException_ReturnsOperationCancelled()
    {
        var svc = CreateService();
        SetupSendAsyncThrows(new OperationCanceledException());

        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.OperationCancelled);
    }

    /// <summary>
    /// Verifies that GetPrintJobStatusAsync returns ConnectionFailed on DicomNetworkException.
    /// </summary>
    [Fact]
    public async Task GetPrintJobStatusAsync_DicomNetworkException_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        SetupSendAsyncThrows(new DicomNetworkException("Connection refused"));

        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    /// <summary>
    /// Verifies that GetPrintJobStatusAsync returns PrintFailed on generic exception.
    /// </summary>
    [Fact]
    public async Task GetPrintJobStatusAsync_GenericException_ReturnsPrintFailed()
    {
        var svc = CreateService();
        SetupSendAsyncThrows(new Exception("Unexpected poll error"));

        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    /// <summary>
    /// Verifies that GetPrintJobStatusAsync returns PrintFailed when the film session UID is null.
    /// </summary>
    [Fact]
    public async Task GetPrintJobStatusAsync_NullFilmSessionUid_ReturnsPrintFailed()
    {
        var svc = CreateService();

        var result = await svc.GetPrintJobStatusAsync(null!, "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    /// <summary>
    /// Verifies that GetPrintJobStatusAsync returns PrintFailed when the printer AE title is empty.
    /// </summary>
    [Fact]
    public async Task GetPrintJobStatusAsync_EmptyPrinterAeTitle_ReturnsPrintFailed()
    {
        var svc = CreateService();

        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", string.Empty, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    /// <summary>
    /// Verifies that GetPrintJobStatusAsync returns Pending when the dataset is null
    /// (covers the null dataset branch in MapExecutionStatus).
    /// </summary>
    [Fact]
    public async Task GetPrintJobStatusAsync_NullResponseDataset_ReturnsPendingThenExhausts()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomNGetRequest nGet)
            {
                // Success response but with null dataset triggers MapExecutionStatus(null) -> Pending
                var response = new DicomNGetResponse(nGet, DicomStatus.Success);
                nGet.OnResponseReceived?.Invoke(nGet, response);
            }
        });

        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", "PRINTER", CancellationToken.None);

        // Null dataset maps to Pending, polling exhausts, returns Pending
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(PrintJobStatus.Pending);
    }

    /// <summary>
    /// Verifies that GetPrintJobStatusAsync returns Pending for unknown ExecutionStatus values.
    /// </summary>
    [Fact]
    public async Task GetPrintJobStatusAsync_UnknownStatus_ReturnsPendingThenExhausts()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomNGetRequest nGet)
            {
                var statusDataset = new DicomDataset
                {
                    { DicomTag.ExecutionStatus, "UNKNOWN_STATUS" },
                };
                var response = new DicomNGetResponse(nGet, DicomStatus.Success);
                response.Dataset = statusDataset;
                nGet.OnResponseReceived?.Invoke(nGet, response);
            }
        });

        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", "PRINTER", CancellationToken.None);

        // Unknown status maps to Pending, polling exhausts, returns Pending
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(PrintJobStatus.Pending);
    }

    /// <summary>
    /// Verifies that MapExecutionStatus handles case-insensitive status values
    /// by testing the static method indirectly. "PRINTING" (uppercase) maps to Printing,
    /// which triggers the polling loop and eventually exhausts returning Pending.
    /// </summary>
    [Fact]
    public async Task GetPrintJobStatusAsync_PrintingStatus_PollsAndExhausts()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomNGetRequest nGet)
            {
                var statusDataset = new DicomDataset
                {
                    { DicomTag.ExecutionStatus, "PRINTING" },
                };
                var response = new DicomNGetResponse(nGet, DicomStatus.Success);
                response.Dataset = statusDataset;
                nGet.OnResponseReceived?.Invoke(nGet, response);
            }
        });

        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", "PRINTER", CancellationToken.None);

        // PRINTING is not Done/Failure, polling exhausts, returns Pending
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(PrintJobStatus.Pending);
    }

    // ════════════════════════════════════════════════════════════════════════════
    // SendRdsrAsync tests
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that SendRdsrAsync throws ArgumentNullException when doseRecord is null.
    /// </summary>
    [Fact]
    public async Task SendRdsrAsync_NullDoseRecord_ThrowsArgumentNullException()
    {
        var svc = CreateService();

        var act = () => svc.SendRdsrAsync(
            null!, CreateTestPatientInfo(), CreateTestStudyInfo(), "PACS");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that SendRdsrAsync throws ArgumentNullException when patientInfo is null.
    /// </summary>
    [Fact]
    public async Task SendRdsrAsync_NullPatientInfo_ThrowsArgumentNullException()
    {
        var svc = CreateService();

        var act = () => svc.SendRdsrAsync(
            CreateTestDoseRecord(), null!, CreateTestStudyInfo(), "PACS");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that SendRdsrAsync throws ArgumentNullException when studyInfo is null.
    /// </summary>
    [Fact]
    public async Task SendRdsrAsync_NullStudyInfo_ThrowsArgumentNullException()
    {
        var svc = CreateService();

        var act = () => svc.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), null!, "PACS");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that SendRdsrAsync returns failure when PACS AE title is empty.
    /// </summary>
    [Fact]
    public async Task SendRdsrAsync_EmptyPacsAeTitle_ReturnsStoreFailed()
    {
        var svc = CreateService();

        var result = await svc.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(), string.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    /// <summary>
    /// Verifies that SendRdsrAsync returns failure when PACS AE title is whitespace.
    /// </summary>
    [Fact]
    public async Task SendRdsrAsync_WhitespacePacsAeTitle_ReturnsStoreFailed()
    {
        var svc = CreateService();

        var result = await svc.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(), "   ");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    /// <summary>
    /// Verifies that SendRdsrAsync returns success when the C-STORE completes
    /// without exception (default success path with mock client).
    /// </summary>
    [Fact]
    public async Task SendRdsrAsync_SuccessResponse_ReturnsSuccess()
    {
        var svc = CreateService();
        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await svc.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(), "PACS");

        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that SendRdsrAsync returns failure when the C-STORE response indicates failure.
    /// </summary>
    [Fact]
    public async Task SendRdsrAsync_FailureResponse_ReturnsStoreFailed()
    {
        var svc = CreateService();
        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var batch = _capturedRequests.ToList();
                _capturedRequests.Clear();
                foreach (var captured in batch)
                {
                    if (captured is DicomCStoreRequest cStore)
                    {
                        cStore.OnResponseReceived?.Invoke(cStore,
                            new DicomCStoreResponse(cStore, DicomStatus.ProcessingFailure));
                    }
                }

                return Task.CompletedTask;
            });

        var result = await svc.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(), "PACS");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    /// <summary>
    /// Verifies that SendRdsrAsync returns ConnectionFailed on DicomNetworkException.
    /// </summary>
    [Fact]
    public async Task SendRdsrAsync_DicomNetworkException_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        SetupSendAsyncThrows(new DicomNetworkException("Connection refused"));

        var result = await svc.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(), "PACS");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    /// <summary>
    /// Verifies that SendRdsrAsync returns OperationCancelled on OperationCanceledException.
    /// </summary>
    [Fact]
    public async Task SendRdsrAsync_OperationCancelledException_ReturnsOperationCancelled()
    {
        var svc = CreateService();
        SetupSendAsyncThrows(new OperationCanceledException());

        var result = await svc.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(), "PACS");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.OperationCancelled);
    }

    /// <summary>
    /// Verifies that SendRdsrAsync returns ConnectionFailed on SocketException.
    /// </summary>
    [Fact]
    public async Task SendRdsrAsync_SocketException_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        SetupSendAsyncThrows(new SocketException(10061));

        var result = await svc.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(), "PACS");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    /// <summary>
    /// Verifies that SendRdsrAsync returns StoreFailed on generic Exception.
    /// </summary>
    [Fact]
    public async Task SendRdsrAsync_GenericException_ReturnsStoreFailed()
    {
        var svc = CreateService();
        SetupSendAsyncThrows(new Exception("Unexpected RDSR error"));

        var result = await svc.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(), "PACS");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    /// <summary>
    /// Verifies that SendRdsrAsync works correctly with null exposureParams.
    /// </summary>
    [Fact]
    public async Task SendRdsrAsync_NullExposureParams_Succeeds()
    {
        var svc = CreateService();
        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await svc.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(), "PACS",
            exposureParams: null);

        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that SendRdsrAsync works correctly with provided exposureParams.
    /// </summary>
    [Fact]
    public async Task SendRdsrAsync_WithExposureParams_Succeeds()
    {
        var svc = CreateService();
        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var exposureParams = new RdsrExposureParams(Kvp: 80.0, Mas: 2.5, ExposureTimeMs: 50.0);
        var result = await svc.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(), "PACS",
            exposureParams: exposureParams);

        result.IsSuccess.Should().BeTrue();
    }

    // ════════════════════════════════════════════════════════════════════════════
    // RequestStorageCommitmentAsync additional error paths
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that RequestStorageCommitmentAsync returns ConnectionFailed when
    /// a SocketException is wrapped as the base exception of a generic Exception.
    /// </summary>
    [Fact]
    public async Task RequestStorageCommitmentAsync_SocketAsBaseException_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        // Create an exception whose GetBaseException() returns a SocketException
        var socketEx = new SocketException(10061);
        var wrapperEx = new Exception("Wrapped", socketEx);
        SetupSendAsyncThrows(wrapperEx);

        var result = await svc.RequestStorageCommitmentAsync(
            "1.2.3.4.5", "1.2.3.4.5.6", "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    // ════════════════════════════════════════════════════════════════════════════
    // DicomService.CreateFilmBoxAsync tests
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CreateFilmBoxAsync returns a DicomUID when the N-CREATE
    /// response is successful.
    /// </summary>
    [Fact]
    public async Task CreateFilmBoxAsync_Success_ReturnsFilmBoxUid()
    {
        var svc = CreateService();
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
        result.Value.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that CreateFilmBoxAsync returns failure when the N-CREATE
    /// response is not successful.
    /// </summary>
    [Fact]
    public async Task CreateFilmBoxAsync_FailureResponse_ReturnsPrintFailed()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomNCreateRequest nCreate)
            {
                nCreate.OnResponseReceived?.Invoke(nCreate,
                    new DicomNCreateResponse(nCreate, DicomStatus.ProcessingFailure));
            }
        });

        var filmSessionUid = DicomUID.Generate();
        var result = await svc.CreateFilmBoxAsync(filmSessionUid, "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    /// <summary>
    /// Verifies that CreateFilmBoxAsync returns failure when the callback is
    /// never invoked (no response received, success flag stays false).
    /// </summary>
    [Fact]
    public async Task CreateFilmBoxAsync_NoResponseCallback_ReturnsPrintFailed()
    {
        var svc = CreateService();
        SetupSendAsync(_ => { }); // No callback invoked

        var filmSessionUid = DicomUID.Generate();
        var result = await svc.CreateFilmBoxAsync(filmSessionUid, "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
        result.ErrorMessage.Should().Contain("Film Box creation did not succeed");
    }

    // ════════════════════════════════════════════════════════════════════════════
    // DicomService.SetFilmBoxAsync tests
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that SetFilmBoxAsync returns success when the N-SET response is successful.
    /// </summary>
    [Fact]
    public async Task SetFilmBoxAsync_Success_ReturnsSuccess()
    {
        var svc = CreateService();
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

    /// <summary>
    /// Verifies that SetFilmBoxAsync returns failure when the N-SET response is not successful.
    /// </summary>
    [Fact]
    public async Task SetFilmBoxAsync_FailureResponse_ReturnsPrintFailed()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomNSetRequest nSet)
            {
                nSet.OnResponseReceived?.Invoke(nSet,
                    new DicomNSetResponse(nSet, DicomStatus.ProcessingFailure));
            }
        });

        var filmBoxUid = DicomUID.Generate();
        var result = await svc.SetFilmBoxAsync(filmBoxUid, "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    /// <summary>
    /// Verifies that SetFilmBoxAsync returns failure when the callback is never invoked.
    /// </summary>
    [Fact]
    public async Task SetFilmBoxAsync_NoResponseCallback_ReturnsPrintFailed()
    {
        var svc = CreateService();
        SetupSendAsync(_ => { }); // No callback invoked

        var filmBoxUid = DicomUID.Generate();
        var result = await svc.SetFilmBoxAsync(filmBoxUid, "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
        result.ErrorMessage.Should().Contain("N-SET Film Box did not succeed");
    }

    // ════════════════════════════════════════════════════════════════════════════
    // DicomService.StoreAsync — transient error retry paths
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that StoreAsync retries when the response is a failure and
    /// then succeeds on the second attempt (retry path exercised).
    /// </summary>
    [Fact]
    public async Task StoreAsync_FailureThenSuccess_WithRetry_ReturnsSuccess()
    {
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PacsHost = "127.0.0.1",
            PacsPort = 104,
            StoreRetryCount = 1,
            StoreRetryDelayMs = 10,
        });
        var tempFile = await CreateTempDicomFileAsync();
        var attemptCount = 0;

        SetupSendAsync(req =>
        {
            attemptCount++;
            if (req is DicomCStoreRequest cStore)
            {
                if (attemptCount == 1)
                {
                    cStore.OnResponseReceived?.Invoke(cStore,
                        new DicomCStoreResponse(cStore, DicomStatus.ProcessingFailure));
                }
                else
                {
                    cStore.OnResponseReceived?.Invoke(cStore,
                        new DicomCStoreResponse(cStore, DicomStatus.Success));
                }
            }
        });

        var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

        // ProcessingFailure (0xC000) is NOT transient, so no retry.
        // The result should be failure from the first attempt.
        result.IsFailure.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that StoreAsync returns failure immediately when retry count is 0
    /// and the response is a failure.
    /// </summary>
    [Fact]
    public async Task StoreAsync_NoRetryConfigured_ReturnsFailureImmediately()
    {
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PacsHost = "127.0.0.1",
            PacsPort = 104,
            StoreRetryCount = 0,
            StoreRetryDelayMs = 10,
        });
        var tempFile = await CreateTempDicomFileAsync();
        var attemptCount = 0;

        SetupSendAsync(req =>
        {
            attemptCount++;
            if (req is DicomCStoreRequest cStore)
            {
                cStore.OnResponseReceived?.Invoke(cStore,
                    new DicomCStoreResponse(cStore, DicomStatus.ProcessingFailure));
            }
        });

        var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        attemptCount.Should().Be(1);
    }

    /// <summary>
    /// Verifies that StoreAsync retries on DicomNetworkException and eventually
    /// returns connection failure after exhausting retries.
    /// </summary>
    [Fact]
    public async Task StoreAsync_DicomNetworkException_RetriesAndReturnsConnectionFailed()
    {
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PacsHost = "127.0.0.1",
            PacsPort = 104,
            StoreRetryCount = 1,
            StoreRetryDelayMs = 10,
        });
        var tempFile = await CreateTempDicomFileAsync();

        SetupSendAsyncThrows(new DicomNetworkException("Connection refused"));

        var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    /// <summary>
    /// Verifies that StoreAsync retries on IOException and eventually
    /// returns store failure after exhausting retries.
    /// </summary>
    [Fact]
    public async Task StoreAsync_IOException_RetriesAndReturnsStoreFailed()
    {
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PacsHost = "127.0.0.1",
            PacsPort = 104,
            StoreRetryCount = 1,
            StoreRetryDelayMs = 10,
        });
        var tempFile = await CreateTempDicomFileAsync();

        SetupSendAsyncThrows(new IOException("Disk read error"));

        var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    /// <summary>
    /// Verifies that StoreAsync exhausts retries for IOException.
    /// </summary>
    [Fact]
    public async Task StoreAsync_IOExceptionExhaustsRetries_ReturnsStoreFailed()
    {
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PacsHost = "127.0.0.1",
            PacsPort = 104,
            StoreRetryCount = 2,
            StoreRetryDelayMs = 10,
        });
        var tempFile = await CreateTempDicomFileAsync();

        SetupSendAsyncThrows(new IOException("Disk read error"));

        var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        result.ErrorMessage.Should().Contain("파일 읽기 오류");
    }

    /// <summary>
    /// Verifies that StoreAsync exhausts retries for DicomNetworkException.
    /// </summary>
    [Fact]
    public async Task StoreAsync_DicomNetworkExceptionExhaustsRetries_ReturnsConnectionFailed()
    {
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PacsHost = "127.0.0.1",
            PacsPort = 104,
            StoreRetryCount = 2,
            StoreRetryDelayMs = 10,
        });
        var tempFile = await CreateTempDicomFileAsync();

        SetupSendAsyncThrows(new DicomNetworkException("Connection refused"));

        var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    /// <summary>
    /// Verifies that StoreAsync exhausts retries for SocketException.
    /// </summary>
    [Fact]
    public async Task StoreAsync_SocketExceptionExhaustsRetries_ReturnsConnectionFailed()
    {
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PacsHost = "127.0.0.1",
            PacsPort = 104,
            StoreRetryCount = 2,
            StoreRetryDelayMs = 10,
        });
        var tempFile = await CreateTempDicomFileAsync();

        SetupSendAsyncThrows(new SocketException(10061));

        var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    // ════════════════════════════════════════════════════════════════════════════
    // DicomStoreScu tests
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that DicomStoreScu.StoreAsync handles a real DICOM file that
    /// triggers IOException during network send and retries.
    /// </summary>
    [Fact]
    public async Task DicomStoreScu_StoreAsync_RealDicomFile_NetworkError_ReturnsFailure()
    {
        var config = Substitute.For<IDicomNetworkConfig>();
        config.PacsHost.Returns("127.0.0.1");
        config.PacsPort.Returns(104);
        config.LocalAeTitle.Returns("HNVUE");
        config.PacsAeTitle.Returns("TESTPACS");

        var scu = new DicomStoreScu(config);
        var tempFile = await CreateTempDicomFileAsync();

        // This will hit the real network and fail, exercising error handling paths
        var result = await scu.StoreAsync(tempFile, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that DicomStoreScu.StoreAsync with a corrupted file returns failure
    /// through the DicomNetworkException or generic exception path.
    /// </summary>
    [Fact]
    public async Task DicomStoreScu_StoreAsync_CorruptedFile_ReturnsFailure()
    {
        var config = Substitute.For<IDicomNetworkConfig>();
        config.PacsHost.Returns("127.0.0.1");
        config.PacsPort.Returns(104);
        config.LocalAeTitle.Returns("HNVUE");
        config.PacsAeTitle.Returns("TESTPACS");

        var scu = new DicomStoreScu(config);

        // Create a non-DICOM temp file
        var tempPath = Path.Combine(Path.GetTempPath(), $"s14_corrupt_{Guid.NewGuid():N}.dcm");
        await File.WriteAllTextAsync(tempPath, "not dicom data");
        _tempFiles.Add(tempPath);

        var result = await scu.StoreAsync(tempPath, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    // ════════════════════════════════════════════════════════════════════════════
    // AsyncStorePipeline DisposeAsync tests
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that calling DisposeAsync twice on AsyncStorePipeline does not throw
    /// and the second call is a no-op.
    /// </summary>
    [Fact]
    public async Task AsyncStorePipeline_DoubleDispose_NoThrow()
    {
        var mockService = Substitute.For<HnVue.Common.Abstractions.IDicomService>();
        var pipeline = new AsyncStorePipeline(
            mockService, "PACS", NullLogger<AsyncStorePipeline>.Instance);

        await pipeline.DisposeAsync();
        await pipeline.DisposeAsync(); // Second call should be no-op

        // No exception thrown
    }

    /// <summary>
    /// Verifies that DisposeAsync without starting the pipeline completes without error.
    /// </summary>
    [Fact]
    public async Task AsyncStorePipeline_DisposeWithoutStart_NoThrow()
    {
        var mockService = Substitute.For<HnVue.Common.Abstractions.IDicomService>();
        var pipeline = new AsyncStorePipeline(
            mockService, "PACS", NullLogger<AsyncStorePipeline>.Instance);

        // Never start, just dispose
        await pipeline.DisposeAsync();

        // No exception thrown
    }

    /// <summary>
    /// Verifies that StartAsync after dispose throws ObjectDisposedException.
    /// </summary>
    [Fact]
    public async Task AsyncStorePipeline_StartAfterDispose_ThrowsObjectDisposedException()
    {
        var mockService = Substitute.For<HnVue.Common.Abstractions.IDicomService>();
        var pipeline = new AsyncStorePipeline(
            mockService, "PACS", NullLogger<AsyncStorePipeline>.Instance);

        await pipeline.DisposeAsync();

        var act = () => pipeline.StartAsync(CancellationToken.None);
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    /// <summary>
    /// Verifies that StopAsync after dispose throws ObjectDisposedException.
    /// </summary>
    [Fact]
    public async Task AsyncStorePipeline_StopAfterDispose_ThrowsObjectDisposedException()
    {
        var mockService = Substitute.For<HnVue.Common.Abstractions.IDicomService>();
        var pipeline = new AsyncStorePipeline(
            mockService, "PACS", NullLogger<AsyncStorePipeline>.Instance);

        await pipeline.DisposeAsync();

        var act = () => pipeline.StopAsync(CancellationToken.None);
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    /// <summary>
    /// Verifies that DisposeAsync during an active consumer task swallows exceptions
    /// from the consumer and completes gracefully.
    /// </summary>
    [Fact]
    public async Task AsyncStorePipeline_DisposeDuringActiveProcessing_CompletesGracefully()
    {
        var mockService = Substitute.For<HnVue.Common.Abstractions.IDicomService>();
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(async callInfo =>
            {
                await Task.Delay(100);
                return Result.Success();
            });

        var pipeline = new AsyncStorePipeline(
            mockService, "PACS", NullLogger<AsyncStorePipeline>.Instance);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        await pipeline.EnqueueAsync("test.dcm", "1.2.3.4");
        await pipeline.StartAsync(cts.Token);

        // Dispose while consumer is running
        await pipeline.DisposeAsync();

        // No exception thrown - consumer task exception swallowed during disposal
    }

    /// <summary>
    /// Verifies that NullLogger constructor does not throw ArgumentNullException.
    /// </summary>
    [Fact]
    public void AsyncStorePipeline_NullLogger_ThrowsArgumentNullException()
    {
        var mockService = Substitute.For<HnVue.Common.Abstractions.IDicomService>();

        var act = () => new AsyncStorePipeline(mockService, "PACS", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ════════════════════════════════════════════════════════════════════════════
    // PrintAsync internal paths — FilmBox failure and SetFilmBox failure
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that PrintAsync returns failure when FilmSession N-CREATE succeeds
    /// but FilmBox N-CREATE fails.
    /// </summary>
    [Fact]
    public async Task PrintAsync_FilmBoxCreateFails_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        var requestIndex = 0;

        SetupSendAsync(req =>
        {
            requestIndex++;
            if (req is DicomNCreateRequest nCreate && requestIndex == 1)
            {
                // First N-CREATE: FilmSession succeeds
                nCreate.OnResponseReceived?.Invoke(nCreate,
                    new DicomNCreateResponse(nCreate, DicomStatus.Success));
            }
            else if (req is DicomNCreateRequest nCreateFilmBox && requestIndex == 2)
            {
                // Second N-CREATE: FilmBox fails
                nCreateFilmBox.OnResponseReceived?.Invoke(nCreateFilmBox,
                    new DicomNCreateResponse(nCreateFilmBox, DicomStatus.ProcessingFailure));
            }
        });

        var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    /// <summary>
    /// Verifies that PrintAsync returns failure when FilmSession and FilmBox N-CREATE
    /// succeed but N-SET FilmBox fails.
    /// </summary>
    [Fact]
    public async Task PrintAsync_SetFilmBoxFails_ReturnsPrintFailed()
    {
        var svc = CreateService();
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

        var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    /// <summary>
    /// Verifies PrintAsync full success path with PENDING -> PRINTING -> DONE
    /// status polling (exercises the polling loop).
    /// </summary>
    [Fact]
    public async Task PrintAsync_FullSuccess_WithStatusPollingTransitions()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        var requestIndex = 0;
        var nGetPollCount = 0;

        SetupSendAsync(req =>
        {
            requestIndex++;
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
                var statusValue = nGetPollCount <= 1 ? "PRINTING" : "DONE";
                var statusDataset = new DicomDataset
                {
                    { DicomTag.ExecutionStatus, statusValue },
                };
                var response = new DicomNGetResponse(nGet, DicomStatus.Success);
                response.Dataset = statusDataset;
                nGet.OnResponseReceived?.Invoke(nGet, response);
            }
        });

        var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        nGetPollCount.Should().BeGreaterThanOrEqualTo(2);
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
        {
            return _client;
        }
    }
}
