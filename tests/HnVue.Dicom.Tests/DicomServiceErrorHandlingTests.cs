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
/// Tests improved error handling in DicomService including:
/// - User-friendly Korean error messages
/// - Retry logic for transient errors
/// - Clear association failure messages
/// SWR-DICOM-021.
/// </summary>
[Trait("SWR", "SWR-DICOM-021")]
public sealed class DicomServiceErrorHandlingTests
{
    private readonly IDicomClient _mockClient;
    private readonly List<DicomRequest> _capturedRequests = [];

    public DicomServiceErrorHandlingTests()
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
            StoreRetryCount = 2, // Enable retry for testing
            StoreRetryDelayMs = 10, // Short delay for testing
        });
        return new TestableDicomService(opts, NullLogger<DicomService>.Instance, _mockClient);
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
        var tempPath = Path.Combine(Path.GetTempPath(), $"dicom_error_test_{Guid.NewGuid():N}.dcm");
        await dicomFile.SaveAsync(tempPath);
        return tempPath;
    }

    // ── User-Friendly Error Messages ───────────────────────────────────────────────

    [Fact]
    public async Task StoreAsync_FailureResponse_ReturnsKoreanErrorMessage()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            _mockClient.SendAsync(Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    foreach (var captured in _capturedRequests)
                    {
                        if (captured is DicomCStoreRequest cStore)
                        {
                            cStore.OnResponseReceived?.Invoke(cStore,
                                new DicomCStoreResponse(cStore, DicomStatus.ProcessingFailure));
                        }
                    }
                    return Task.CompletedTask;
                });

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
            result.ErrorMessage.Should().Contain("실패"); // "실패" (failure) in Korean
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_NetworkError_ReturnsKoreanErrorMessage()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            _mockClient.SendAsync(Arg.Any<CancellationToken>())
                .Returns(_ => throw new FellowOakDicom.Network.DicomNetworkException("Connection refused"));

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
            result.ErrorMessage.Should().Contain("연결 실패"); // "연결 실패" (connection failed) in Korean
            result.ErrorMessage.Should().Contain("PACS");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_SocketError_ReturnsKoreanErrorMessage()
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
            result.ErrorMessage.Should().Contain("서버에 연결할 수 없습니다"); // "Cannot connect to server" in Korean
            result.ErrorMessage.Should().Contain("PACS");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_FileNotFoundError_ReturnsKoreanErrorMessage()
    {
        var svc = CreateService();
        var result = await svc.StoreAsync(@"C:\nonexistent\file.dcm", "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        result.ErrorMessage.Should().Contain("찾을 수 없습니다"); // "Cannot find" in Korean
    }

    [Fact]
    public async Task StoreAsync_EmptyFilePath_ReturnsKoreanErrorMessage()
    {
        var svc = CreateService();
        var result = await svc.StoreAsync(string.Empty, "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        result.ErrorMessage.Should().Contain("비어있습니다"); // "Is empty" in Korean
    }

    // ─── Retry Logic ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task StoreAsync_TransientNetworkError_RetriesAndSucceeds()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            var attemptCount = 0;
            _mockClient.SendAsync(Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    attemptCount++;
                    if (attemptCount == 1)
                    {
                        // First attempt: transient error
                        throw new FellowOakDicom.Network.DicomNetworkException("timeout");
                    }
                    else
                    {
                        // Second attempt: success
                        foreach (var captured in _capturedRequests)
                        {
                            if (captured is DicomCStoreRequest cStore)
                            {
                                cStore.OnResponseReceived?.Invoke(cStore,
                                    new DicomCStoreResponse(cStore, DicomStatus.Success));
                            }
                        }
                        return Task.CompletedTask;
                    }
                });

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            attemptCount.Should().BeGreaterThan(1, "Should have retried at least once");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_TransientSocketError_RetriesAndSucceeds()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            var attemptCount = 0;
            _mockClient.SendAsync(Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    attemptCount++;
                    if (attemptCount == 1)
                    {
                        // First attempt: socket error
                        throw new System.Net.Sockets.SocketException(10060); // Timeout
                    }
                    else
                    {
                        // Second attempt: success
                        foreach (var captured in _capturedRequests)
                        {
                            if (captured is DicomCStoreRequest cStore)
                            {
                                cStore.OnResponseReceived?.Invoke(cStore,
                                    new DicomCStoreResponse(cStore, DicomStatus.Success));
                            }
                        }
                        return Task.CompletedTask;
                    }
                });

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            attemptCount.Should().BeGreaterThan(1, "Should have retried at least once");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_PersistentError_AllRetriesExhausted()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            var attemptCount = 0;
            _mockClient.SendAsync(Arg.Any<CancellationToken>())
                .Returns(_ =>
                {
                    attemptCount++;
                    throw new FellowOakDicom.Network.DicomNetworkException("Persistent error");
                });

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            attemptCount.Should().Be(3, "Should have attempted initial + 2 retries = 3 total");
            result.ErrorMessage.Should().Contain("재시도 후에도 성공하지 못했습니다"); // "Failed after retries" in Korean
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_NoRetryConfigured_FailsImmediately()
    {
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PacsHost = "127.0.0.1",
            PacsPort = 104,
            StoreRetryCount = 0, // No retry
            StoreRetryDelayMs = 10,
        });
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            var attemptCount = 0;
            _mockClient.SendAsync(Arg.Any<CancellationToken>())
                .Returns(_ =>
                {
                    attemptCount++;
                    throw new FellowOakDicom.Network.DicomNetworkException("Error");
                });

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            attemptCount.Should().Be(1, "Should have attempted only once (no retry)");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ─── Testable subclass ───────────────────────────────────────────────────────────

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
