using HnVue.Common.Models;

namespace HnVue.Data.Tests;

/// <summary>
/// Tests for <see cref="ResultExtensions"/> internal null-success helper.
/// </summary>
public sealed class ResultExtensionsTests
{
    [Fact]
    public void SuccessWithNull_PatientRecord_ReturnsSuccessWithNullValue()
    {
        var result = ResultExtensions.SuccessWithNull<PatientRecord?>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void SuccessWithNull_StudyRecord_ReturnsSuccessWithNullValue()
    {
        var result = ResultExtensions.SuccessWithNull<StudyRecord?>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void SuccessWithNull_String_ReturnsSuccessWithNullValue()
    {
        var result = ResultExtensions.SuccessWithNull<string?>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }
}
