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
/// Additional unit tests for <see cref="DicomService"/> covering edge cases in
/// parameter validation, BuildWorklistRequest, MapToWorklistItem, and PrintAsync.
/// REQ-COV-001: Extends coverage from 49.6% towards 80%.
/// </summary>
public sealed class DicomServiceAdditionalTests
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
        });
        return new DicomService(opts, NullLogger<DicomService>.Instance);
    }

    // ── StoreAsync – more validation paths ──────────────────────────────────

    [Fact]
    public async Task StoreAsync_NullAeTitle_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var result = await svc.StoreAsync("some.dcm", null!, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    [Fact]
    public async Task StoreAsync_WhitespaceAeTitle_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var result = await svc.StoreAsync("some.dcm", "   ", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    [Fact]
    public async Task StoreAsync_NonExistentFile_ErrorMessageContainsFilePath()
    {
        var svc = CreateService();
        var result = await svc.StoreAsync(@"C:\nonexistent\path\to\file.dcm", "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Contain("not found");
    }

    // ── QueryWorklistAsync – more parameter paths ────────────────────────────

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
    public async Task QueryWorklistAsync_ErrorContainsAeTitleMessage()
    {
        var svc = CreateService();
        var query = new WorklistQuery(null, null, null, string.Empty);
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Contain("AE title");
    }

    // ── PrintAsync – more paths ──────────────────────────────────────────────

    [Fact]
    public async Task PrintAsync_NullFilePath_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync(null!, "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    [Fact]
    public async Task PrintAsync_WhitespaceFilePath_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync("   ", "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    [Fact]
    public async Task PrintAsync_NullPrinterAeTitle_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync("some.dcm", null!, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    [Fact]
    public async Task PrintAsync_WhitespacePrinterAeTitle_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync("some.dcm", "   ", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    [Fact]
    public async Task PrintAsync_NonExistentFile_ErrorMessageContainsFilePath()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync(@"C:\nonexistent\file.dcm", "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Contain("not found");
    }

    // ── RequestStorageCommitmentAsync ────────────────────────────────────────

    [Fact]
    public async Task RequestStorageCommitmentAsync_NoPacsHost_ReturnsConnectionFailed()
    {
        var options = new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PacsHost = string.Empty,
            PacsPort = 104,
        };
        var svc = CreateService(options);

        var result = await svc.RequestStorageCommitmentAsync("1.2.3", "1.2.3.4.5", "PACS");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        result.ErrorMessage.Should().Contain("PACS host is not configured");
    }

    [Fact]
    public async Task RequestStorageCommitmentAsync_NullSopClassUid_ThrowsArgumentNull()
    {
        var svc = CreateService(new DicomOptions
        {
            PacsHost = "127.0.0.1",
            PacsPort = 104,
        });

        var act = async () => await svc.RequestStorageCommitmentAsync(null!, "1.2.3.4.5", "PACS");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RequestStorageCommitmentAsync_NullSopInstanceUid_ThrowsArgumentNull()
    {
        var svc = CreateService(new DicomOptions
        {
            PacsHost = "127.0.0.1",
            PacsPort = 104,
        });

        var act = async () => await svc.RequestStorageCommitmentAsync("1.2.3", null!, "PACS");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RequestStorageCommitmentAsync_NullAeTitle_ThrowsArgumentNull()
    {
        var svc = CreateService(new DicomOptions
        {
            PacsHost = "127.0.0.1",
            PacsPort = 104,
        });

        var act = async () => await svc.RequestStorageCommitmentAsync("1.2.3", "1.2.3.4.5", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── BuildWorklistRequest (internal static) ────────────────────────────────

    [Fact]
    public void BuildWorklistRequest_NullDates_CreatesRequestWithoutDateRange()
    {
        var query = new WorklistQuery(null, null, null, "MWL_SCP");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_BothDatesSet_CreatesRequestWithDateRange()
    {
        var query = new WorklistQuery(
            null,
            DateOnly.Parse("2026-01-01"),
            DateOnly.Parse("2026-01-31"),
            "MWL_SCP");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_OnlyDateFrom_CreatesValidRequest()
    {
        var query = new WorklistQuery(
            null,
            DateOnly.Parse("2026-01-01"),
            null,
            "MWL_SCP");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_OnlyDateTo_CreatesValidRequest()
    {
        var query = new WorklistQuery(
            null,
            null,
            DateOnly.Parse("2026-01-31"),
            "MWL_SCP");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_WithPatientId_IncludesPatientId()
    {
        var query = new WorklistQuery("PAT001", null, null, "MWL_SCP");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    // ── MapToWorklistItem (internal static) ──────────────────────────────────

    [Fact]
    public void MapToWorklistItem_EmptyDataset_ReturnsWorklistItemWithEmptyStrings()
    {
        var dataset = new DicomDataset();

        var item = DicomService.MapToWorklistItem(dataset);

        item.Should().NotBeNull();
        item.AccessionNumber.Should().BeEmpty();
        item.PatientId.Should().BeEmpty();
        item.PatientName.Should().BeEmpty();
        item.StudyDate.Should().BeNull();
        item.BodyPart.Should().BeNull();
        item.RequestedProcedure.Should().BeNull();
    }

    [Fact]
    public void MapToWorklistItem_WithAllFields_MapsCorrectly()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC001" },
            { DicomTag.PatientID, "PAT001" },
            { DicomTag.PatientName, "Doe^John" },
            { DicomTag.StudyDate, "20260101" },
            { DicomTag.RequestedProcedureDescription, "Chest X-Ray" },
        };

        var item = DicomService.MapToWorklistItem(dataset);

        item.AccessionNumber.Should().Be("ACC001");
        item.PatientId.Should().Be("PAT001");
        item.PatientName.Should().Be("Doe^John");
        item.StudyDate.Should().Be(new DateOnly(2026, 1, 1));
        item.RequestedProcedure.Should().Be("Chest X-Ray");
    }

    [Fact]
    public void MapToWorklistItem_InvalidStudyDate_StudyDateIsNull()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.StudyDate, "INVALID_DATE" },
        };

        var item = DicomService.MapToWorklistItem(dataset);

        item.StudyDate.Should().BeNull();
    }

    [Fact]
    public void MapToWorklistItem_ShortStudyDate_StudyDateIsNull()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.StudyDate, "2026" }, // Only year, not full date
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
    public void MapToWorklistItem_WithSpsSequence_ExtractsBodyPart()
    {
        var spsItem = new DicomDataset
        {
            { DicomTag.ScheduledProtocolCodeSequence, "CHEST" },
        };
        var dataset = new DicomDataset();
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().Be("CHEST");
    }

    [Fact]
    public void MapToWorklistItem_EmptySpsSequence_BodyPartIsNull()
    {
        var dataset = new DicomDataset();
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().BeNull();
    }

    // ── DicomOptions – TLS and MPPS settings ─────────────────────────────────

    [Fact]
    public void DicomOptions_MppsDefaults_AreExpected()
    {
        var opts = new DicomOptions();

        opts.MppsAeTitle.Should().BeEmpty();
        opts.MppsHost.Should().BeEmpty();
        opts.MppsPort.Should().Be(104);
    }

    [Fact]
    public void DicomOptions_MwlDefaults_AreExpected()
    {
        var opts = new DicomOptions();

        opts.MwlAeTitle.Should().BeEmpty();
        opts.MwlHost.Should().BeEmpty();
        opts.MwlPort.Should().Be(104);
    }

    [Fact]
    public void DicomOptions_PrinterDefaults_AreExpected()
    {
        var opts = new DicomOptions();

        opts.PrinterAeTitle.Should().BeEmpty();
        opts.PrinterHost.Should().BeEmpty();
        opts.PrinterPort.Should().Be(104);
    }

    [Fact]
    public void DicomOptions_TlsEnabledByDefault_IsFalse()
    {
        var opts = new DicomOptions();

        opts.TlsEnabled.Should().BeFalse();
    }
}
