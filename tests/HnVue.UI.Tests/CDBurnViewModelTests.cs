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
}
