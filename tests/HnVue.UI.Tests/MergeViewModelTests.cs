using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.UI.ViewModels;
using NSubstitute;
using Xunit;

namespace HnVue.UI.Tests;

/// <summary>
/// Unit tests for <see cref="MergeViewModel"/>.
/// SWR-UI-MERGE-001 through SWR-UI-MERGE-010.
/// </summary>
public sealed class MergeViewModelTests
{
    private static (MergeViewModel Vm, IPatientService PatientService) CreateSut()
    {
        var patientService = Substitute.For<IPatientService>();
        var vm = new MergeViewModel(patientService);
        return (vm, patientService);
    }

    // ── Constructor tests ────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-MERGE-001")]
    public void Constructor_InitializesWithEmptySearchQueries()
    {
        var (vm, _) = CreateSut();

        vm.SearchQueryA.Should().BeEmpty();
        vm.SearchQueryB.Should().BeEmpty();
    }

    [Fact]
    [Trait("SWR", "SWR-UI-MERGE-001")]
    public void Constructor_InitializesWithEmptyPatientCollections()
    {
        var (vm, _) = CreateSut();

        vm.PatientsA.Should().BeEmpty();
        vm.PatientsB.Should().BeEmpty();
    }

    [Fact]
    [Trait("SWR", "SWR-UI-MERGE-001")]
    public void Constructor_NullSelectedPatientsByDefault()
    {
        var (vm, _) = CreateSut();

        vm.SelectedPatientA.Should().BeNull();
        vm.SelectedPatientB.Should().BeNull();
    }

    [Fact]
    [Trait("SWR", "SWR-UI-MERGE-001")]
    public void Constructor_IsLoadingFalseByDefault()
    {
        var (vm, _) = CreateSut();

        vm.IsLoading.Should().BeFalse();
        vm.ErrorMessage.Should().BeNull();
    }

    // ── Property change notification tests ──────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-MERGE-002")]
    public void SearchQueryA_WhenSet_RaisesPropertyChanged()
    {
        var (vm, _) = CreateSut();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.SearchQueryA = "Kim";

        raised.Should().Contain(nameof(vm.SearchQueryA));
    }

    [Fact]
    [Trait("SWR", "SWR-UI-MERGE-002")]
    public void SearchQueryB_WhenSet_RaisesPropertyChanged()
    {
        var (vm, _) = CreateSut();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.SearchQueryB = "Park";

        raised.Should().Contain(nameof(vm.SearchQueryB));
    }

    [Fact]
    [Trait("SWR", "SWR-UI-MERGE-002")]
    public void SelectedPatientA_WhenSet_RaisesPropertyChanged()
    {
        var (vm, _) = CreateSut();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.SelectedPatientA = MakePatient("P-001");

        raised.Should().Contain(nameof(vm.SelectedPatientA));
    }

    // ── SearchA command tests ────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-MERGE-003")]
    public async Task SearchACommand_WhenServiceSucceeds_PopulatesPatientsA()
    {
        var (vm, patientService) = CreateSut();
        var patients = new List<PatientRecord> { MakePatient("P-001"), MakePatient("P-002") };
        vm.SearchQueryA = "Kim";

        patientService.SearchAsync("Kim", Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<PatientRecord>>(patients));

        vm.SearchACommand.Execute(null);
        await Task.Delay(200);

        vm.PatientsA.Should().HaveCount(2);
    }

    [Fact]
    [Trait("SWR", "SWR-UI-MERGE-003")]
    public async Task SearchACommand_WhenServiceFails_SetsErrorMessage()
    {
        var (vm, patientService) = CreateSut();
        vm.SearchQueryA = "Kim";

        patientService.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<PatientRecord>>(ErrorCode.DatabaseError, "DB error"));

        vm.SearchACommand.Execute(null);
        await Task.Delay(200);

        vm.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("SWR", "SWR-UI-MERGE-003")]
    public async Task SearchACommand_SetsIsLoadingDuringExecution()
    {
        var (vm, patientService) = CreateSut();
        vm.SearchQueryA = "Kim";

        var loadingDuringExecution = false;
        patientService.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                loadingDuringExecution = vm.IsLoading;
                await Task.Delay(10);
                return Result.Success<IReadOnlyList<PatientRecord>>(new List<PatientRecord>());
            });

        vm.SearchACommand.Execute(null);
        await Task.Delay(200);

        loadingDuringExecution.Should().BeTrue("IsLoading should be true during search execution");
        vm.IsLoading.Should().BeFalse("IsLoading should be false after search completes");
    }

    // ── SearchB command tests ────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-MERGE-004")]
    public async Task SearchBCommand_WhenServiceSucceeds_PopulatesPatientsB()
    {
        var (vm, patientService) = CreateSut();
        var patients = new List<PatientRecord> { MakePatient("P-010") };
        vm.SearchQueryB = "Park";

        patientService.SearchAsync("Park", Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<PatientRecord>>(patients));

        vm.SearchBCommand.Execute(null);
        await Task.Delay(200);

        vm.PatientsB.Should().HaveCount(1);
    }

    // ── Merge command tests ──────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-MERGE-005")]
    public async Task MergeCommand_WhenBothPatientsSelected_RaisesMergeCompleted()
    {
        var (vm, _) = CreateSut();
        vm.SelectedPatientA = MakePatient("P-001");
        vm.SelectedPatientB = MakePatient("P-002");

        var mergeCompleted = false;
        vm.MergeCompleted += (_, _) => mergeCompleted = true;

        vm.MergeCommand.Execute(null);
        await Task.Delay(200);

        mergeCompleted.Should().BeTrue();
    }

    [Fact]
    [Trait("SWR", "SWR-UI-MERGE-005")]
    public async Task MergeCommand_WhenPatientAIsNull_SetsErrorMessage()
    {
        var (vm, _) = CreateSut();
        vm.SelectedPatientA = null;
        vm.SelectedPatientB = MakePatient("P-002");

        vm.MergeCommand.Execute(null);
        await Task.Delay(100);

        vm.ErrorMessage.Should().NotBeNullOrEmpty("should require both patients to be selected");
    }

    [Fact]
    [Trait("SWR", "SWR-UI-MERGE-005")]
    public async Task MergeCommand_WhenPatientBIsNull_SetsErrorMessage()
    {
        var (vm, _) = CreateSut();
        vm.SelectedPatientA = MakePatient("P-001");
        vm.SelectedPatientB = null;

        vm.MergeCommand.Execute(null);
        await Task.Delay(100);

        vm.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    // ── Cancel command tests ─────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-MERGE-006")]
    public void CancelCommand_RaisesCancelledEvent()
    {
        var (vm, _) = CreateSut();
        var cancelled = false;
        vm.Cancelled += (_, _) => cancelled = true;

        vm.CancelCommand.Execute(null);

        cancelled.Should().BeTrue();
    }

    // ── IViewModelBase explicit interface tests ──────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-MERGE-007")]
    public void IViewModelBase_IsLoading_ReflectsInstanceProperty()
    {
        var (vm, _) = CreateSut();
        var asBase = (HnVue.UI.Contracts.ViewModels.IViewModelBase)vm;

        asBase.IsLoading.Should().BeFalse();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static PatientRecord MakePatient(string patientId) => new(
        PatientId: patientId,
        Name: $"Patient {patientId}",
        DateOfBirth: new DateOnly(1990, 1, 1),
        Sex: "M",
        IsEmergency: false,
        CreatedAt: DateTimeOffset.UtcNow,
        CreatedBy: "test");
}
