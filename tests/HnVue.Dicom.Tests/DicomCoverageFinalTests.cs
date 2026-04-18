using System.IO;
using System.Net.Sockets;
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
/// Final coverage gap tests to push HnVue.Dicom from 83.7% to 85%+.
/// Uses TestableDicomService pattern with mocked IDicomClient for deterministic testing.
/// S10-R4 Task 1: Dicom 83.7% to 85%+.
/// </summary>
[Trait("SWR", "SWR-DICOM-020")]
[Trait("SWR", "SWR-DC-055")]
[Trait("SWR", "SWR-DC-056")]
public sealed class DicomCoverageFinalTests
{
    private readonly IDicomClient _mockClient;
    private readonly List<DicomRequest> _capturedRequests = [];

    public DicomCoverageFinalTests()
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
            PacsAeTitle = "TESTPACS",
            MwlHost = "127.0.0.1",
            MwlPort = 104,
            MwlAeTitle = "TESTMWL",
            PrinterHost = "127.0.0.1",
            PrinterPort = 104,
            PrinterAeTitle = "TESTPRINTER",
            MppsHost = "127.0.0.1",
            MppsPort = 104,
            MppsAeTitle = "TESTMPPS",
        });
        return new TestableDicomService(opts, NullLogger<DicomService>.Instance, _mockClient);
    }

    private void SetupSendAsync(Action<DicomRequest> callback)
    {
        _capturedRequests.Clear();
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
            { DicomTag.PatientID, "COV-FINAL" },
            { DicomTag.PatientName, "Final^Test" },
        };
        var dicomFile = new DicomFile(dataset);
        var tempPath = Path.Combine(Path.GetTempPath(), $"dicom_final_{Guid.NewGuid():N}.dcm");
        await dicomFile.SaveAsync(tempPath);
        return tempPath;
    }

    // == StoreAsync uncovered branches ==

    [Fact]
    public async Task StoreAsync_Cancelled_ThrowsOperationCancelled_ReturnsCancelled()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            SetupSendAsyncToThrow(new OperationCanceledException());

            var result = await svc.StoreAsync(tempFile, "TESTPACS", CancellationToken.None);

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
            SetupSendAsyncToThrow(new IOException("Disk read error"));

            var result = await svc.StoreAsync(tempFile, "TESTPACS", CancellationToken.None);

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
    public async Task StoreAsync_DicomNetworkException_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            SetupSendAsyncToThrow(new DicomNetworkException("Connection refused"));

            var result = await svc.StoreAsync(tempFile, "TESTPACS", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
            result.ErrorMessage.Should().Contain("PACS 연결 실패");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // == MapToWorklistItem additional edge case: Invalid study date format ==

    [Fact]
    public void MapToWorklistItem_InvalidDateString_ReturnsNullDate()
    {
        // Use a non-date VR tag to hold the string, then the parsing in
        // MapToWorklistItem will call GetSingleValueOrDefault which returns the raw string.
        // StudyDate VR is DA. For non-parseable date values, fo-dicom stores them as strings.
        // We test the TryParseExact failure path by using an empty StudyDate dataset.
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC-NO-DATE" },
            { DicomTag.PatientID, "PAT-NO-DATE" },
            { DicomTag.PatientName, "NoDate^Test" },
            { DicomTag.StudyDate, string.Empty },
        };

        var item = DicomService.MapToWorklistItem(dataset);

        // Empty string won't parse as yyyyMMdd
        item.StudyDate.Should().BeNull();
        item.AccessionNumber.Should().Be("ACC-NO-DATE");
    }

    // == MapToWorklistItem: Empty body part in SPS, fallback to ScheduledProcedureStepDescription ==

    [Fact]
    public void MapToWorklistItem_SpsWithEmptyBodyPartAndDescription_ReturnsDescription()
    {
        var spsItem = new DicomDataset
        {
            { DicomTag.BodyPartExamined, string.Empty },
            { DicomTag.ScheduledProcedureStepDescription, "Lateral Chest" },
        };

        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC-DESC" },
            { DicomTag.PatientID, "PAT-DESC" },
            { DicomTag.PatientName, "Desc^Test" },
        };
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().Be("Lateral Chest");
    }

    // == MapToWorklistItem: Empty CodeMeaning falls back to CodeValue in ProtocolCodeSequence ==

    [Fact]
    public void MapToWorklistItem_EmptyCodeMeaning_FallsBackToCodeValue()
    {
        var protocolItem = new DicomDataset
        {
            { DicomTag.CodeMeaning, string.Empty },
            { DicomTag.CodeValue, "PROTO-VAL" },
        };
        var protocolSequence = new DicomSequence(DicomTag.ScheduledProtocolCodeSequence, protocolItem);

        var spsItem = new DicomDataset();
        spsItem.Add(protocolSequence);

        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC-CV" },
            { DicomTag.PatientID, "PAT-CV" },
            { DicomTag.PatientName, "CodeVal^Test" },
        };
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().Be("PROTO-VAL");
    }

    // == Test infrastructure ==

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
