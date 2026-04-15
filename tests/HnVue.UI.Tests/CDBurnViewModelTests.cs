using FluentAssertions;
using HnVue.Common.Abstractions;
using Xunit;
using HnVue.Common.Results;
using HnVue.UI.ViewModels;
using NSubstitute;

namespace HnVue.UI.Tests;

/// <summary>
/// Tests for <see cref="CDBurnViewModel"/>.
/// </summary>
public sealed class CDBurnViewModelTests
{
    private readonly ICDDVDBurnService _burnService = Substitute.For<ICDDVDBurnService>();

    private CDBurnViewModel CreateSut() => new(_burnService);

    [Fact]
    public void Constructor_SetsDefaultProperties()
    {
        var sut = CreateSut();

        sut.SelectedStudyId.Should().BeNull();
        sut.IsBurning.Should().BeFalse();
        sut.BurnProgress.Should().Be(0);
    }

    [Fact]
    public void StartBurnCommand_CannotExecute_WhenStudyIdIsNull()
    {
        var sut = CreateSut();
        sut.SelectedStudyId = null;

        sut.StartBurnCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void StartBurnCommand_CannotExecute_WhenStudyIdIsEmpty()
    {
        var sut = CreateSut();
        sut.SelectedStudyId = string.Empty;

        sut.StartBurnCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void StartBurnCommand_CanExecute_WhenStudyIdIsSet()
    {
        var sut = CreateSut();
        sut.SelectedStudyId = "1.2.3.4.5";

        sut.StartBurnCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task StartBurnCommand_OnSuccess_SetsBurnProgressTo100()
    {
        _burnService
            .BurnStudyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = CreateSut();
        sut.SelectedStudyId = "1.2.3.4";

        await sut.StartBurnCommand.ExecuteAsync(null);

        sut.BurnProgress.Should().Be(100);
        sut.IsBurning.Should().BeFalse();
    }

    [Fact]
    public async Task StartBurnCommand_OnSuccess_SetsSuccessStatusMessage()
    {
        _burnService
            .BurnStudyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = CreateSut();
        sut.SelectedStudyId = "1.2.3.4";

        await sut.StartBurnCommand.ExecuteAsync(null);

        sut.StatusMessage.Should().Contain("success");
    }

    [Fact]
    public async Task StartBurnCommand_OnFailure_SetsErrorMessage()
    {
        _burnService
            .BurnStudyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.BurnFailed, "No disc inserted."));

        var sut = CreateSut();
        sut.SelectedStudyId = "1.2.3.4";

        await sut.StartBurnCommand.ExecuteAsync(null);

        sut.ErrorMessage.Should().Be("No disc inserted.");
        sut.IsBurning.Should().BeFalse();
    }

    [Fact]
    public async Task StartBurnCommand_CallsServiceWithCorrectStudyId()
    {
        _burnService
            .BurnStudyAsync("1.2.3.4.5", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = CreateSut();
        sut.SelectedStudyId = "1.2.3.4.5";

        await sut.StartBurnCommand.ExecuteAsync(null);

        await _burnService.Received(1)
            .BurnStudyAsync("1.2.3.4.5", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void CancelBurnCommand_CannotExecute_WhenNotBurning()
    {
        var sut = CreateSut();
        sut.CancelBurnCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task StartBurnCommand_SetsIsBurningFalseAfterCompletion()
    {
        _burnService
            .BurnStudyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = CreateSut();
        sut.SelectedStudyId = "1.2.3.4";

        await sut.StartBurnCommand.ExecuteAsync(null);

        sut.IsBurning.Should().BeFalse();
    }

    [Fact]
    public void IViewModelBase_IsLoading_MapsToIsBurning()
    {
        var sut = CreateSut();
        sut.IsBurning = false;
        ((HnVue.UI.Contracts.ViewModels.IViewModelBase)sut).IsLoading.Should().BeFalse();

        sut.IsBurning = true;
        ((HnVue.UI.Contracts.ViewModels.IViewModelBase)sut).IsLoading.Should().BeTrue();
    }

    [Fact]
    public void ICDBurnViewModel_StartBurnCommand_ReturnsStartBurnCommand()
    {
        var sut = CreateSut();
        ((HnVue.UI.Contracts.ViewModels.ICDBurnViewModel)sut).StartBurnCommand
            .Should().BeSameAs(sut.StartBurnCommand);
    }

    [Fact]
    public void ICDBurnViewModel_CancelBurnCommand_ReturnsCancelBurnCommand()
    {
        var sut = CreateSut();
        ((HnVue.UI.Contracts.ViewModels.ICDBurnViewModel)sut).CancelBurnCommand
            .Should().BeSameAs(sut.CancelBurnCommand);
    }

    [Fact]
    public void CancelBurnCommand_CanExecute_WhenBurning()
    {
        var sut = CreateSut();
        // Simulate burning state
        sut.IsBurning = true;

        sut.CancelBurnCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void Dispose_DisposesCancellationTokenSource()
    {
        var sut = CreateSut();
        sut.SelectedStudyId = "1.2.3.4";

        // Start and immediately cancel to create a CancellationTokenSource
        sut.StartBurnCommand.ExecuteAsync(null);
        sut.CancelBurnCommand.Execute(null);

        // Should not throw when disposing
        sut.Dispose();
    }

    [Fact]
    public async Task StartBurnCommand_WithLongStudyId_TruncatesLabel()
    {
        _burnService
            .BurnStudyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = CreateSut();
        sut.SelectedStudyId = "1.2.3.4.5.6.7.8.9"; // 17 characters

        await sut.StartBurnCommand.ExecuteAsync(null);

        // Verify the label was truncated correctly
        // Study ID "1.2.3.4.5.6.7.8.9" (17 chars) is truncated to first 16 chars: "1.2.3.4.5.6.7.8."
        // Label becomes "STUDY_" + "1.2.3.4.5.6.7.8." = 22 characters total
        await _burnService.Received(1).BurnStudyAsync(
            Arg.Any<string>(),
            Arg.Is<string>(label => label == "STUDY_1.2.3.4.5.6.7.8."),
            Arg.Any<CancellationToken>());
    }
}
