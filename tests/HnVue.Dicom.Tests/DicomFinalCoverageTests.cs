using System.IO;
using FellowOakDicom;
using FluentAssertions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dicom;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Additional Dicom coverage tests targeting uncovered branches:
/// - DicomFileIO WriteAsync with existing directory
/// - MapToWorklistItem edge cases
/// - BuildWorklistRequest with PatientId filter
/// - DicomFileWrapper with missing tags
/// </summary>
[Trait("SWR", "SWR-DC-060")]
public sealed class DicomAdditionalCoverageTests
{
    private static async Task<string> CreateTempDicomFileAsync(
        string patientId = "TEST-PAT",
        string patientName = "Test^Patient",
        string studyDate = "20260414")
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
            { DicomTag.PatientID, patientId },
            { DicomTag.PatientName, patientName },
            { DicomTag.StudyDate, studyDate },
        };
        var dicomFile = new DicomFile(dataset);
        var tempPath = Path.Combine(Path.GetTempPath(), $"dicom_addl_{Guid.NewGuid():N}.dcm");
        await dicomFile.SaveAsync(tempPath);
        return tempPath;
    }

    // ── DicomFileIO WriteAsync with existing directory ──────────────────────────

    [Fact]
    public async Task WriteAsync_ExistingDirectory_SavesSuccessfully()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
        };
        var dcmFile = new DicomFile(dataset);
        var wrapper = new DicomFileWrapper(dcmFile);

        var tempDir = Path.GetTempPath();
        var outputPath = Path.Combine(tempDir, $"dicom_existing_dir_{Guid.NewGuid():N}.dcm");
        try
        {
            var result = await DicomFileIO.WriteAsync(wrapper, outputPath, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            File.Exists(outputPath).Should().BeTrue();
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    // ── DicomFileIO ReadAsync success ───────────────────────────────────────────

    [Fact]
    public async Task ReadAsync_ValidFile_ReturnsWrapper()
    {
        var tempFile = await CreateTempDicomFileAsync();
        try
        {
            var result = await DicomFileIO.ReadAsync(tempFile, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.SopInstanceUid.Should().NotBeNullOrEmpty();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadAsync_NonExistentFile_ReturnsNotFound()
    {
        var result = await DicomFileIO.ReadAsync("C:/nonexistent_read_abc/file.dcm", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── MapToWorklistItem edge cases ────────────────────────────────────────────

    [Fact]
    public void MapToWorklistItem_NoScheduledProcedureStep_ReturnsNullBodyPart()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC-NO-SPS" },
            { DicomTag.PatientID, "PAT-NO-SPS" },
            { DicomTag.PatientName, "NoSPS^Test" },
            { DicomTag.StudyDate, "20260414" },
        };

        // Access internal method via InternalsVisibleTo
        var worklistItem = DicomService.MapToWorklistItem(dataset);

        worklistItem.AccessionNumber.Should().Be("ACC-NO-SPS");
        worklistItem.PatientId.Should().Be("PAT-NO-SPS");
        worklistItem.BodyPart.Should().BeNull();
        worklistItem.StudyDate.Should().Be(new DateOnly(2026, 4, 14));
    }

    [Fact]
    public void MapToWorklistItem_NoStudyDateTag_ReturnsNullStudyDate()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC-NO-DATE" },
            { DicomTag.PatientID, "PAT-NO-DATE" },
            { DicomTag.PatientName, "NoDate^Test" },
        };

        var worklistItem = DicomService.MapToWorklistItem(dataset);

        worklistItem.StudyDate.Should().BeNull();
    }

    [Fact]
    public void MapToWorklistItem_EmptyStudyDate_ReturnsNullStudyDate()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC-EMPTY-DATE" },
            { DicomTag.PatientID, "PAT-EMPTY-DATE" },
            { DicomTag.PatientName, "EmptyDate^Test" },
            { DicomTag.StudyDate, string.Empty },
        };

        var worklistItem = DicomService.MapToWorklistItem(dataset);

        worklistItem.StudyDate.Should().BeNull();
    }

    [Fact]
    public void MapToWorklistItem_SpsWithEmptyStrings_NoBodyPart()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC-EMPTY" },
            { DicomTag.PatientID, "PAT-EMPTY" },
            { DicomTag.PatientName, "Empty^Test" },
            { DicomTag.StudyDate, "20260414" },
        };
        var spsSequence = new DicomSequence(DicomTag.ScheduledProcedureStepSequence);
        var spsItem = new DicomDataset
        {
            { DicomTag.BodyPartExamined, string.Empty },
            { DicomTag.ScheduledProcedureStepDescription, "   " },
        };
        spsSequence.Items.Add(spsItem);
        dataset.Add(spsSequence);

        var worklistItem = DicomService.MapToWorklistItem(dataset);

        worklistItem.BodyPart.Should().BeNull();
    }

    [Fact]
    public void MapToWorklistItem_WithRequestedProcedureDescription_SetsRequestedProcedure()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC-REQ-PROC" },
            { DicomTag.PatientID, "PAT-REQ-PROC" },
            { DicomTag.PatientName, "ReqProc^Test" },
            { DicomTag.StudyDate, "20260414" },
            { DicomTag.RequestedProcedureDescription, "Chest PA and Lateral" },
        };

        var worklistItem = DicomService.MapToWorklistItem(dataset);

        worklistItem.RequestedProcedure.Should().Be("Chest PA and Lateral");
    }

    // ── BuildWorklistRequest ────────────────────────────────────────────────────

    [Fact]
    public void BuildWorklistRequest_WithPatientId_ReturnsValidRequest()
    {
        var query = new WorklistQuery(
            AeTitle: "TESTMWL",
            DateFrom: null,
            DateTo: null,
            PatientId: "PAT12345");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_WithBothDates_ReturnsValidRequest()
    {
        var query = new WorklistQuery(
            AeTitle: "TESTMWL",
            DateFrom: new DateOnly(2026, 1, 1),
            DateTo: new DateOnly(2026, 12, 31),
            PatientId: null);

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    // ── DicomFileWrapper property coverage ──────────────────────────────────────

    [Fact]
    public void DicomFileWrapper_MissingTags_ReturnsDefaults()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
            // No PatientName, no StudyInstanceUID
        };
        var dcmFile = new DicomFile(dataset);
        var wrapper = new DicomFileWrapper(dcmFile);

        wrapper.SopInstanceUid.Should().NotBeNullOrEmpty();
        wrapper.StudyInstanceUid.Should().BeNull();
        wrapper.PatientName.Should().BeNull();
    }

    [Fact]
    public void DicomFileWrapper_AllTags_ReturnsValues()
    {
        var sopUid = DicomUID.Generate();
        var studyUid = DicomUID.Generate();
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, sopUid },
            { DicomTag.StudyInstanceUID, studyUid },
            { DicomTag.PatientName, "Doe^John" },
        };
        var dcmFile = new DicomFile(dataset);
        var wrapper = new DicomFileWrapper(dcmFile);

        wrapper.SopInstanceUid.Should().Be(sopUid.UID);
        wrapper.StudyInstanceUid.Should().Be(studyUid.UID);
        wrapper.PatientName.Should().Be("Doe^John");
    }

    // ── DicomOutbox basic coverage ──────────────────────────────────────────────

    [Fact]
    public async Task DicomOutbox_CreateInstance_HasZeroCount()
    {
        var dicomService = NSubstitute.Substitute.For<HnVue.Common.Abstractions.IDicomService>();
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<DicomOutbox>.Instance;
        var outbox = new DicomOutbox(dicomService, logger);

        outbox.Count.Should().Be(0);
    }

    [Fact]
    public async Task DicomOutbox_Enqueue_IncrementsCount()
    {
        var dicomService = NSubstitute.Substitute.For<HnVue.Common.Abstractions.IDicomService>();
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<DicomOutbox>.Instance;
        var outbox = new DicomOutbox(dicomService, logger);

        await outbox.EnqueueAsync("/tmp/test.dcm");

        outbox.Count.Should().Be(1);
    }

    [Fact]
    public async Task DicomOutbox_Enqueue_EmptyPath_Throws()
    {
        var dicomService = NSubstitute.Substitute.For<HnVue.Common.Abstractions.IDicomService>();
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<DicomOutbox>.Instance;
        var outbox = new DicomOutbox(dicomService, logger);

        var act = async () => await outbox.EnqueueAsync(string.Empty);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DicomOutbox_Enqueue_NullPath_Throws()
    {
        var dicomService = NSubstitute.Substitute.For<HnVue.Common.Abstractions.IDicomService>();
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<DicomOutbox>.Instance;
        var outbox = new DicomOutbox(dicomService, logger);

        var act = async () => await outbox.EnqueueAsync(null!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ── DicomOptions defaults ───────────────────────────────────────────────────

    [Fact]
    public void DicomOptions_DefaultValues_AreCorrect()
    {
        var options = new DicomOptions();

        options.LocalAeTitle.Should().Be("HNVUE");
        options.PacsPort.Should().Be(104);
        options.MwlPort.Should().Be(104);
        options.PrinterPort.Should().Be(104);
        options.MppsPort.Should().Be(104);
        options.TlsEnabled.Should().BeFalse();
        options.PacsHost.Should().BeEmpty();
        options.PacsAeTitle.Should().BeEmpty();
        options.MwlHost.Should().BeEmpty();
        options.PrinterHost.Should().BeEmpty();
        options.MppsHost.Should().BeEmpty();
    }
}
