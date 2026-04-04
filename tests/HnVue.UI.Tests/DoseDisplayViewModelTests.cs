using FluentAssertions;
using HnVue.Common.Abstractions;
using Xunit;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.UI.ViewModels;
using NSubstitute;

namespace HnVue.UI.Tests;

/// <summary>
/// Tests for <see cref="DoseDisplayViewModel"/>.
/// </summary>
public sealed class DoseDisplayViewModelTests
{
    private readonly IDoseService _doseService = Substitute.For<IDoseService>();

    private DoseDisplayViewModel CreateSut() => new(_doseService);

    private static DoseRecord MakeDoseRecord(double dap = 100.0) => new(
        DoseId: Guid.NewGuid().ToString(),
        StudyInstanceUid: "1.2.3.4",
        Dap: dap,
        Ei: 320,
        EffectiveDose: 0.1,
        BodyPart: "CHEST",
        RecordedAt: DateTimeOffset.UtcNow);

    [Fact]
    public void Constructor_IsDoseAlert_FalseWhenDapIsZero()
    {
        var sut = CreateSut();
        sut.IsDoseAlert.Should().BeFalse();
    }

    [Fact]
    public void IsDoseAlert_TrueWhenCurrentDapExceedsDrl()
    {
        var sut = CreateSut();
        sut.DrlReferenceLevel = 100.0;
        sut.CurrentDoseDap = 150.0;

        sut.IsDoseAlert.Should().BeTrue();
    }

    [Fact]
    public void IsDoseAlert_FalseWhenCurrentDapBelowDrl()
    {
        var sut = CreateSut();
        sut.DrlReferenceLevel = 150.0;
        sut.CurrentDoseDap = 100.0;

        sut.IsDoseAlert.Should().BeFalse();
    }

    [Fact]
    public void IsDoseAlert_FalseWhenCurrentDapEqualsExactlyDrl()
    {
        var sut = CreateSut();
        sut.DrlReferenceLevel = 100.0;
        sut.CurrentDoseDap = 100.0;

        // IsDoseAlert only when strictly greater than.
        sut.IsDoseAlert.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshCommand_PopulatesDoseHistoryOnSuccess()
    {
        var record = MakeDoseRecord(120.0);
        _doseService
            .GetDoseByStudyAsync("1.2.3.4", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord>(record));

        var sut = CreateSut();
        await sut.RefreshCommand.ExecuteAsync("1.2.3.4");

        sut.DoseHistory.Should().ContainSingle();
        sut.CurrentDoseDap.Should().Be(120.0);
    }

    [Fact]
    public async Task RefreshCommand_WithNullStudyUid_DoesNotCallService()
    {
        var sut = CreateSut();
        await sut.RefreshCommand.ExecuteAsync(null);

        await _doseService.DidNotReceive().GetDoseByStudyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshCommand_ServiceFailure_SetsErrorMessage()
    {
        _doseService
            .GetDoseByStudyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<DoseRecord?>(ErrorCode.DatabaseError, "Connection refused"));

        var sut = CreateSut();
        await sut.RefreshCommand.ExecuteAsync("1.2.3.4");

        sut.ErrorMessage.Should().Be("Connection refused");
    }

    [Fact]
    public void ChangingDrlLevel_UpdatesIsDoseAlert()
    {
        var sut = CreateSut();
        sut.CurrentDoseDap = 120.0;
        sut.DrlReferenceLevel = 150.0;
        sut.IsDoseAlert.Should().BeFalse();

        sut.DrlReferenceLevel = 100.0;
        sut.IsDoseAlert.Should().BeTrue();
    }
}
