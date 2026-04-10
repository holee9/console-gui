using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.UI.ViewModels;
using NSubstitute;
using Xunit;

namespace HnVue.UI.Tests;

/// <summary>
/// Unit tests for <see cref="StudylistViewModel"/>.
/// SWR-UI-SL-001 through SWR-UI-SL-010.
/// </summary>
public sealed class StudylistViewModelTests
{
    private static (StudylistViewModel Vm, IStudyRepository StudyRepository) CreateSut()
    {
        var studyRepository = Substitute.For<IStudyRepository>();
        var vm = new StudylistViewModel(studyRepository);
        return (vm, studyRepository);
    }

    // ── Constructor tests ────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-SL-001")]
    public void Constructor_InitializesWithDefaultPacsServer()
    {
        var (vm, _) = CreateSut();

        vm.SelectedPacsServer.Should().Be("LOCAL", "first PACS server should be pre-selected");
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SL-001")]
    public void Constructor_PacsServersContainsThreeEntries()
    {
        var (vm, _) = CreateSut();

        vm.PacsServers.Should().HaveCount(3);
        vm.PacsServers.Should().Contain("LOCAL");
        vm.PacsServers.Should().Contain("PACS-01");
        vm.PacsServers.Should().Contain("PACS-02");
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SL-001")]
    public void Constructor_DefaultPeriodFilterIsAll()
    {
        var (vm, _) = CreateSut();

        vm.ActivePeriodFilter.Should().Be("All");
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SL-001")]
    public void Constructor_StudiesCollectionIsEmpty()
    {
        var (vm, _) = CreateSut();

        vm.Studies.Should().BeEmpty();
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SL-001")]
    public void Constructor_IsLoadingFalseByDefault()
    {
        var (vm, _) = CreateSut();

        vm.IsLoading.Should().BeFalse();
        vm.ErrorMessage.Should().BeNull();
    }

    // ── Property change notification tests ──────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-SL-002")]
    public void SearchQuery_WhenSet_RaisesPropertyChanged()
    {
        var (vm, _) = CreateSut();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.SearchQuery = "Kim";

        raised.Should().Contain(nameof(vm.SearchQuery));
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SL-002")]
    public void ActivePeriodFilter_WhenSet_RaisesPropertyChanged()
    {
        var (vm, _) = CreateSut();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.ActivePeriodFilter = "Today";

        raised.Should().Contain(nameof(vm.ActivePeriodFilter));
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SL-002")]
    public void SelectedPacsServer_WhenSet_RaisesPropertyChanged()
    {
        var (vm, _) = CreateSut();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.SelectedPacsServer = "PACS-01";

        raised.Should().Contain(nameof(vm.SelectedPacsServer));
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SL-002")]
    public void SelectedStudy_WhenSet_RaisesPropertyChanged()
    {
        var (vm, _) = CreateSut();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.SelectedStudy = MakeStudy("1.2.3.4");

        raised.Should().Contain(nameof(vm.SelectedStudy));
    }

    // ── FilterByPeriod command tests ─────────────────────────────────────────

    [Theory]
    [InlineData("Today")]
    [InlineData("3Days")]
    [InlineData("1Week")]
    [InlineData("1Month")]
    [InlineData("All")]
    [Trait("SWR", "SWR-UI-SL-003")]
    public async Task FilterByPeriodCommand_SetsActivePeriodFilter(string period)
    {
        var (vm, _) = CreateSut();

        vm.FilterByPeriodCommand.Execute(period);
        await Task.Delay(100);

        vm.ActivePeriodFilter.Should().Be(period);
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SL-003")]
    public async Task FilterByPeriodCommand_WithNull_DefaultsToAll()
    {
        var (vm, _) = CreateSut();
        vm.ActivePeriodFilter = "Today";

        vm.FilterByPeriodCommand.Execute(null);
        await Task.Delay(100);

        vm.ActivePeriodFilter.Should().Be("All");
    }

    // ── LoadStudies command tests ─────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-SL-004")]
    public async Task LoadStudiesCommand_SetsIsLoadingThenRestores()
    {
        var (vm, _) = CreateSut();

        vm.LoadStudiesCommand.Execute(null);
        await Task.Delay(100);

        vm.IsLoading.Should().BeFalse("IsLoading should be false after load completes");
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SL-004")]
    public async Task LoadStudiesCommand_DoesNotThrow()
    {
        var (vm, _) = CreateSut();

        Func<Task> act = async () =>
        {
            vm.LoadStudiesCommand.Execute(null);
            await Task.Delay(100);
        };

        await act.Should().NotThrowAsync();
    }

    // ── SelectStudy command tests ─────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-SL-005")]
    public void SelectStudyCommand_SetsSelectedStudy()
    {
        var (vm, _) = CreateSut();
        var study = MakeStudy("1.2.3.4");

        vm.SelectStudyCommand.Execute(study);

        vm.SelectedStudy.Should().Be(study);
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SL-005")]
    public void SelectStudyCommand_WithNull_SetsSelectedStudyToNull()
    {
        var (vm, _) = CreateSut();
        vm.SelectedStudy = MakeStudy("1.2.3.4");

        vm.SelectStudyCommand.Execute(null);

        vm.SelectedStudy.Should().BeNull();
    }

    // ── NavigatePrevious / NavigateNext command tests ────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-SL-006")]
    public void NavigatePreviousCommand_CanExecuteWithoutException()
    {
        var (vm, _) = CreateSut();

        var act = () => vm.NavigatePreviousCommand.Execute(null);

        act.Should().NotThrow();
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SL-006")]
    public void NavigateNextCommand_CanExecuteWithoutException()
    {
        var (vm, _) = CreateSut();

        var act = () => vm.NavigateNextCommand.Execute(null);

        act.Should().NotThrow();
    }

    // ── IViewModelBase explicit interface tests ──────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-SL-007")]
    public void IViewModelBase_IsLoading_ReflectsInstanceProperty()
    {
        var (vm, _) = CreateSut();
        var asBase = (HnVue.UI.Contracts.ViewModels.IViewModelBase)vm;

        asBase.IsLoading.Should().BeFalse();
    }

    // ── IStudylistViewModel command explicit mapping tests ───────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-SL-008")]
    public void IStudylistViewModel_CommandsAreExplicitlyMapped()
    {
        var (vm, _) = CreateSut();
        var iface = (HnVue.UI.Contracts.ViewModels.IStudylistViewModel)vm;

        iface.NavigatePreviousCommand.Should().NotBeNull();
        iface.NavigateNextCommand.Should().NotBeNull();
        iface.FilterByPeriodCommand.Should().NotBeNull();
        iface.LoadStudiesCommand.Should().NotBeNull();
        iface.SelectStudyCommand.Should().NotBeNull();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static StudyRecord MakeStudy(string studyInstanceUid) => new(
        StudyInstanceUid: studyInstanceUid,
        PatientId: "P-001",
        StudyDate: DateTimeOffset.Now,
        Description: "DR CHEST",
        AccessionNumber: "ACC-001",
        BodyPart: "CHEST");
}
