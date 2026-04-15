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

    [Fact]
    public void IViewModelBase_IsLoading_MapsToIsRefreshing()
    {
        var sut = CreateSut();
        sut.IsRefreshing = false;
        ((HnVue.UI.Contracts.ViewModels.IViewModelBase)sut).IsLoading.Should().BeFalse();

        sut.IsRefreshing = true;
        ((HnVue.UI.Contracts.ViewModels.IViewModelBase)sut).IsLoading.Should().BeTrue();
    }

    [Fact]
    public void IDoseDisplayViewModel_RefreshCommand_ReturnsRefreshCommand()
    {
        var sut = CreateSut();
        ((HnVue.UI.Contracts.ViewModels.IDoseDisplayViewModel)sut).RefreshCommand
            .Should().BeSameAs(sut.RefreshCommand);
    }

    [Fact]
    public void DrlPercentage_ReturnsZeroWhenDrlReferenceLevelIsZero()
    {
        var sut = CreateSut();
        sut.DrlReferenceLevel = 0.0;
        sut.CurrentDoseDap = 100.0;

        sut.DrlPercentage.Should().Be(0.0);
    }

    [Fact]
    public void DrlPercentage_CalculatesCorrectlyForVariousDoses()
    {
        var sut = CreateSut();
        sut.DrlReferenceLevel = 100.0;

        sut.CurrentDoseDap = 50.0;
        sut.DrlPercentage.Should().Be(50.0);

        sut.CurrentDoseDap = 90.0;
        sut.DrlPercentage.Should().Be(90.0);

        sut.CurrentDoseDap = 100.0;
        sut.DrlPercentage.Should().Be(100.0);

        // Should cap at 100% for doses above DRL
        sut.CurrentDoseDap = 150.0;
        sut.DrlPercentage.Should().Be(100.0);
    }

    [Fact]
    public async Task RefreshCommand_WithEmptyStudyUid_DoesNotCallService()
    {
        var sut = CreateSut();
        await sut.RefreshCommand.ExecuteAsync(string.Empty);

        await _doseService.DidNotReceive().GetDoseByStudyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshCommand_WithWhitespaceStudyUid_DoesNotCallService()
    {
        var sut = CreateSut();
        await sut.RefreshCommand.ExecuteAsync("   ");

        await _doseService.DidNotReceive().GetDoseByStudyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshCommand_ClearsExistingHistoryBeforeAdding()
    {
        var record1 = MakeDoseRecord(100.0);
        var record2 = MakeDoseRecord(150.0);

        _doseService
            .GetDoseByStudyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord>(record2));

        var sut = CreateSut();

        // Add first record manually
        sut.DoseHistory.Add(record1);
        sut.DoseHistory.Should().ContainSingle();

        // Refresh should clear and replace
        await sut.RefreshCommand.ExecuteAsync("1.2.3.4");

        sut.DoseHistory.Should().ContainSingle();
        sut.DoseHistory[0].Dap.Should().Be(150.0);
    }

    [Fact]
    public async Task RefreshCommand_WhenServiceReturnsNull_DoesNotUpdateCurrentDose()
    {
        _doseService
            .GetDoseByStudyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord>(null));

        var sut = CreateSut();
        sut.CurrentDoseDap = 50.0; // Set initial value

        await sut.RefreshCommand.ExecuteAsync("1.2.3.4");

        // CurrentDoseDap should remain unchanged when result.Value is null
        sut.CurrentDoseDap.Should().Be(50.0);
    }
}
