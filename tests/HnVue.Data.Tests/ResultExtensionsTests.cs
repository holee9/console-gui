using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Data.Tests;

/// <summary>
/// Tests for <see cref="Result.SuccessNullable{T}"/> factory method.
/// </summary>
public sealed class SuccessNullableTests
{
    [Fact]
    public void SuccessNullable_PatientRecord_ReturnsSuccessWithNullValue()
    {
        var result = Result.SuccessNullable<PatientRecord?>(null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void SuccessNullable_StudyRecord_ReturnsSuccessWithNullValue()
    {
        var result = Result.SuccessNullable<StudyRecord?>(null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void SuccessNullable_String_ReturnsSuccessWithNullValue()
    {
        var result = Result.SuccessNullable<string?>(null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void SuccessNullable_NonNullValue_ReturnsSuccessWithValue()
    {
        var dob = new DateOnly(1990, 1, 1);
        var patient = new PatientRecord("P1", "Test", dob, "M", false, DateTimeOffset.UtcNow, "u1");

        var result = Result.SuccessNullable<PatientRecord?>(patient);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(patient);
    }
}
