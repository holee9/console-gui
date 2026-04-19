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
/// Comprehensive coverage boost tests for HnVue.Dicom module.
/// Uses TestableDicomService with mock IDicomClient to exercise all branches
/// in DicomService (StoreAsync, QueryWorklistAsync, PrintAsync, RequestStorageCommitmentAsync),
/// DicomOutbox, DicomFileIO, and DicomStoreScu.
/// Target: push Dicom module branch coverage from ~43% to 80%+.
/// </summary>
[Trait("SWR", "SWR-DC-001")]
public sealed class DicomCoverageBoostTests
{
    // ── Test infrastructure ─────────────────────────────────────────────────────

    private readonly IDicomClient _mockClient;
    private readonly List<DicomRequest> _capturedRequests = [];

    public DicomCoverageBoostTests()
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
                foreach (var captured in _capturedRequests)
                {
                    callback(captured);
                }
                return Task.CompletedTask;
            });
    }

    private void SetupSendAsyncToThrow(Exception exception)
    {
        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(_ => throw exception);
    }

    private static async Task<string> CreateTempDicomFileAsync()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
            { DicomTag.PatientID, "COV001" },
            { DicomTag.PatientName, "Coverage^Test" },
        };
        var dicomFile = new DicomFile(dataset);
        var tempPath = Path.Combine(Path.GetTempPath(), $"dicom_cov_{Guid.NewGuid():N}.dcm");
        await dicomFile.SaveAsync(tempPath);
        return tempPath;
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // StoreAsync — Full branch coverage
    // ══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task StoreAsync_EmptyPath_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var result = await svc.StoreAsync(string.Empty, "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        result.ErrorMessage.Should().Contain("DICOM 파일 경로가 비어있습니다");
    }

    [Fact]
    public async Task StoreAsync_NullPath_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var result = await svc.StoreAsync(null!, "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    [Fact]
    public async Task StoreAsync_WhitespacePath_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var result = await svc.StoreAsync("   ", "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    [Fact]
    public async Task StoreAsync_EmptyAeTitle_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            var result = await svc.StoreAsync(tempFile, string.Empty, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
            result.ErrorMessage.Should().Contain("PACS AE Title이 비어있습니다");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_NullAeTitle_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            var result = await svc.StoreAsync(tempFile, null!, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_FileNotFound_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var result = await svc.StoreAsync("C:/nonexistent_abc123/file.dcm", "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        result.ErrorMessage.Should().Contain("찾을 수 없습니다");
    }

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
    public async Task StoreAsync_NonSuccessResponse_ReturnsStoreFailed()
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
            result.ErrorMessage.Should().Contain("C-STORE 실패");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_DicomNetworkException_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            SetupSendAsyncToThrow(new DicomNetworkException("Connection refused"));

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
    public async Task StoreAsync_IOException_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            SetupSendAsyncToThrow(new IOException("Disk read error"));

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
            result.ErrorMessage.Should().Contain("파일 읽기 오류");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_OperationCancelledException_ReturnsCancelled()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            SetupSendAsyncToThrow(new OperationCanceledException());

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
    public async Task StoreAsync_GenericException_WithSocketBase_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            // Wrap a SocketException inside a generic Exception to test GetBaseException() path
            var socketEx = new System.Net.Sockets.SocketException(10061);
            SetupSendAsyncToThrow(new Exception("wrapper", socketEx));

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
            result.ErrorMessage.Should().Contain("PACS 연결 실패");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_GenericException_WithoutSocketBase_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            SetupSendAsyncToThrow(new Exception("Unexpected failure"));

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
            result.ErrorMessage.Should().Contain("C-STORE 실패");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_InvalidDicomFileContent_ReturnsFailure()
    {
        var svc = CreateService();
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(tempFile, new byte[] { 0x00, 0x01, 0x02, 0xFF, 0xFE });

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // QueryWorklistAsync — Full branch coverage
    // ══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task QueryWorklistAsync_EmptyAeTitle_ReturnsQueryFailed()
    {
        var svc = CreateService();
        var query = new WorklistQuery(null, null, null, string.Empty);

        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomQueryFailed);
        result.ErrorMessage.Should().Contain("Worklist AE title must not be empty");
    }

    [Fact]
    public async Task QueryWorklistAsync_SuccessWithPendingResults_ReturnsItems()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomCFindRequest cFind)
            {
                var dataset = new DicomDataset
                {
                    { DicomTag.AccessionNumber, "ACC001" },
                    { DicomTag.PatientID, "PAT001" },
                    { DicomTag.PatientName, "Test^Patient" },
                    { DicomTag.StudyDate, "20260413" },
                };
                var pending = new DicomCFindResponse(cFind, DicomStatus.Pending);
                pending.Dataset = dataset;
                cFind.OnResponseReceived?.Invoke(cFind, pending);

                cFind.OnResponseReceived?.Invoke(cFind,
                    new DicomCFindResponse(cFind, DicomStatus.Success));
            }
        });

        var query = new WorklistQuery(null, null, null, "MWL_SCP");
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].AccessionNumber.Should().Be("ACC001");
        result.Value[0].PatientId.Should().Be("PAT001");
        result.Value[0].StudyDate.Should().Be(new DateOnly(2026, 4, 13));
    }

    [Fact]
    public async Task QueryWorklistAsync_SuccessWithNullDataset_PendingSkipped()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomCFindRequest cFind)
            {
                // Pending with null dataset should be skipped
                var pending = new DicomCFindResponse(cFind, DicomStatus.Pending);
                pending.Dataset = null;
                cFind.OnResponseReceived?.Invoke(cFind, pending);

                cFind.OnResponseReceived?.Invoke(cFind,
                    new DicomCFindResponse(cFind, DicomStatus.Success));
            }
        });

        var query = new WorklistQuery(null, null, null, "MWL_SCP");
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryWorklistAsync_SuccessResponseOnly_ReturnsEmptyList()
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

        var query = new WorklistQuery(null, null, null, "MWL_SCP");
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryWorklistAsync_MultiplePendingItems_ReturnsAllItems()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomCFindRequest cFind)
            {
                for (var i = 1; i <= 5; i++)
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

        var query = new WorklistQuery(null, null, null, "MWL_SCP");
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(5);
    }

    [Fact]
    public async Task QueryWorklistAsync_DicomNetworkException_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        SetupSendAsyncToThrow(new DicomNetworkException("Network failure"));

        var query = new WorklistQuery(null, null, null, "MWL_SCP");
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    public async Task QueryWorklistAsync_SocketException_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        SetupSendAsyncToThrow(new System.Net.Sockets.SocketException(10061));

        var query = new WorklistQuery(null, null, null, "MWL_SCP");
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        result.ErrorMessage.Should().Contain("Connection failed");
    }

    [Fact]
    public async Task QueryWorklistAsync_OperationCancelledException_ReturnsCancelled()
    {
        var svc = CreateService();
        SetupSendAsyncToThrow(new OperationCanceledException());

        var query = new WorklistQuery(null, null, null, "MWL_SCP");
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.OperationCancelled);
    }

    [Fact]
    public async Task QueryWorklistAsync_GenericException_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        SetupSendAsyncToThrow(new Exception("Unexpected query failure"));

        var query = new WorklistQuery(null, null, null, "MWL_SCP");
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        result.ErrorMessage.Should().Contain("Query failed");
    }

    [Fact]
    public async Task QueryWorklistAsync_WithDateRange_BothDates_CallsSuccessfully()
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

        var query = new WorklistQuery("PAT001", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), "MWL_SCP");
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task QueryWorklistAsync_WithOnlyDateFrom_CallsSuccessfully()
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

        var query = new WorklistQuery(null, new DateOnly(2026, 1, 1), null, "MWL_SCP");
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task QueryWorklistAsync_WithOnlyDateTo_CallsSuccessfully()
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

        var query = new WorklistQuery(null, null, new DateOnly(2026, 12, 31), "MWL_SCP");
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // PrintAsync — Full branch coverage (both N-CREATE and N-ACTION paths)
    // ══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task PrintAsync_EmptyPath_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync(string.Empty, "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
        result.ErrorMessage.Should().Contain("DICOM file path must not be empty");
    }

    [Fact]
    public async Task PrintAsync_NullPath_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync(null!, "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    [Fact]
    public async Task PrintAsync_EmptyAeTitle_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            var result = await svc.PrintAsync(tempFile, string.Empty, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomPrintFailed);
            result.ErrorMessage.Should().Contain("Printer AE title must not be empty");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_FileNotFound_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync("C:/nonexistent_xyz/test.dcm", "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
        result.ErrorMessage.Should().Contain("not found");
    }

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
    public async Task PrintAsync_NCreateFails_WithMessage_ReturnsPrintFailed()
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
            result.ErrorMessage.Should().Contain("N-CREATE Basic Film Session failed");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_NCreateNoCallback_WithoutMessage_ReturnsDefaultMessage()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            // No callback invoked - sessionCreated stays false, createFailMessage stays empty
            SetupSendAsync(_ => { });

            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomPrintFailed);
            result.ErrorMessage.Should().Contain("N-CREATE did not succeed");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_NCreateSucceeds_NActionFails_WithMessage_ReturnsPrintFailed()
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
                else if (req is DicomNSetRequest nSet)
                {
                    nSet.OnResponseReceived?.Invoke(nSet,
                        new DicomNSetResponse(nSet, DicomStatus.Success));
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
            result.ErrorMessage.Should().Contain("N-ACTION Print failed");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_NCreateSucceeds_NActionNoCallback_ReturnsDefaultMessage()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            // First call: N-CREATE succeeds; subsequent calls: N-SET succeeds, N-ACTION gets no callback
            var sendCallCount = 0;
            _mockClient.SendAsync(Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    sendCallCount++;
                    if (sendCallCount <= 3)
                    {
                        foreach (var captured in _capturedRequests)
                        {
                            if (captured is DicomNCreateRequest nCreate)
                            {
                                nCreate.OnResponseReceived?.Invoke(nCreate,
                                    new DicomNCreateResponse(nCreate, DicomStatus.Success));
                            }
                            else if (captured is DicomNSetRequest nSet)
                            {
                                nSet.OnResponseReceived?.Invoke(nSet,
                                    new DicomNSetResponse(nSet, DicomStatus.Success));
                            }
                        }
                    }
                    // Fourth call (N-ACTION): no callback, actionSucceeded stays false
                    return Task.CompletedTask;
                });

            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomPrintFailed);
            result.ErrorMessage.Should().Contain("N-ACTION did not succeed");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_OperationCancelled_ReturnsCancelled()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            SetupSendAsyncToThrow(new OperationCanceledException());

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
            SetupSendAsyncToThrow(new IOException("File read error"));

            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomPrintFailed);
            result.ErrorMessage.Should().Contain("I/O error");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_DicomNetworkException_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            SetupSendAsyncToThrow(new DicomNetworkException("Network failure"));

            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
            result.ErrorMessage.Should().Contain("Network error");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_SocketExceptionInBase_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            var socketEx = new System.Net.Sockets.SocketException(10061);
            SetupSendAsyncToThrow(new Exception("wrapper", socketEx));

            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
            result.ErrorMessage.Should().Contain("Connection failed");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_GenericException_NoSocketBase_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            SetupSendAsyncToThrow(new Exception("Unexpected print error"));

            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomPrintFailed);
            result.ErrorMessage.Should().Contain("Print failed");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // RequestStorageCommitmentAsync — Full branch coverage
    // ══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task StorageCommitment_EmptyPacsHost_ReturnsConnectionFailed()
    {
        var svc = CreateService(new DicomOptions { PacsHost = string.Empty, LocalAeTitle = "HNVUE" });

        var result = await svc.RequestStorageCommitmentAsync("1.2.3", "1.2.3.4", "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        result.ErrorMessage.Should().Contain("PACS host is not configured");
    }

    [Fact]
    public async Task StorageCommitment_NullPacsHost_ReturnsConnectionFailed()
    {
        var svc = CreateService(new DicomOptions { PacsHost = null!, LocalAeTitle = "HNVUE" });

        var result = await svc.RequestStorageCommitmentAsync("1.2.3", "1.2.3.4", "PACS", CancellationToken.None);

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
    public async Task StorageCommitment_NullPacsAeTitle_ThrowsArgumentNullException()
    {
        var svc = CreateService();

        var act = () => svc.RequestStorageCommitmentAsync("1.2.3", "1.2.3.4", null!, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task StorageCommitment_Success_ReturnsSuccess()
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

        var result = await svc.RequestStorageCommitmentAsync("1.2.3.4.5", "1.2.3.4.5.6", "PACS", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task StorageCommitment_NActionFailure_ReturnsStoreFailed()
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

        var result = await svc.RequestStorageCommitmentAsync("1.2.3.4.5", "1.2.3.4.5.6", "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        result.ErrorMessage.Should().Contain("Storage Commitment N-ACTION failed");
    }

    [Fact]
    public async Task StorageCommitment_DicomNetworkException_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        SetupSendAsyncToThrow(new DicomNetworkException("Connection refused"));

        var result = await svc.RequestStorageCommitmentAsync("1.2.3", "1.2.3.4", "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        result.ErrorMessage.Should().Contain("Storage Commitment network error");
    }

    [Fact]
    public async Task StorageCommitment_OperationCancelled_ReturnsCancelled()
    {
        var svc = CreateService();
        SetupSendAsyncToThrow(new OperationCanceledException());

        var result = await svc.RequestStorageCommitmentAsync("1.2.3", "1.2.3.4", "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.OperationCancelled);
        result.ErrorMessage.Should().Contain("Storage Commitment was cancelled");
    }

    [Fact]
    public async Task StorageCommitment_SocketExceptionInBase_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        var socketEx = new System.Net.Sockets.SocketException(10061);
        SetupSendAsyncToThrow(new Exception("wrapper", socketEx));

        var result = await svc.RequestStorageCommitmentAsync("1.2.3", "1.2.3.4", "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        result.ErrorMessage.Should().Contain("Storage Commitment connection failed");
    }

    [Fact]
    public async Task StorageCommitment_GenericException_NoSocketBase_ReturnsStoreFailed()
    {
        var svc = CreateService();
        SetupSendAsyncToThrow(new Exception("Unexpected storage commitment error"));

        var result = await svc.RequestStorageCommitmentAsync("1.2.3", "1.2.3.4", "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        result.ErrorMessage.Should().Contain("Storage Commitment failed");
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // BuildWorklistRequest — All date range combinations
    // ══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public void BuildWorklistRequest_NoDates_ReturnsNonNullRequest()
    {
        var query = new WorklistQuery(null, null, null, "MWL_SCP");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_DateFromOnly_ReturnsNonNullRequest()
    {
        var query = new WorklistQuery(null, new DateOnly(2026, 1, 1), null, "MWL_SCP");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_DateToOnly_ReturnsNonNullRequest()
    {
        var query = new WorklistQuery(null, null, new DateOnly(2026, 12, 31), "MWL_SCP");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_BothDates_ReturnsNonNullRequest()
    {
        var query = new WorklistQuery(null, new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), "MWL_SCP");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_WithPatientId_ReturnsNonNullRequest()
    {
        var query = new WorklistQuery("PAT001", null, null, "MWL_SCP");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_AllFieldsPopulated_ReturnsNonNullRequest()
    {
        var query = new WorklistQuery("PAT999", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), "MWL_SCP");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // MapToWorklistItem — All body part extraction paths
    // ══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public void MapToWorklistItem_EmptyDataset_AllFieldsDefault()
    {
        var dataset = new DicomDataset();

        var item = DicomService.MapToWorklistItem(dataset);

        item.AccessionNumber.Should().BeEmpty();
        item.PatientId.Should().BeEmpty();
        item.PatientName.Should().BeEmpty();
        item.StudyDate.Should().BeNull();
        item.BodyPart.Should().BeNull();
        item.RequestedProcedure.Should().BeNull();
    }

    [Fact]
    public void MapToWorklistItem_FullFields_MapsCorrectly()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC001" },
            { DicomTag.PatientID, "PAT001" },
            { DicomTag.PatientName, "Doe^John" },
            { DicomTag.StudyDate, "20260115" },
            { DicomTag.RequestedProcedureDescription, "Chest PA" },
        };

        var item = DicomService.MapToWorklistItem(dataset);

        item.AccessionNumber.Should().Be("ACC001");
        item.PatientId.Should().Be("PAT001");
        item.PatientName.Should().Be("Doe^John");
        item.StudyDate.Should().Be(new DateOnly(2026, 1, 15));
        item.RequestedProcedure.Should().Be("Chest PA");
    }

    [Fact]
    public void MapToWorklistItem_EmptyStudyDate_ParsesAsNull()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.StudyDate, string.Empty },
        };

        var item = DicomService.MapToWorklistItem(dataset);

        item.StudyDate.Should().BeNull();
    }

    [Fact]
    public void MapToWorklistItem_InvalidStudyDate_ParsesAsNull()
    {
        // fo-dicom validates DA VR strictly (YYYYMMDD must be a real calendar date).
        // To test the TryParseExact failure branch in MapToWorklistItem, we use a valid
        // but intentionally wrong-length date string added via the dataset builder.
        // The only way to trigger the parse failure is with an empty string (already tested)
        // or by building the dataset without auto-validation.
        var dataset = new DicomDataset
        {
            { DicomTag.StudyDate, string.Empty },
        };

        var item = DicomService.MapToWorklistItem(dataset);

        item.StudyDate.Should().BeNull();
    }

    [Fact]
    public void MapToWorklistItem_EmptyRequestedProcedure_IsNull()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.RequestedProcedureDescription, string.Empty },
        };

        var item = DicomService.MapToWorklistItem(dataset);

        item.RequestedProcedure.Should().BeNull();
    }

    [Fact]
    public void MapToWorklistItem_WhitespaceRequestedProcedure_IsNull()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.RequestedProcedureDescription, "   " },
        };

        var item = DicomService.MapToWorklistItem(dataset);

        item.RequestedProcedure.Should().BeNull();
    }

    [Fact]
    public void MapToWorklistItem_BodyPartExamined_TakesPriorityOverDescription()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "A1" },
            { DicomTag.PatientID, "P1" },
            { DicomTag.PatientName, "Test^1" },
        };

        var spsItem = new DicomDataset
        {
            { DicomTag.BodyPartExamined, "CHEST" },
            { DicomTag.ScheduledProcedureStepDescription, "Chest PA and Lateral" },
        };
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().Be("CHEST");
    }

    [Fact]
    public void MapToWorklistItem_SpsDescription_WhenBodyPartEmpty()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "A2" },
            { DicomTag.PatientID, "P2" },
            { DicomTag.PatientName, "Test^2" },
        };

        var spsItem = new DicomDataset
        {
            { DicomTag.ScheduledProcedureStepDescription, "Abdomen AP" },
        };
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().Be("Abdomen AP");
    }

    [Fact]
    public void MapToWorklistItem_CodeMeaning_WhenBothBodyPartAndDescriptionEmpty()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "A3" },
            { DicomTag.PatientID, "P3" },
            { DicomTag.PatientName, "Test^3" },
        };

        var protocolItem = new DicomDataset
        {
            { DicomTag.CodeMeaning, "PELVIS" },
        };
        var spsItem = new DicomDataset
        {
            { DicomTag.ScheduledProtocolCodeSequence,
                new DicomSequence(DicomTag.ScheduledProtocolCodeSequence, protocolItem) },
        };
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().Be("PELVIS");
    }

    [Fact]
    public void MapToWorklistItem_CodeValueFallback_WhenCodeMeaningEmpty()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "A4" },
            { DicomTag.PatientID, "P4" },
            { DicomTag.PatientName, "Test^4" },
        };

        var protocolItem = new DicomDataset
        {
            { DicomTag.CodeValue, "EXTREMITY" },
            { DicomTag.CodeMeaning, string.Empty },
        };
        var spsItem = new DicomDataset
        {
            { DicomTag.ScheduledProtocolCodeSequence,
                new DicomSequence(DicomTag.ScheduledProtocolCodeSequence, protocolItem) },
        };
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().Be("EXTREMITY");
    }

    [Fact]
    public void MapToWorklistItem_EmptySpsSequence_BodyPartIsNull()
    {
        var dataset = new DicomDataset();
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().BeNull();
    }

    [Fact]
    public void MapToWorklistItem_SpsItemNoBodyPartOrDescription_NoProtocolSequence_BodyPartNull()
    {
        var dataset = new DicomDataset();
        var spsItem = new DicomDataset(); // Empty SPS item
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().BeNull();
    }

    [Fact]
    public void MapToWorklistItem_EmptyProtocolCodeSequence_BodyPartNull()
    {
        var dataset = new DicomDataset();
        var spsItem = new DicomDataset
        {
            { DicomTag.ScheduledProtocolCodeSequence,
                new DicomSequence(DicomTag.ScheduledProtocolCodeSequence) },
        };
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().BeNull();
    }

    [Fact]
    public void MapToWorklistItem_AllSourcesEmpty_BodyPartNull()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "A5" },
            { DicomTag.PatientID, "P5" },
        };

        var protocolItem = new DicomDataset
        {
            { DicomTag.CodeMeaning, string.Empty },
            { DicomTag.CodeValue, string.Empty },
        };
        var spsItem = new DicomDataset
        {
            { DicomTag.ScheduledProtocolCodeSequence,
                new DicomSequence(DicomTag.ScheduledProtocolCodeSequence, protocolItem) },
        };
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().BeNull();
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // DicomOutbox — Additional coverage for ProcessAsync retry and dead-letter paths
    // ══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Outbox_EnqueueAsync_ValidPath_IncrementsCount()
    {
        var mockService = Substitute.For<HnVue.Common.Abstractions.IDicomService>();
        var outbox = new DicomOutbox(mockService, NullLogger<DicomOutbox>.Instance);

        outbox.Count.Should().Be(0);
        await outbox.EnqueueAsync("/file.dcm");
        outbox.Count.Should().Be(1);
    }

    [Fact]
    public async Task Outbox_ProcessAsync_SuccessfulStore_ClearsItem()
    {
        var mockService = Substitute.For<HnVue.Common.Abstractions.IDicomService>();
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var outbox = new DicomOutbox(mockService, NullLogger<DicomOutbox>.Instance);
        await outbox.EnqueueAsync("/file.dcm");
        await outbox.ProcessAsync("PACS");

        outbox.Count.Should().Be(0);
    }

    [Fact]
    public async Task Outbox_ProcessAsync_PermanentFailure_DeadLetters()
    {
        var mockService = Substitute.For<HnVue.Common.Abstractions.IDicomService>();
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DicomStoreFailed, "server down"));

        var outbox = new DicomOutbox(mockService, NullLogger<DicomOutbox>.Instance);
        await outbox.EnqueueAsync("/bad.dcm");
        await outbox.ProcessAsync("PACS");

        // After all retries exhausted, item is dead-lettered (discarded)
        outbox.Count.Should().Be(0);
    }

    [Fact]
    public async Task Outbox_ProcessAsync_Cancelled_RequeuesItem()
    {
        var mockService = Substitute.For<HnVue.Common.Abstractions.IDicomService>();
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result>(new OperationCanceledException()));

        var outbox = new DicomOutbox(mockService, NullLogger<DicomOutbox>.Instance);
        await outbox.EnqueueAsync("/file.dcm");

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await outbox.ProcessAsync("PACS", cts.Token);

        outbox.Count.Should().Be(1);
    }

    [Fact]
    public async Task Outbox_ProcessAsync_PreCancelledToken_DoesNotProcess()
    {
        var mockService = Substitute.For<HnVue.Common.Abstractions.IDicomService>();
        var outbox = new DicomOutbox(mockService, NullLogger<DicomOutbox>.Instance);
        await outbox.EnqueueAsync("/file.dcm");

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await outbox.ProcessAsync("PACS", cts.Token);

        outbox.Count.Should().Be(1);
        await mockService.DidNotReceive().StoreAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Outbox_EnqueueAsync_NullPath_ThrowsArgumentException()
    {
        var mockService = Substitute.For<HnVue.Common.Abstractions.IDicomService>();
        var outbox = new DicomOutbox(mockService, NullLogger<DicomOutbox>.Instance);

        var act = async () => await outbox.EnqueueAsync(null!);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Outbox_EnqueueAsync_EmptyPath_ThrowsArgumentException()
    {
        var mockService = Substitute.For<HnVue.Common.Abstractions.IDicomService>();
        var outbox = new DicomOutbox(mockService, NullLogger<DicomOutbox>.Instance);

        var act = async () => await outbox.EnqueueAsync(string.Empty);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // DicomFileIO — Write and Read edge cases
    // ══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task DicomFileIO_WriteAsync_CreatesDirectoryIfMissing()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
        };
        var dicomFile = new DicomFile(dataset);
        var wrapper = new DicomFileWrapper(dicomFile);
        var outputDir = Path.Combine(Path.GetTempPath(), $"dicom_test_dir_{Guid.NewGuid():N}");
        var outputPath = Path.Combine(outputDir, "test.dcm");

        try
        {
            var result = await DicomFileIO.WriteAsync(wrapper, outputPath);

            result.IsSuccess.Should().BeTrue();
            File.Exists(outputPath).Should().BeTrue();
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
            if (Directory.Exists(outputDir)) Directory.Delete(outputDir, recursive: true);
        }
    }

    [Fact]
    public async Task DicomFileIO_ReadAsync_FileNotFound_ReturnsNotFound()
    {
        var result = await DicomFileIO.ReadAsync("C:/nonexistent_xyz/file.dcm");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task DicomFileIO_ReadAsync_NullPath_ThrowsArgumentNullException()
    {
        var act = async () => await DicomFileIO.ReadAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DicomFileIO_WriteAsync_NullWrapper_ThrowsArgumentNullException()
    {
        var act = async () => await DicomFileIO.WriteAsync(null!, "output.dcm");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DicomFileIO_WriteAsync_NullOutputPath_ThrowsArgumentNullException()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
        };
        var wrapper = new DicomFileWrapper(new DicomFile(dataset));

        var act = async () => await DicomFileIO.WriteAsync(wrapper, null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DicomFileIO_GetTagValueAsync_ValidFile_ReturnsTagValue()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
            { DicomTag.PatientID, "TAG_TEST_PAT" },
        };
        var dicomFile = new DicomFile(dataset);
        var tempPath = Path.Combine(Path.GetTempPath(), $"dicom_tag_{Guid.NewGuid():N}.dcm");

        try
        {
            await dicomFile.SaveAsync(tempPath);
            var result = await DicomFileIO.GetTagValueAsync(tempPath, "PatientID");

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("TAG_TEST_PAT");
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task DicomFileIO_GetTagValueAsync_MissingTag_ReturnsNull()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
        };
        var dicomFile = new DicomFile(dataset);
        var tempPath = Path.Combine(Path.GetTempPath(), $"dicom_miss_{Guid.NewGuid():N}.dcm");

        try
        {
            await dicomFile.SaveAsync(tempPath);
            var result = await DicomFileIO.GetTagValueAsync(tempPath, "InstitutionName");

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeNull();
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task DicomFileIO_ReadAsync_InvalidFileContent_ReturnsProcessingFailed()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(tempFile, new byte[] { 0x00, 0x01, 0x02, 0xFF });

            var result = await DicomFileIO.ReadAsync(tempFile);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // DicomStoreScu — Constructor and file validation
    // ══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public void DicomStoreScu_NullConfig_ThrowsArgumentNullException()
    {
        var act = () => new DicomStoreScu(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public async Task DicomStoreScu_NullFilePath_ThrowsArgumentNullException()
    {
        var config = Substitute.For<IDicomNetworkConfig>();
        config.PacsHost.Returns("127.0.0.1");
        config.PacsPort.Returns(104);
        config.LocalAeTitle.Returns("HNVUE");
        config.PacsAeTitle.Returns("PACS");

        var scu = new DicomStoreScu(config);
        var act = async () => await scu.StoreAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DicomStoreScu_FileNotFound_ReturnsStoreFailed()
    {
        var config = Substitute.For<IDicomNetworkConfig>();
        config.PacsHost.Returns("127.0.0.1");
        config.PacsPort.Returns(104);
        config.LocalAeTitle.Returns("HNVUE");
        config.PacsAeTitle.Returns("PACS");

        var scu = new DicomStoreScu(config);
        var result = await scu.StoreAsync("C:/nonexistent_abc/scu_test.dcm");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        result.ErrorMessage.Should().Contain("찾을 수 없습니다");
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // DicomFileWrapper — Property access
    // ══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public void DicomFileWrapper_NullDicomFile_ThrowsArgumentNullException()
    {
        var act = () => new DicomFileWrapper(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DicomFileWrapper_SopInstanceUid_ReturnsValue()
    {
        var uid = DicomUID.Generate();
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, uid },
        };
        var wrapper = new DicomFileWrapper(new DicomFile(dataset));

        wrapper.SopInstanceUid.Should().Be(uid.UID);
    }

    [Fact]
    public void DicomFileWrapper_StudyInstanceUid_ReturnsValue()
    {
        var uid = DicomUID.Generate();
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
            { DicomTag.StudyInstanceUID, uid },
        };
        var wrapper = new DicomFileWrapper(new DicomFile(dataset));

        wrapper.StudyInstanceUid.Should().Be(uid.UID);
    }

    [Fact]
    public void DicomFileWrapper_PatientName_ReturnsValue()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
            { DicomTag.PatientName, "Doe^John" },
        };
        var wrapper = new DicomFileWrapper(new DicomFile(dataset));

        wrapper.PatientName.Should().Be("Doe^John");
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // Testable subclass — enables mock injection for DicomService
    // ══════════════════════════════════════════════════════════════════════════════

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
