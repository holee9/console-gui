using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.UI.ViewModels;
using NSubstitute;
using Xunit;

namespace HnVue.UI.Tests;

/// <summary>
/// Unit tests for <see cref="DoseViewModel"/>.
/// </summary>
public sealed class DoseViewModelTests
{
    private static DoseRecord CreateDoseRecord(string studyUid = "1.2.3") =>
        new(
            DoseId: Guid.NewGuid().ToString(),
            StudyInstanceUid: studyUid,
            Dap: 12.5,
            Ei: 300.0,
            EffectiveDose: 0.05,
            BodyPart: "CHEST",
            RecordedAt: DateTimeOffset.UtcNow);

    private static (DoseViewModel Vm, IDoseService Service) CreateSut()
    {
        var service = Substitute.For<IDoseService>();
        var vm = new DoseViewModel(service);
        return (vm, service);
    }

    // ── Constructor guard ────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WhenServiceIsNull_ThrowsArgumentNullException()
    {
        var act = () => new DoseViewModel(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("doseService");
    }

    // ── Initial state ────────────────────────────────────────────────────────

    [Fact]
    public void CurrentDose_InitiallyNull()
    {
        var (vm, _) = CreateSut();
        vm.CurrentDose.Should().BeNull();
    }

    [Fact]
    public void ValidationLevel_InitiallyAllow()
    {
        var (vm, _) = CreateSut();
        vm.ValidationLevel.Should().Be(DoseValidationLevel.Allow);
    }

    [Fact]
    public void IsLoading_InitiallyFalse()
    {
        var (vm, _) = CreateSut();
        vm.IsLoading.Should().BeFalse();
    }

    // ── RefreshCommand: null / empty study ───────────────────────────────────

    [Fact]
    public async Task RefreshCommand_WhenStudyUidIsNull_ClearsDoseAndDoesNotCallService()
    {
        var (vm, service) = CreateSut();
        vm.ActiveStudyInstanceUid = null;

        await vm.RefreshCommand.ExecuteAsync(null);

        vm.CurrentDose.Should().BeNull();
        await service.DidNotReceive().GetDoseByStudyAsync(
            Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshCommand_WhenStudyUidIsEmpty_ClearsDoseAndDoesNotCallService()
    {
        var (vm, service) = CreateSut();
        vm.ActiveStudyInstanceUid = string.Empty;

        await vm.RefreshCommand.ExecuteAsync(null);

        vm.CurrentDose.Should().BeNull();
        await service.DidNotReceive().GetDoseByStudyAsync(
            Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ── RefreshCommand: success paths ────────────────────────────────────────

    [Fact]
    public async Task RefreshCommand_WhenServiceReturnsRecord_SetsCurrentDose()
    {
        var (vm, service) = CreateSut();
        var record = CreateDoseRecord("1.2.3");
        service
            .GetDoseByStudyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<DoseRecord?>(record));

        vm.ActiveStudyInstanceUid = "1.2.3";
        await vm.RefreshCommand.ExecuteAsync(null);

        vm.CurrentDose.Should().Be(record);
    }

    [Fact]
    public async Task RefreshCommand_WhenServiceFails_CurrentDoseIsNullAfterPreviousLoad()
    {
        var (vm, service) = CreateSut();
        // First load succeeds.
        service
            .GetDoseByStudyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.Success<DoseRecord?>(CreateDoseRecord()),
                Result.Failure<DoseRecord?>(ErrorCode.NotFound, "gone"));

        vm.ActiveStudyInstanceUid = "1.2.3";
        await vm.RefreshCommand.ExecuteAsync(null);
        vm.CurrentDose.Should().NotBeNull();

        // Second call fails — CurrentDose should be cleared.
        await vm.RefreshCommand.ExecuteAsync(null);

        vm.CurrentDose.Should().BeNull();
    }

    [Fact]
    public async Task RefreshCommand_AfterSuccess_IsLoadingIsFalse()
    {
        var (vm, service) = CreateSut();
        service
            .GetDoseByStudyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<DoseRecord?>(CreateDoseRecord()));

        vm.ActiveStudyInstanceUid = "1.2.3";
        await vm.RefreshCommand.ExecuteAsync(null);

        vm.IsLoading.Should().BeFalse();
    }

    // ── RefreshCommand: failure path ─────────────────────────────────────────

    [Fact]
    public async Task RefreshCommand_WhenServiceFails_SetsErrorMessage()
    {
        var (vm, service) = CreateSut();
        service
            .GetDoseByStudyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<DoseRecord?>(ErrorCode.DatabaseError, "db error"));

        vm.ActiveStudyInstanceUid = "1.2.3";
        await vm.RefreshCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RefreshCommand_WhenServiceFails_CurrentDoseIsNull()
    {
        var (vm, service) = CreateSut();
        service
            .GetDoseByStudyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<DoseRecord?>(ErrorCode.DatabaseError, "db error"));

        vm.ActiveStudyInstanceUid = "1.2.3";
        await vm.RefreshCommand.ExecuteAsync(null);

        vm.CurrentDose.Should().BeNull();
    }

    [Fact]
    public async Task RefreshCommand_AfterFailure_IsLoadingIsFalse()
    {
        var (vm, service) = CreateSut();
        service
            .GetDoseByStudyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<DoseRecord?>(ErrorCode.DatabaseError, "db error"));

        vm.ActiveStudyInstanceUid = "1.2.3";
        await vm.RefreshCommand.ExecuteAsync(null);

        vm.IsLoading.Should().BeFalse();
    }
}
