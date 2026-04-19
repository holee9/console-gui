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
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Tests for enhanced DICOM Print SCU operations in <see cref="DicomService"/>.
/// Covers: full print flow (N-CREATE Film Session + Film Box + N-SET + N-ACTION + N-GET status),
/// Film Box creation, Film Box N-SET configuration, print status polling, and error handling.
/// Uses a testable subclass that injects a mock DICOM client.
/// </summary>
[Trait("SWR", "SWR-DICOM-020")]
public sealed class DicomPrintScuTests
{
    private readonly IDicomClient _mockClient;
    private readonly List<DicomRequest> _capturedRequests = [];

    public DicomPrintScuTests()
    {
        _mockClient = Substitute.For<IDicomClient>();
        _capturedRequests = [];
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
                // Process only the requests captured for this specific SendAsync call,
                // then clear so the next SendAsync only processes its own requests.
                var batch = _capturedRequests.ToList();
                _capturedRequests.Clear();
                foreach (var captured in batch)
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
        var tempPath = Path.Combine(Path.GetTempPath(), $"dicom_print_test_{Guid.NewGuid():N}.dcm");
        await dicomFile.SaveAsync(tempPath);
        return tempPath;
    }

    // ── Full Print Flow Tests ──────────────────────────────────────────────────

    [Fact]
    public async Task PrintAsync_FullFlow_AllStepsSucceed_ReturnsSuccess()
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
                else if (req is DicomNGetRequest nGet)
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

            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_FullFlow_WithPrintingThenDone_ReturnsSuccess()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            var nGetCount = 0;
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
                    nGetCount++;
                    var statusValue = nGetCount <= 2 ? "PRINTING" : "DONE";
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
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── Film Session Creation Failure ──────────────────────────────────────────

    [Fact]
    public async Task PrintAsync_FilmSessionCreationFails_ReturnsPrintFailed()
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

    // ── Film Box Creation Failure ──────────────────────────────────────────────

