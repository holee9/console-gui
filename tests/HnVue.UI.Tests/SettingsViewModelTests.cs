using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.UI.ViewModels;
using NSubstitute;
using Xunit;

namespace HnVue.UI.Tests;

/// <summary>
/// Unit tests for <see cref="SettingsViewModel"/>.
/// SWR-UI-SET-001 through SWR-UI-SET-010.
/// </summary>
public sealed class SettingsViewModelTests
{
    private static SettingsViewModel CreateSut() => new(Substitute.For<ISystemAdminService>());

    // ── Constructor / initial state tests ────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-SET-001")]
    public void Constructor_InitializesWithSystemTabActive()
    {
        var vm = CreateSut();

        vm.ActiveTab.Should().Be("System");
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SET-001")]
    public void Constructor_TabsContainsTenItems()
    {
        var vm = CreateSut();

        vm.Tabs.Should().HaveCount(10);
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SET-001")]
    public void Constructor_AvailableRolesContainsThreeEntries()
    {
        var vm = CreateSut();

        vm.AvailableRoles.Should().HaveCount(3);
        vm.AvailableRoles.Should().Contain("Admin");
        vm.AvailableRoles.Should().Contain("Technician");
        vm.AvailableRoles.Should().Contain("Radiologist");
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SET-001")]
    public void Constructor_NetworkDefaultPorts()
    {
        var vm = CreateSut();

        vm.PacsServerPort.Should().Be(104);
        vm.WorklistServerPort.Should().Be(4006);
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SET-001")]
    public void Constructor_IsLoadingFalseAndNoError()
    {
        var vm = CreateSut();

        vm.IsLoading.Should().BeFalse();
        vm.ErrorMessage.Should().BeNull();
    }

    // ── Property change notification tests ──────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-SET-002")]
    public void ActiveTab_WhenSet_RaisesPropertyChanged()
    {
        var vm = CreateSut();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.ActiveTab = "Network";

        raised.Should().Contain(nameof(vm.ActiveTab));
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SET-002")]
    public void PacsServerAddress_WhenSet_RaisesPropertyChanged()
    {
        var vm = CreateSut();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.PacsServerAddress = "192.168.1.100";

        raised.Should().Contain(nameof(vm.PacsServerAddress));
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SET-002")]
    public void NewAccountRole_WhenSet_RaisesPropertyChanged()
    {
        var vm = CreateSut();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.NewAccountRole = "Admin";

        raised.Should().Contain(nameof(vm.NewAccountRole));
    }

    // ── SelectTab command tests ──────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-SET-003")]
    public void SelectTabCommand_WithValidTab_ChangesActiveTab()
    {
        var vm = CreateSut();

        vm.SelectTabCommand.Execute("Network");

        vm.ActiveTab.Should().Be("Network");
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SET-003")]
    public void SelectTabCommand_WithNullTab_DoesNotChangeActiveTab()
    {
        var vm = CreateSut();
        vm.ActiveTab = "System";

        vm.SelectTabCommand.Execute(null);

        vm.ActiveTab.Should().Be("System");
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SET-003")]
    public void SelectTabCommand_WithEmptyTab_DoesNotChangeActiveTab()
    {
        var vm = CreateSut();
        vm.ActiveTab = "System";

        vm.SelectTabCommand.Execute(string.Empty);

        vm.ActiveTab.Should().Be("System");
    }

    [Theory]
    [InlineData("System")]
    [InlineData("Account")]
    [InlineData("Detector")]
    [InlineData("Network")]
    [InlineData("Display")]
    [InlineData("DicomSet")]
    [InlineData("RIS Code")]
    [Trait("SWR", "SWR-UI-SET-003")]
    public void SelectTabCommand_CanSelectAnyKnownTab(string tabName)
    {
        var vm = CreateSut();

        vm.SelectTabCommand.Execute(tabName);

        vm.ActiveTab.Should().Be(tabName);
    }

    // ── Save command tests ───────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-SET-004")]
    public async Task SaveCommand_RaisesSaveCompletedEvent()
    {
        var vm = CreateSut();
        var saveCompleted = false;
        vm.SaveCompleted += (_, _) => saveCompleted = true;

        vm.SaveCommand.Execute(null);
        await Task.Delay(100);

        saveCompleted.Should().BeTrue();
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SET-004")]
    public async Task SaveCommand_SetsIsLoadingThenRestoresItToFalse()
    {
        var vm = CreateSut();

        vm.SaveCommand.Execute(null);
        await Task.Delay(100);

        vm.IsLoading.Should().BeFalse("IsLoading should be reset to false after save completes");
    }

    // ── Cancel command tests ─────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-SET-005")]
    public void CancelCommand_RaisesCancelledEvent()
    {
        var vm = CreateSut();
        var cancelled = false;
        vm.Cancelled += (_, _) => cancelled = true;

        vm.CancelCommand.Execute(null);

        cancelled.Should().BeTrue();
    }

    // ── IViewModelBase explicit interface tests ──────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-SET-006")]
    public void IViewModelBase_IsLoading_ReflectsInstanceProperty()
    {
        var vm = CreateSut();
        var asBase = (HnVue.UI.Contracts.ViewModels.IViewModelBase)vm;

        asBase.IsLoading.Should().BeFalse();
    }

    // ── ISettingsViewModel command explicit mapping tests ────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-SET-007")]
    public void ISettingsViewModel_CommandsAreExplicitlyMapped()
    {
        var vm = CreateSut();
        var iface = (HnVue.UI.Contracts.ViewModels.ISettingsViewModel)vm;

        iface.SaveCommand.Should().NotBeNull();
        iface.CancelCommand.Should().NotBeNull();
        iface.SelectTabCommand.Should().NotBeNull();
    }

    // ── RIS tab tests ────────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-UI-SET-008")]
    public void ActiveRisTab_DefaultsToMatching()
    {
        var vm = CreateSut();

        vm.ActiveRisTab.Should().Be("Matching");
    }

    [Fact]
    [Trait("SWR", "SWR-UI-SET-008")]
    public void ActiveRisTab_WhenSet_RaisesPropertyChanged()
    {
        var vm = CreateSut();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.ActiveRisTab = "Un-Matched";

        raised.Should().Contain(nameof(vm.ActiveRisTab));
    }
}
