using FluentAssertions;
using HnVue.Common.Enums;
using Xunit;

namespace HnVue.Common.Tests.Models;

public sealed class DoseValidationLevelTests
{
    [Fact]
    public void DoseValidationLevel_HasThreeValues()
    {
        Enum.GetValues<DoseValidationLevel>().Should().HaveCount(3);
    }

    [Fact]
    public void DoseValidationLevel_ContainsExpectedMembers()
    {
        Enum.GetNames<DoseValidationLevel>().Should().BeEquivalentTo(
            new[] { "Allow", "Warn", "Block" });
    }

    [Fact]
    public void DoseValidationLevel_Allow_HasLowestOrdinal()
    {
        // Allow should be the first value (ordinal 0) to serve as the default.
        ((int)DoseValidationLevel.Allow).Should().Be(0);
    }
}