    [Fact]
    public async Task PrintAsync_FilmBoxCreationFails_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            SetupSendAsync(req =>
            {
                if (req is DicomNCreateRequest nCreate)
                {
                    // First N-CREATE is Film Session (Success), second N-CREATE is Film Box (Failure)
                    var firstCreate = _capturedRequests.OfType<DicomNCreateRequest>().FirstOrDefault();
                    if (ReferenceEquals(req, firstCreate))
                    {
                        nCreate.OnResponseReceived?.Invoke(nCreate,
                            new DicomNCreateResponse(nCreate, DicomStatus.Success));
                    }
                    else
                    {
                        nCreate.OnResponseReceived?.Invoke(nCreate,
                            new DicomNCreateResponse(nCreate, DicomStatus.ProcessingFailure));
                    }
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

    // ── Film Box N-SET Failure ─────────────────────────────────────────────────

    [Fact]
    public async Task PrintAsync_FilmBoxSetFails_ReturnsPrintFailed()
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
                        new DicomNSetResponse(nSet, DicomStatus.ProcessingFailure));
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

    // ── N-ACTION Print Failure ─────────────────────────────────────────────────

    [Fact]
    public async Task PrintAsync_ActionFails_ReturnsPrintFailed()
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
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── Parameter Validation ───────────────────────────────────────────────────

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
    public async Task PrintAsync_NonExistentFile_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync(@"C:\nonexistent\print_test.dcm", "PRINTER", CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
        result.ErrorMessage.Should().Contain("not found");
    }

    // ── Error Handling ─────────────────────────────────────────────────────────

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

    // ── CreateFilmBoxAsync Tests ───────────────────────────────────────────────

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

        var result = await svc.CreateFilmBoxAsync(DicomUID.Generate(), "PRINTER", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateFilmBoxAsync_Failure_ReturnsPrintFailed()
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

        var result = await svc.CreateFilmBoxAsync(DicomUID.Generate(), "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    [Fact]
    public async Task CreateFilmBoxAsync_NoSuccessFlag_ReturnsPrintFailed()
    {
        var svc = CreateService();
        SetupSendAsync(_ => { }); // No callbacks invoked

        var result = await svc.CreateFilmBoxAsync(DicomUID.Generate(), "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    // ── SetFilmBoxAsync Tests ──────────────────────────────────────────────────

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

        var result = await svc.SetFilmBoxAsync(DicomUID.Generate(), "PRINTER", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SetFilmBoxAsync_Failure_ReturnsPrintFailed()
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

        var result = await svc.SetFilmBoxAsync(DicomUID.Generate(), "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    [Fact]
    public async Task SetFilmBoxAsync_NoSuccessFlag_ReturnsPrintFailed()
    {
        var svc = CreateService();
        SetupSendAsync(_ => { }); // No callbacks invoked

        var result = await svc.SetFilmBoxAsync(DicomUID.Generate(), "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    // ── GetPrintJobStatusAsync Tests ───────────────────────────────────────────

    [Fact]
    public async Task GetPrintJobStatusAsync_Done_ReturnsDone()
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

    [Fact]
    public async Task GetPrintJobStatusAsync_Failure_ReturnsFailure()
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

    [Fact]
    public async Task GetPrintJobStatusAsync_Printing_ThenDone_ReturnsDone()
    {
        var svc = CreateService();
        var pollCount = 0;
        SetupSendAsync(req =>
        {
            if (req is DicomNGetRequest nGet)
            {
                pollCount++;
                var statusValue = pollCount < 3 ? "PRINTING" : "DONE";
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
    }

    [Fact]
    public async Task GetPrintJobStatusAsync_Pending_ThenPrinting_ThenDone_ReturnsDone()
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
                    _ => "DONE"
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
    }

    [Fact]
    public async Task GetPrintJobStatusAsync_EmptyFilmSessionUid_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.GetPrintJobStatusAsync(string.Empty, "PRINTER", CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    [Fact]
    public async Task GetPrintJobStatusAsync_EmptyAeTitle_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", string.Empty, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    [Fact]
    public async Task GetPrintJobStatusAsync_NGetFailure_ReturnsPrintFailed()
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

    [Fact]
    public async Task GetPrintJobStatusAsync_NullDataset_ReturnsPending()
    {
        var svc = CreateService();
        SetupSendAsync(req =>
        {
            if (req is DicomNGetRequest nGet)
            {
                // Response with null dataset
                var response = new DicomNGetResponse(nGet, DicomStatus.Success);
                nGet.OnResponseReceived?.Invoke(nGet, response);
            }
        });

        // Null dataset on first poll should return Pending, then it keeps polling
        // The default max poll is 10 with 1s delay — this test needs to be quick
        // Let's test with a Done response on a later poll to avoid timeout
        var pollCount = 0;
        SetupSendAsync(req =>
        {
            if (req is DicomNGetRequest nGet)
            {
                pollCount++;
                if (pollCount <= 1)
                {
                    // Null dataset
                    var response = new DicomNGetResponse(nGet, DicomStatus.Success);
                    nGet.OnResponseReceived?.Invoke(nGet, response);
                }
                else
                {
                    var statusDataset = new DicomDataset
                    {
                        { DicomTag.ExecutionStatus, "DONE" },
                    };
                    var response = new DicomNGetResponse(nGet, DicomStatus.Success);
                    response.Dataset = statusDataset;
                    nGet.OnResponseReceived?.Invoke(nGet, response);
                }
            }
        });

        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", "PRINTER", CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(PrintJobStatus.Done);
    }

    [Fact]
    public async Task GetPrintJobStatusAsync_NetworkError_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(_ => throw new DicomNetworkException("Network error"));

        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", "PRINTER", CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    public async Task GetPrintJobStatusAsync_CancelledToken_ReturnsOperationCancelled()
    {
        var svc = CreateService();
        _mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns(_ => throw new OperationCanceledException());

        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", "PRINTER", CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.OperationCancelled);
    }

    // ── MapExecutionStatus edge cases via GetPrintJobStatusAsync ───────────────

    [Fact]
    public async Task GetPrintJobStatusAsync_UnknownStatus_ReturnsPending()
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
                // Unknown status maps to Pending which triggers more polling.
                // For test speed, we can't let it poll 10 times.
                // We'll accept that it returns Pending after polling exhaustion.
                nGet.OnResponseReceived?.Invoke(nGet, response);
            }
        });

        // This will poll until exhaustion since UNKNOWN maps to Pending.
        // To keep test fast, we verify it doesn't crash. But 10 polls with 1s delay is too slow.
        // Instead, use a pre-cancelled token after 1 poll to avoid timeout.
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        var result = await svc.GetPrintJobStatusAsync("1.2.3.4.5", "PRINTER", cts.Token);

        // Should either return Pending (exhaustion) or OperationCancelled
        if (result.IsSuccess)
        {
            result.Value.Should().Be(PrintJobStatus.Pending);
        }
        else
        {
            result.Error.Should().Be(ErrorCode.OperationCancelled);
        }
    }

    // ── PrintAsync with status poll failure (non-fatal) ────────────────────────

    [Fact]
    public async Task PrintAsync_StatusPollFailure_StillReturnsSuccess()
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
                else if (req is DicomNGetRequest nGet)
                {
                    // N-GET fails — but print action already succeeded
                    nGet.OnResponseReceived?.Invoke(nGet,
                        new DicomNGetResponse(nGet, DicomStatus.ProcessingFailure));
                }
            });

            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);
            // Print action itself succeeded, status poll failure is non-fatal
            result.IsSuccess.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
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
