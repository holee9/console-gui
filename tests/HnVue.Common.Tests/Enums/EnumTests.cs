using FluentAssertions;
using HnVue.Common.Enums;
using Xunit;

namespace HnVue.Common.Tests.Enums;

public sealed class EnumTests
{
    [Fact]
    public void SafeState_HasFiveValues()
    {
        // SafeState.Warning added for WARN interlock level. Issue #21.
        var values = Enum.GetValues<SafeState>();
        values.Should().HaveCount(5);
    }

    [Fact]
    public void SafeState_ContainsExpectedMembers()
    {
        // Warning added for SWR-WF-023 WARN interlock. Issue #21.
        Enum.GetNames<SafeState>().Should().BeEquivalentTo(
            new[] { "Idle", "Warning", "Degraded", "Blocked", "Emergency" });
    }

    [Fact]
    public void UserRole_HasFourValues()
    {
        Enum.GetValues<UserRole>().Should().HaveCount(4);
    }

    [Fact]
    public void UserRole_ContainsExpectedMembers()
    {
        Enum.GetNames<UserRole>().Should().BeEquivalentTo(
            new[] { "Radiographer", "Radiologist", "Admin", "Service" });
    }

    [Fact]
    public void WorkflowState_HasTenValues()
    {
        Enum.GetValues<WorkflowState>().Should().HaveCount(10);
    }

    [Fact]
    public void WorkflowState_ContainsErrorMember()
    {
        Enum.GetNames<WorkflowState>().Should().Contain("Error");
    }

    [Fact]
    public void WorkflowState_ContainsAllExpectedMembers()
    {
        Enum.GetNames<WorkflowState>().Should().BeEquivalentTo(new[]
        {
            "Idle",
            "PatientSelected",
            "ProtocolLoaded",
            "ReadyToExpose",
            "Exposing",
            "ImageAcquiring",
            "ImageProcessing",
            "ImageReview",
            "Completed",
            "Error",
        });
    }

    [Fact]
    public void GeneratorState_HasSevenValues()
    {
        Enum.GetValues<GeneratorState>().Should().HaveCount(7);
    }

    [Fact]
    public void GeneratorState_ContainsExpectedMembers()
    {
        Enum.GetNames<GeneratorState>().Should().BeEquivalentTo(new[]
        {
            "Disconnected",
            "Idle",
            "Preparing",
            "Ready",
            "Exposing",
            "Done",
            "Error",
        });
    }

    [Fact]
    public void IncidentSeverity_HasFourValues()
    {
        Enum.GetValues<IncidentSeverity>().Should().HaveCount(4);
    }

    [Fact]
    public void IncidentSeverity_ContainsExpectedMembers()
    {
        Enum.GetNames<IncidentSeverity>().Should().BeEquivalentTo(
            new[] { "Critical", "High", "Medium", "Low" });
    }
}
