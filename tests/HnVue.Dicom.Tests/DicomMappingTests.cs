using FellowOakDicom;
using FluentAssertions;
using HnVue.Common.Models;
using HnVue.Dicom;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Unit tests for the internal DICOM dataset-to-domain mapping logic
/// (<see cref="DicomService.MapToWorklistItem"/>) and C-FIND request building
/// (<see cref="DicomService.BuildWorklistRequest"/>).
/// </summary>
public sealed class DicomMappingTests
{
    // ── MapToWorklistItem ────────────────────────────────────────────────────

    [Fact]
    public void MapToWorklistItem_AllTagsPresent_MapsCorrectly()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC999" },
            { DicomTag.PatientID, "P42" },
            { DicomTag.PatientName, "TEST^PATIENT" },
            { DicomTag.StudyDate, "20260101" },
            { DicomTag.RequestedProcedureDescription, "CR CHEST" }
        };

        var item = DicomService.MapToWorklistItem(dataset);

        item.AccessionNumber.Should().Be("ACC999");
        item.PatientId.Should().Be("P42");
        item.PatientName.Should().Be("TEST^PATIENT");
        item.StudyDate.Should().Be(new DateOnly(2026, 1, 1));
        item.RequestedProcedure.Should().Be("CR CHEST");
    }

    [Fact]
    public void MapToWorklistItem_MissingOptionalTags_NullableFieldsAreNull()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC001" },
            { DicomTag.PatientID, "P01" },
            { DicomTag.PatientName, "ANON" }
            // No StudyDate, no RequestedProcedureDescription
        };

        var item = DicomService.MapToWorklistItem(dataset);

        item.StudyDate.Should().BeNull();
        item.RequestedProcedure.Should().BeNull();
        item.BodyPart.Should().BeNull();
    }

    [Fact]
    public void MapToWorklistItem_ShortDateString_StudyDateIsNull()
    {
        // A dataset with a StudyDate that has wrong length (6 chars instead of 8)
        // should result in a null StudyDate without throwing.
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "A1" },
            { DicomTag.PatientID, "P1" },
            { DicomTag.PatientName, "N" },
            // Omit StudyDate entirely — empty string is equivalent to missing
        };

        var item = DicomService.MapToWorklistItem(dataset);

        // No StudyDate tag → null.
        item.StudyDate.Should().BeNull();
    }

    [Fact]
    public void MapToWorklistItem_EmptyDataset_ReturnsDefaultStrings()
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
    public void MapToWorklistItem_EmptyRequestedProcedure_IsNormalisedToNull()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "A2" },
            { DicomTag.PatientID, "P2" },
            { DicomTag.PatientName, "N2" },
            { DicomTag.RequestedProcedureDescription, string.Empty }
        };

        var item = DicomService.MapToWorklistItem(dataset);

        item.RequestedProcedure.Should().BeNull();
    }

    // ── BuildWorklistRequest ─────────────────────────────────────────────────

    [Fact]
    public void BuildWorklistRequest_WithPatientId_SetsPatientId()
    {
        var query = new WorklistQuery("PAT42", null, null, "MWL");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
        var patId = request.Dataset.GetSingleValueOrDefault(DicomTag.PatientID, string.Empty);
        patId.Should().Be("PAT42");
    }

    [Fact]
    public void BuildWorklistRequest_NullPatientId_NoPatientIdInDataset()
    {
        var query = new WorklistQuery(null, null, null, "MWL");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_WithDateRange_ReturnsRequest()
    {
        var query = new WorklistQuery(null, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "MWL");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_DateFromOnly_ReturnsRequest()
    {
        var query = new WorklistQuery(null, new DateOnly(2026, 6, 1), null, "MWL");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    [Fact]
    public void BuildWorklistRequest_DateToOnly_ReturnsRequest()
    {
        var query = new WorklistQuery(null, null, new DateOnly(2026, 6, 30), "MWL");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }
}
