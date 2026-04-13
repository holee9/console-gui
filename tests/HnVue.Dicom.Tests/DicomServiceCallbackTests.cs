using System.IO;
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using FluentAssertions;
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
/// Tests DicomService success paths and OnResponseReceived callback execution
/// using a testable subclass that injects a mock DICOM client.
/// SWR-DICOM-020.
/// </summary>
[Trait("SWR", "SWR-DICOM-020")]
public sealed class DicomServiceCallbackTests
{
    private readonly IDicomClient _mockClient;
    private readonly List<DicomRequest> _capturedRequests = [];

    public DicomServiceCallbackTests()
    {
        _mockClient = Substitute.For<IDicomClient>();
        _mockClient.AddRequestAsync(Arg.Any<DicomRequest>())
            .Returns(call =>
            {
                _capturedRequests.Add(call.Arg<DicomRequest>());
                return Task.CompletedTask;
            });
    }

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
        });
        return new TestableDicomService(opts, NullLogger<DicomService>.Instance, _mockClient);
    }

    private void SetupSendAsync(Action<DicomRequest> callback)
    {
        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                foreach (var captured in _capturedRequests)
                {
                    callback(captured);
                }

                return Task.CompletedTask;
            });
    }

    private static async Task<string> CreateTempDicomFileAsync()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
            { DicomTag.PatientID, "TEST001" },
            { DicomTag.PatientName, "Test^Patient" },
        };
        var dicomFile = new DicomFile(dataset);
        var tempPath = Path.Combine(Path.GetTempPath(), $"dicom_svc_test_{Guid.NewGuid():N}.dcm");
        await dicomFile.SaveAsync(tempPath);
        return tempPath;
    }

    private static WorklistQuery CreateWorklistQuery(string aeTitle = "MWL_SCP")
    {
        return new WorklistQuery(null, null, null, aeTitle);
    }

    // ── StoreAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task StoreAsync_SuccessResponse_ReturnsSuccess()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            SetupSendAsync(req =>
            {
                if (req is DicomCStoreRequest cStore)
                {
                    cStore.OnResponseReceived?.Invoke(cStore,
                        new DicomCStoreResponse(cStore, DicomStatus.Success));
                }
            });

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_FailureResponse_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            SetupSendAsync(req =>
            {
                if (req is DicomCStoreRequest cStore)
                {
                    cStore.OnResponseReceived?.Invoke(cStore,
                        new DicomCStoreResponse(cStore, DicomStatus.ProcessingFailure));
                }
            });

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_NetworkError_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            _mockClient.SendAsync(Arg.Any<CancellationToken>())
                .Returns(_ => throw new DicomNetworkException("Connection refused"));

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_SocketError_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            _mockClient.SendAsync(Arg.Any<CancellationToken>())
                .Returns(_ => throw new System.Net.Sockets.SocketException(10061));

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_Cancelled_ReturnsOperationCancelled()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            _mockClient.SendAsync(Arg.Any<CancellationToken>())
                .Returns(_ => throw new OperationCanceledException());

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.OperationCancelled);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_IOException_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            _mockClient.SendAsync(Arg.Any<CancellationToken>())
                .Returns(_ => throw new IOException("Disk error"));

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_GenericException_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            _mockClient.SendAsync(Arg.Any<CancellationToken>())
                .Returns(_ => throw new Exception("Unexpected error"));

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── QueryWorklistAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task QueryWorklistAsync_SuccessWithPendingItems_ReturnsWorklistItems()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomCFindRequest cFind)
            {
                var pendingDataset = new DicomDataset
                {
                    { DicomTag.AccessionNumber, "ACC001" },
                    { DicomTag.PatientID, "PAT001" },
                    { DicomTag.PatientName, "Test^Patient" },
                    { DicomTag.StudyDate, "20260413" },
                };
                var pending = new DicomCFindResponse(cFind, DicomStatus.Pending);
                pending.Dataset = pendingDataset;
                cFind.OnResponseReceived?.Invoke(cFind, pending);

                cFind.OnResponseReceived?.Invoke(cFind,
                    new DicomCFindResponse(cFind, DicomStatus.Success));
            }
        });

        var result = await svc.QueryWorklistAsync(CreateWorklistQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].PatientId.Should().Be("PAT001");
    }

    [Fact]
    public async Task QueryWorklistAsync_EmptyResult_ReturnsEmptyList()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomCFindRequest cFind)
            {
                cFind.OnResponseReceived?.Invoke(cFind,
                    new DicomCFindResponse(cFind, DicomStatus.Success));
            }
        });

        var result = await svc.QueryWorklistAsync(CreateWorklistQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryWorklistAsync_EmptyAeTitle_ReturnsQueryFailed()
    {
        var svc = CreateService();
        var query = new WorklistQuery(null, null, null, string.Empty);

        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomQueryFailed);
    }

    [Fact]
    public async Task QueryWorklistAsync_NetworkError_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(_ => throw new DicomNetworkException("Connection failed"));

        var result = await svc.QueryWorklistAsync(CreateWorklistQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    public async Task QueryWorklistAsync_Cancelled_ReturnsOperationCancelled()
    {
        var svc = CreateService();
        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(_ => throw new OperationCanceledException());

        var result = await svc.QueryWorklistAsync(CreateWorklistQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.OperationCancelled);
    }

    [Fact]
    public async Task QueryWorklistAsync_SocketError_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(_ => throw new System.Net.Sockets.SocketException(10061));

        var result = await svc.QueryWorklistAsync(CreateWorklistQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    public async Task QueryWorklistAsync_GenericException_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(_ => throw new Exception("Unexpected error"));

        var result = await svc.QueryWorklistAsync(CreateWorklistQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    // ── PrintAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task PrintAsync_BothSucceed_ReturnsSuccess()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            SetupSendAsync(req =>
            {
                if (req is DicomNCreateRequest nCreate)
                {
                    nCreate.OnResponseReceived?.Invoke(nCreate,
                        new DicomNCreateResponse(nCreate, DicomStatus.Success));
                }
                else if (req is DicomNActionRequest nAction)
                {
                    nAction.OnResponseReceived?.Invoke(nAction,
                        new DicomNActionResponse(nAction, DicomStatus.Success));
                }
            });

            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_NCreateFails_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            SetupSendAsync(req =>
            {
                if (req is DicomNCreateRequest nCreate)
                {
                    nCreate.OnResponseReceived?.Invoke(nCreate,
                        new DicomNCreateResponse(nCreate, DicomStatus.ProcessingFailure));
                }
            });

            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomPrintFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_NActionFails_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            var requestIndex = 0;
            SetupSendAsync(req =>
            {
                requestIndex++;
                if (req is DicomNCreateRequest nCreate && requestIndex <= 1)
                {
                    nCreate.OnResponseReceived?.Invoke(nCreate,
                        new DicomNCreateResponse(nCreate, DicomStatus.Success));
                }
                else if (req is DicomNActionRequest nAction)
                {
                    nAction.OnResponseReceived?.Invoke(nAction,
                        new DicomNActionResponse(nAction, DicomStatus.ProcessingFailure));
                }
            });

            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomPrintFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_EmptyFilePath_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync(string.Empty, "PRINTER", CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    [Fact]
    public async Task PrintAsync_EmptyAeTitle_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync("some.dcm", string.Empty, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    [Fact]
    public async Task PrintAsync_Cancelled_ReturnsOperationCancelled()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            _mockClient.SendAsync(Arg.Any<CancellationToken>())
                .Returns(_ => throw new OperationCanceledException());

            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.OperationCancelled);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_IOException_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            _mockClient.SendAsync(Arg.Any<CancellationToken>())
                .Returns(_ => throw new IOException("Disk error"));

            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomPrintFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_NetworkError_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            _mockClient.SendAsync(Arg.Any<CancellationToken>())
                .Returns(_ => throw new DicomNetworkException("Network error"));

            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_SocketError_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            _mockClient.SendAsync(Arg.Any<CancellationToken>())
                .Returns(_ => throw new System.Net.Sockets.SocketException(10061));

            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_GenericException_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            _mockClient.SendAsync(Arg.Any<CancellationToken>())
                .Returns(_ => throw new Exception("Unexpected error"));

            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomPrintFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_NCreateNoSuccessFlag_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            SetupSendAsync(_ => { });

            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomPrintFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── RequestStorageCommitmentAsync ───────────────────────────────────────────

    [Fact]
    public async Task StorageCommitment_SuccessResponse_ReturnsSuccess()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomNActionRequest nAction)
            {
                nAction.OnResponseReceived?.Invoke(nAction,
                    new DicomNActionResponse(nAction, DicomStatus.Success));
            }
        });

        var result = await svc.RequestStorageCommitmentAsync(
            "1.2.3.4.5", "1.2.3.4.5.6", "PACS", CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task StorageCommitment_FailureResponse_ReturnsStoreFailed()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomNActionRequest nAction)
            {
                nAction.OnResponseReceived?.Invoke(nAction,
                    new DicomNActionResponse(nAction, DicomStatus.ProcessingFailure));
            }
        });

        var result = await svc.RequestStorageCommitmentAsync(
            "1.2.3.4.5", "1.2.3.4.5.6", "PACS", CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    [Fact]
    public async Task StorageCommitment_NullPacsHost_ReturnsConnectionFailed()
    {
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PacsHost = null!,
        });

        var result = await svc.RequestStorageCommitmentAsync(
            "1.2.3.4.5", "1.2.3.4.5.6", "PACS", CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    public async Task StorageCommitment_NullSopClassUid_ThrowsArgumentNullException()
    {
        var svc = CreateService();
        var act = () => svc.RequestStorageCommitmentAsync(null!, "1.2.3", "PACS", CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task StorageCommitment_NullSopInstanceUid_ThrowsArgumentNullException()
    {
        var svc = CreateService();
        var act = () => svc.RequestStorageCommitmentAsync("1.2.3", null!, "PACS", CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task StorageCommitment_NullAeTitle_ThrowsArgumentNullException()
    {
        var svc = CreateService();
        var act = () => svc.RequestStorageCommitmentAsync("1.2.3", "1.2.3.4", null!, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task StorageCommitment_NetworkError_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(_ => throw new DicomNetworkException("Network error"));

        var result = await svc.RequestStorageCommitmentAsync(
            "1.2.3.4.5", "1.2.3.4.5.6", "PACS", CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    public async Task StorageCommitment_Cancelled_ReturnsOperationCancelled()
    {
        var svc = CreateService();
        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(_ => throw new OperationCanceledException());

        var result = await svc.RequestStorageCommitmentAsync(
            "1.2.3.4.5", "1.2.3.4.5.6", "PACS", CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.OperationCancelled);
    }

    [Fact]
    public async Task StorageCommitment_SocketError_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(_ => throw new System.Net.Sockets.SocketException(10061));

        var result = await svc.RequestStorageCommitmentAsync(
            "1.2.3.4.5", "1.2.3.4.5.6", "PACS", CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    public async Task StorageCommitment_GenericException_ReturnsStoreFailed()
    {
        var svc = CreateService();
        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(_ => throw new Exception("Unexpected error"));

        var result = await svc.RequestStorageCommitmentAsync(
            "1.2.3.4.5", "1.2.3.4.5.6", "PACS", CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    // ── MapToWorklistItem via QueryWorklistAsync ────────────────────────────────

    [Fact]
    public async Task QueryWorklistAsync_WorklistItemWithBodyPart_ReturnsMappedItem()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomCFindRequest cFind)
            {
                var dataset = new DicomDataset
                {
                    { DicomTag.AccessionNumber, "ACC002" },
                    { DicomTag.PatientID, "PAT002" },
                    { DicomTag.PatientName, "Body^Part" },
                    { DicomTag.StudyDate, "20260401" },
                    { DicomTag.RequestedProcedureDescription, "Chest X-Ray" },
                };
                var spsItem = new DicomDataset
                {
                    { DicomTag.BodyPartExamined, "CHEST" },
                };
                dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

                var pending = new DicomCFindResponse(cFind, DicomStatus.Pending);
                pending.Dataset = dataset;
                cFind.OnResponseReceived?.Invoke(cFind, pending);
                cFind.OnResponseReceived?.Invoke(cFind,
                    new DicomCFindResponse(cFind, DicomStatus.Success));
            }
        });

        var result = await svc.QueryWorklistAsync(CreateWorklistQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].BodyPart.Should().Be("CHEST");
        result.Value[0].RequestedProcedure.Should().Be("Chest X-Ray");
    }

    [Fact]
    public async Task QueryWorklistAsync_MultiplePendingItems_ReturnsAll()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomCFindRequest cFind)
            {
                for (var i = 1; i <= 3; i++)
                {
                    var dataset = new DicomDataset
                    {
                        { DicomTag.AccessionNumber, $"ACC{i:D3}" },
                        { DicomTag.PatientID, $"PAT{i:D3}" },
                        { DicomTag.PatientName, $"Patient^{i}" },
                        { DicomTag.StudyDate, "20260413" },
                    };
                    var pending = new DicomCFindResponse(cFind, DicomStatus.Pending);
                    pending.Dataset = dataset;
                    cFind.OnResponseReceived?.Invoke(cFind, pending);
                }

                cFind.OnResponseReceived?.Invoke(cFind,
                    new DicomCFindResponse(cFind, DicomStatus.Success));
            }
        });

        var result = await svc.QueryWorklistAsync(CreateWorklistQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    // ── BuildWorklistRequest ────────────────────────────────────────────────────

    [Fact]
    public void BuildWorklistRequest_DateFromOnly_CreatesRequest()
    {
        var query = new WorklistQuery(null, new DateOnly(2026, 4, 1), null, "MWL_SCP");
        var request = DicomService.BuildWorklistRequest(query);
        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_DateToOnly_CreatesRequest()
    {
        var query = new WorklistQuery(null, null, new DateOnly(2026, 4, 30), "MWL_SCP");
        var request = DicomService.BuildWorklistRequest(query);
        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_BothDates_CreatesRequest()
    {
        var query = new WorklistQuery(null, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30), "MWL_SCP");
        var request = DicomService.BuildWorklistRequest(query);
        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_NoDates_CreatesRequest()
    {
        var query = CreateWorklistQuery();
        var request = DicomService.BuildWorklistRequest(query);
        request.Should().NotBeNull();
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
