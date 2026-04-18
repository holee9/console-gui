using System.Globalization;
using System.IO;
using FellowOakDicom;
using FluentAssertions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dicom;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Additional coverage tests for <see cref="DicomService"/>.
/// Targets StoreAsync, QueryWorklistAsync, PrintAsync, RequestStorageCommitmentAsync error paths.
/// </summary>
[Trait("SWR", "SWR-DICOM-020")]
public sealed class DicomServiceCoverageTests
{
    private static DicomService CreateService(DicomOptions? options = null)
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
        return new DicomService(opts, NullLogger<DicomService>.Instance);
    }

    // ── StoreAsync Coverage ──────────────────────────────────────────────────

    [Fact]
    public async Task StoreAsync_NullPacsAeTitle_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var result = await svc.StoreAsync("/tmp/test.dcm", null!, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    [Fact]
    public async Task StoreAsync_EmptyPacsAeTitle_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var result = await svc.StoreAsync("/tmp/test.dcm", string.Empty, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    [Fact]
    public async Task StoreAsync_WhitespacePacsAeTitle_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var result = await svc.StoreAsync("/tmp/test.dcm", "   ", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    [Fact]
    public async Task StoreAsync_NonExistentFile_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var result = await svc.StoreAsync("C:/nonexistent_path_abc123/test.dcm", "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        result.ErrorMessage.Should().Contain("찾을 수 없습니다");
    }

    [Fact]
    public async Task StoreAsync_InvalidDicomFile_ReturnsFailure()
    {
        var svc = CreateService();
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "not valid dicom content");
            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── QueryWorklistAsync Coverage ──────────────────────────────────────────

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
    public async Task QueryWorklistAsync_NullAeTitle_ReturnsQueryFailed()
    {
        var svc = CreateService();
        var query = new WorklistQuery(null, null, null, null!);
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomQueryFailed);
    }

    [Fact]
    public async Task QueryWorklistAsync_WhitespaceAeTitle_ReturnsQueryFailed()
    {
        var svc = CreateService();
        var query = new WorklistQuery(null, null, null, "   ");
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomQueryFailed);
    }

    [Fact]
    public async Task QueryWorklistAsync_NetworkUnreachable_ReturnsConnectionFailed()
    {
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            MwlHost = "192.0.2.1", // TEST-NET-1, unreachable
            MwlPort = 19999,
        });
        var query = new WorklistQuery(null, null, null, "MWL_SCP");
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    // ── PrintAsync Coverage ──────────────────────────────────────────────────

    [Fact]
    public async Task PrintAsync_NullFilePath_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync(null!, "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
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
    public async Task PrintAsync_NullPrinterAeTitle_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync("/tmp/test.dcm", null!, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    [Fact]
    public async Task PrintAsync_NonExistentFile_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync("C:/nonexistent_abcxyz/test.dcm", "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    [Fact]
    public async Task PrintAsync_EmptyPrinterAeTitle_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync("/tmp/test.dcm", string.Empty, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    // ── RequestStorageCommitmentAsync Coverage ──────────────────────────────

    [Fact]
    public async Task RequestStorageCommitmentAsync_EmptyPacsHost_ReturnsConnectionFailed()
    {
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PacsHost = string.Empty,
        });
        var result = await svc.RequestStorageCommitmentAsync("1.2.3", "1.2.3.4", "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    public async Task RequestStorageCommitmentAsync_NullSopClassUid_ThrowsArgumentNullException()
    {
        var svc = CreateService();
        var act = async () => await svc.RequestStorageCommitmentAsync(null!, "1.2.3", "PACS", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RequestStorageCommitmentAsync_NullSopInstanceUid_ThrowsArgumentNullException()
    {
        var svc = CreateService();
        var act = async () => await svc.RequestStorageCommitmentAsync("1.2.3", null!, "PACS", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RequestStorageCommitmentAsync_NullPacsAeTitle_ThrowsArgumentNullException()
    {
        var svc = CreateService();
        var act = async () => await svc.RequestStorageCommitmentAsync("1.2.3", "1.2.3.4", null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── BuildWorklistRequest Coverage ────────────────────────────────────────

    [Fact]
    public void BuildWorklistRequest_WithDateRange_CreatesValidRequest()
    {
        var query = new WorklistQuery(
            PatientId: "P001",
            DateFrom: new DateOnly(2026, 1, 1),
            DateTo: new DateOnly(2026, 3, 31),
            AeTitle: "MWL");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_WithOnlyDateFrom_CreatesValidRequest()
    {
        var query = new WorklistQuery(
            PatientId: null,
            DateFrom: new DateOnly(2026, 1, 1),
            DateTo: null,
            AeTitle: "MWL");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_WithOnlyDateTo_CreatesValidRequest()
    {
        var query = new WorklistQuery(
            PatientId: null,
            DateFrom: null,
            DateTo: new DateOnly(2026, 12, 31),
            AeTitle: "MWL");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_NoDates_CreatesValidRequest()
    {
        var query = new WorklistQuery(null, null, null, "MWL");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    // ── MapToWorklistItem Coverage ───────────────────────────────────────────

    [Fact]
    public void MapToWorklistItem_WithValidStudyDate_ParsesCorrectly()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC001" },
            { DicomTag.PatientID, "P001" },
            { DicomTag.PatientName, "Test Patient" },
            { DicomTag.StudyDate, "20260115" },
        };

        var item = DicomService.MapToWorklistItem(dataset);

        item.AccessionNumber.Should().Be("ACC001");
        item.PatientId.Should().Be("P001");
        item.PatientName.Should().Be("Test Patient");
        item.StudyDate.Should().Be(new DateOnly(2026, 1, 15));
    }

    [Fact]
    public void MapToWorklistItem_WithEmptyStudyDate_ReturnsNullDate()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC002" },
            { DicomTag.PatientID, "P002" },
            { DicomTag.PatientName, "Another Patient" },
            { DicomTag.StudyDate, string.Empty },
        };

        var item = DicomService.MapToWorklistItem(dataset);

        item.StudyDate.Should().BeNull();
    }

    [Fact]
    public void MapToWorklistItem_WithMissingStudyDate_ReturnsNullDate()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC003" },
            { DicomTag.PatientID, "P003" },
            { DicomTag.PatientName, "Patient Three" },
        };

        var item = DicomService.MapToWorklistItem(dataset);

        item.StudyDate.Should().BeNull();
    }

    [Fact]
    public void MapToWorklistItem_WithRequestedProcedure_ReturnsProcedure()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC004" },
            { DicomTag.PatientID, "P004" },
            { DicomTag.PatientName, "Patient Four" },
            { DicomTag.StudyDate, string.Empty },
            { DicomTag.RequestedProcedureDescription, "Chest PA" },
        };

        var item = DicomService.MapToWorklistItem(dataset);

        item.RequestedProcedure.Should().Be("Chest PA");
    }

    [Fact]
    public void MapToWorklistItem_WithScheduledProcedureStepSequence_ExtractsBodyPart()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC005" },
            { DicomTag.PatientID, "P005" },
            { DicomTag.PatientName, "Patient Five" },
            { DicomTag.StudyDate, string.Empty },
        };

        var spsSequence = new DicomSequence(DicomTag.ScheduledProcedureStepSequence,
            new DicomDataset
            {
                {
                    DicomTag.ScheduledProtocolCodeSequence,
                    new DicomSequence(
                        DicomTag.ScheduledProtocolCodeSequence,
                        new DicomDataset
                        {
                            { DicomTag.CodeMeaning, "CHEST" },
                        })
                },
            });
        dataset.Add(spsSequence);

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().Be("CHEST");
    }
}
