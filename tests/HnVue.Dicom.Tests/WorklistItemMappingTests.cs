using FluentAssertions;
using HnVue.Common.Models;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Unit tests for <see cref="WorklistItem"/> record construction
/// and <see cref="WorklistQuery"/> parameter handling.
/// </summary>
public sealed class WorklistItemMappingTests
{
    // ── WorklistItem record ───────────────────────────────────────────────────

    [Fact]
    public void WorklistItem_AllPropertiesSet_CanBeConstructed()
    {
        var item = new WorklistItem(
            AccessionNumber: "ACC001",
            PatientId: "PAT123",
            PatientName: "SMITH^JOHN",
            StudyDate: new DateOnly(2026, 1, 15),
            BodyPart: "CHEST",
            RequestedProcedure: "CR CHEST PA");

        item.AccessionNumber.Should().Be("ACC001");
        item.PatientId.Should().Be("PAT123");
        item.PatientName.Should().Be("SMITH^JOHN");
        item.StudyDate.Should().Be(new DateOnly(2026, 1, 15));
        item.BodyPart.Should().Be("CHEST");
        item.RequestedProcedure.Should().Be("CR CHEST PA");
    }

    [Fact]
    public void WorklistItem_NullableFieldsCanBeNull()
    {
        var item = new WorklistItem(
            AccessionNumber: "ACC002",
            PatientId: "PAT456",
            PatientName: "DOE^JANE",
            StudyDate: null,
            BodyPart: null,
            RequestedProcedure: null);

        item.StudyDate.Should().BeNull();
        item.BodyPart.Should().BeNull();
        item.RequestedProcedure.Should().BeNull();
    }

    [Fact]
    public void WorklistItem_EqualityByValue()
    {
        var item1 = new WorklistItem("A1", "P1", "NAME", new DateOnly(2026, 3, 1), null, null);
        var item2 = new WorklistItem("A1", "P1", "NAME", new DateOnly(2026, 3, 1), null, null);

        item1.Should().Be(item2);
    }

    // ── WorklistQuery ─────────────────────────────────────────────────────────

    [Fact]
    public void WorklistQuery_DateRangeBothProvided_Stored()
    {
        var q = new WorklistQuery("PAT1", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "MWL_SCP");

        q.PatientId.Should().Be("PAT1");
        q.DateFrom.Should().Be(new DateOnly(2026, 1, 1));
        q.DateTo.Should().Be(new DateOnly(2026, 12, 31));
        q.AeTitle.Should().Be("MWL_SCP");
    }

    [Fact]
    public void WorklistQuery_NoDateRange_NullDates()
    {
        var q = new WorklistQuery(null, null, null, "MWL_SCP");

        q.PatientId.Should().BeNull();
        q.DateFrom.Should().BeNull();
        q.DateTo.Should().BeNull();
    }

    [Fact]
    public void WorklistQuery_DateFromOnly_ToIsNull()
    {
        var q = new WorklistQuery(null, new DateOnly(2026, 6, 1), null, "MWL_SCP");

        q.DateFrom.Should().Be(new DateOnly(2026, 6, 1));
        q.DateTo.Should().BeNull();
    }

    [Fact]
    public void WorklistQuery_DateToOnly_FromIsNull()
    {
        var q = new WorklistQuery(null, null, new DateOnly(2026, 6, 30), "MWL_SCP");

        q.DateFrom.Should().BeNull();
        q.DateTo.Should().Be(new DateOnly(2026, 6, 30));
    }
}
