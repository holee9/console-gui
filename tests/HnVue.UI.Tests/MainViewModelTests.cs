using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.UI.Contracts.ViewModels;
using HnVue.UI.ViewModels;
using NSubstitute;
using Xunit;

// Suppress: child VMs need real objects in tests (NSubstitute cannot mock sealed source-generated partial classes)
#pragma warning disable CA2000

namespace HnVue.UI.Tests;

/// <summary>
/// Unit tests for <see cref="MainViewModel"/>.
/// </summary>
public sealed class MainViewModelTests
{
    private static MainViewModel CreateSut(ISecurityContext context) => new(
        context,
        Substitute.For<ISecurityService>(),
        new PatientListViewModel(Substitute.For<IPatientService>(), Substitute.For<IStudylistViewModel>()),
        new ImageViewerViewModel(Substitute.For<IImageProcessor>()),
        new WorkflowViewModel(Substitute.For<IWorkflowEngine>(), Substitute.For<ISecurityContext>()),
        new DoseDisplayViewModel(Substitute.For<IDoseService>()),
        new CDBurnViewModel(Substitute.For<ICDDVDBurnService>()),
        new SystemAdminViewModel(Substitute.For<ISystemAdminService>(), Substitute.For<ISecurityContext>()),
        Substitute.For<IStudylistViewModel>(),
        Substitute.For<IMergeViewModel>(),
        Substitute.For<ISettingsViewModel>());

    // ── Constructor guard test ───────────────────────────────────────────────

    [Fact]
    public void Constructor_WhenContextIsNull_ThrowsArgumentNullException()
    {
        var act = () => new MainViewModel(
            null!,
            Substitute.For<ISecurityService>(),
            new PatientListViewModel(Substitute.For<IPatientService>(), Substitute.For<IStudylistViewModel>()),
            new ImageViewerViewModel(Substitute.For<IImageProcessor>()),
            new WorkflowViewModel(Substitute.For<IWorkflowEngine>(), Substitute.For<ISecurityContext>()),
            new DoseDisplayViewModel(Substitute.For<IDoseService>()),
            new CDBurnViewModel(Substitute.For<ICDDVDBurnService>()),
            new SystemAdminViewModel(Substitute.For<ISystemAdminService>(), Substitute.For<ISecurityContext>()),
            Substitute.For<IStudylistViewModel>(),
            Substitute.For<IMergeViewModel>(),
            Substitute.For<ISettingsViewModel>());
        act.Should().Throw<ArgumentNullException>().WithParameterName("securityContext");
    }

    // ── RefreshFromContext tests ──────────────────────────────────────────────

    [Fact]
    public void RefreshFromContext_WhenAuthenticated_SetsIsAuthenticatedTrue()
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(true);
        context.CurrentUsername.Returns("john");
        context.CurrentRole.Returns(UserRole.Radiologist);

        var vm = CreateSut(context);
        vm.RefreshFromContext();

        vm.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void RefreshFromContext_WhenAuthenticated_SetsCurrentUsername()
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(true);
        context.CurrentUsername.Returns("john");
        context.CurrentRole.Returns(UserRole.Radiologist);

        var vm = CreateSut(context);
        vm.RefreshFromContext();

        vm.CurrentUsername.Should().Be("john");
    }

    [Fact]
    public void RefreshFromContext_WhenAuthenticated_SetsCurrentRoleDisplay()
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(true);
        context.CurrentUsername.Returns("john");
        context.CurrentRole.Returns(UserRole.Radiologist);

        var vm = CreateSut(context);
        vm.RefreshFromContext();

        vm.CurrentRoleDisplay.Should().Be(UserRole.Radiologist.ToString());
    }

    [Fact]
    public void RefreshFromContext_WhenNotAuthenticated_SetsIsAuthenticatedFalse()
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(false);
        context.CurrentUsername.Returns((string?)null);
        context.CurrentRole.Returns((UserRole?)null);

        var vm = CreateSut(context);
        vm.RefreshFromContext();

        vm.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void RefreshFromContext_WhenNotAuthenticated_CurrentUsernameIsNull()
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(false);
        context.CurrentUsername.Returns((string?)null);
        context.CurrentRole.Returns((UserRole?)null);

        var vm = CreateSut(context);
        vm.RefreshFromContext();

        vm.CurrentUsername.Should().BeNull();
    }

    [Fact]
    public void RefreshFromContext_WhenNotAuthenticated_CurrentRoleDisplayIsNull()
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(false);
        context.CurrentUsername.Returns((string?)null);
        context.CurrentRole.Returns((UserRole?)null);

        var vm = CreateSut(context);
        vm.RefreshFromContext();

        vm.CurrentRoleDisplay.Should().BeNull();
    }

    [Theory]
    [InlineData(UserRole.Radiographer)]
    [InlineData(UserRole.Radiologist)]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Service)]
    public void RefreshFromContext_AllRoles_SetsRoleDisplayCorrectly(UserRole role)
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(true);
        context.CurrentUsername.Returns("user");
        context.CurrentRole.Returns(role);

        var vm = CreateSut(context);
        vm.RefreshFromContext();

        vm.CurrentRoleDisplay.Should().Be(role.ToString());
    }

    // ── OnLoginSuccess tests ────────────────────────────────────────────────

    [Fact]
    public void OnLoginSuccess_SetsCurrentUsername()
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(true);
        context.CurrentUsername.Returns("john");
        context.CurrentRole.Returns(UserRole.Admin);

        var vm = CreateSut(context);
        var user = new HnVue.Common.Models.AuthenticatedUser("u1", "john", UserRole.Admin);
        vm.OnLoginSuccess(user);

        vm.CurrentUsername.Should().Be("john");
    }

    [Fact]
    public void OnLoginSuccess_HidesLogin()
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(true);
        context.CurrentUsername.Returns("john");

        var vm = CreateSut(context);
        var user = new HnVue.Common.Models.AuthenticatedUser("u1", "john", UserRole.Admin);
        vm.OnLoginSuccess(user);

        vm.IsLoginVisible.Should().BeFalse();
    }

    [Fact]
    public void OnLoginSuccess_ShowsMainContent()
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(true);
        context.CurrentUsername.Returns("john");

        var vm = CreateSut(context);
        var user = new HnVue.Common.Models.AuthenticatedUser("u1", "john", UserRole.Admin);
        vm.OnLoginSuccess(user);

        vm.IsMainContentVisible.Should().BeTrue();
    }

    [Fact]
    public void OnLoginSuccess_NavigatesToPatientList()
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(true);
        context.CurrentUsername.Returns("john");

        var vm = CreateSut(context);
        var user = new HnVue.Common.Models.AuthenticatedUser("u1", "john", UserRole.Admin);
        vm.OnLoginSuccess(user);

        vm.ActiveNavItem.Should().Be("PatientList");
    }

    // ── NavigateTo tests ────────────────────────────────────────────────────

    [Theory]
    [InlineData(HnVue.UI.Contracts.Navigation.NavigationToken.Workflow)]
    [InlineData(HnVue.UI.Contracts.Navigation.NavigationToken.ImageViewer)]
    [InlineData(HnVue.UI.Contracts.Navigation.NavigationToken.DoseDisplay)]
    [InlineData(HnVue.UI.Contracts.Navigation.NavigationToken.CDBurn)]
    [InlineData(HnVue.UI.Contracts.Navigation.NavigationToken.SystemAdmin)]
    [InlineData(HnVue.UI.Contracts.Navigation.NavigationToken.Studylist)]
    [InlineData(HnVue.UI.Contracts.Navigation.NavigationToken.Merge)]
    [InlineData(HnVue.UI.Contracts.Navigation.NavigationToken.Settings)]
    public void NavigateTo_SetsActiveNavItem(HnVue.UI.Contracts.Navigation.NavigationToken token)
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);
        vm.NavigateTo(token);

        vm.ActiveNavItem.Should().Be(token.ToString());
    }

    [Fact]
    public void NavigateTo_PatientList_SetsCurrentViewToPatientListViewModel()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);
        vm.NavigateTo(HnVue.UI.Contracts.Navigation.NavigationToken.PatientList);

        vm.CurrentView.Should().Be(vm.PatientListViewModel);
    }

    [Fact]
    public void NavigateTo_Workflow_SetsCurrentViewToWorkflowViewModel()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);
        vm.NavigateTo(HnVue.UI.Contracts.Navigation.NavigationToken.Workflow);

        vm.CurrentView.Should().Be(vm.WorkflowViewModel);
    }

    [Fact]
    public void NavigateTo_ImageViewer_SetsCurrentViewToImageViewerViewModel()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);
        vm.NavigateTo(HnVue.UI.Contracts.Navigation.NavigationToken.ImageViewer);

        vm.CurrentView.Should().Be(vm.ImageViewerViewModel);
    }

    [Fact]
    public void NavigateTo_DoseDisplay_SetsCurrentViewToDoseDisplayViewModel()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);
        vm.NavigateTo(HnVue.UI.Contracts.Navigation.NavigationToken.DoseDisplay);

        vm.CurrentView.Should().Be(vm.DoseDisplayViewModel);
    }

    [Fact]
    public void NavigateTo_CDBurn_SetsCurrentViewToCDBurnViewModel()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);
        vm.NavigateTo(HnVue.UI.Contracts.Navigation.NavigationToken.CDBurn);

        vm.CurrentView.Should().Be(vm.CDBurnViewModel);
    }

    [Fact]
    public void NavigateTo_SystemAdmin_SetsCurrentViewToSystemAdminViewModel()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);
        vm.NavigateTo(HnVue.UI.Contracts.Navigation.NavigationToken.SystemAdmin);

        vm.CurrentView.Should().Be(vm.SystemAdminViewModel);
    }

    [Fact]
    public void NavigateTo_Studylist_SetsCurrentViewToStudylistViewModel()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);
        vm.NavigateTo(HnVue.UI.Contracts.Navigation.NavigationToken.Studylist);

        vm.CurrentView.Should().Be(vm.StudylistViewModel);
    }

    [Fact]
    public void NavigateTo_Merge_SetsCurrentViewToMergeViewModel()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);
        vm.NavigateTo(HnVue.UI.Contracts.Navigation.NavigationToken.Merge);

        vm.CurrentView.Should().Be(vm.MergeViewModel);
    }

    [Fact]
    public void NavigateTo_Settings_SetsCurrentViewToSettingsViewModel()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);
        vm.NavigateTo(HnVue.UI.Contracts.Navigation.NavigationToken.Settings);

        vm.CurrentView.Should().Be(vm.SettingsViewModel);
    }

    // ── NavigateBack tests ──────────────────────────────────────────────────

    [Fact]
    public void NavigateBack_WithHistory_ReturnsToPreviousView()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);
        vm.NavigateTo(HnVue.UI.Contracts.Navigation.NavigationToken.PatientList);
        vm.NavigateTo(HnVue.UI.Contracts.Navigation.NavigationToken.Workflow);

        vm.NavigateBack();

        vm.CurrentView.Should().Be(vm.PatientListViewModel);
        vm.ActiveNavItem.Should().Be("PatientList");
    }

    [Fact]
    public void NavigateBack_EmptyHistory_DoesNothing()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);

        // Should not throw
        vm.NavigateBack();
    }

    [Fact]
    public void NavigationHistory_ReturnsCorrectEntries()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);
        vm.NavigateTo(HnVue.UI.Contracts.Navigation.NavigationToken.PatientList);
        vm.NavigateTo(HnVue.UI.Contracts.Navigation.NavigationToken.Workflow);
        vm.NavigateTo(HnVue.UI.Contracts.Navigation.NavigationToken.ImageViewer);

        vm.NavigationHistory.Should().HaveCountGreaterOrEqualTo(2);
    }

    // ── Logout tests ────────────────────────────────────────────────────────

    [Fact]
    public void Logout_ClearsAuthenticationState()
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(true);
        context.CurrentUsername.Returns("john");
        context.CurrentUserId.Returns("u1");

        var vm = CreateSut(context);
        var user = new HnVue.Common.Models.AuthenticatedUser("u1", "john", UserRole.Admin);
        vm.OnLoginSuccess(user);
        vm.LogoutCommand.Execute(null);

        vm.CurrentUsername.Should().BeNull();
        vm.IsAuthenticated.Should().BeFalse();
        vm.IsMainContentVisible.Should().BeFalse();
        vm.IsLoginVisible.Should().BeTrue();
    }

    [Fact]
    public void Logout_ClearsActiveNavItem()
    {
        var context = Substitute.For<ISecurityContext>();
        context.CurrentUserId.Returns("u1");

        var vm = CreateSut(context);
        vm.NavigateTo(HnVue.UI.Contracts.Navigation.NavigationToken.PatientList);
        vm.LogoutCommand.Execute(null);

        vm.ActiveNavItem.Should().BeEmpty();
    }

    // ── Static property tests ───────────────────────────────────────────────

    [Fact]
    public void IsLoading_AlwaysReturnsFalse()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);

        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void ErrorMessage_AlwaysReturnsNull()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);

        vm.ErrorMessage.Should().BeNull();
    }

    // ── Dispose tests ───────────────────────────────────────────────────────

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);

        var act = () => vm.Dispose();
        act.Should().NotThrow();
    }

    // ── Navigate command tests ──────────────────────────────────────────────

    [Fact]
    public void NavigateCommand_SetsActiveNavItem()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);

        vm.NavigateCommand.Execute("Workflow");

        vm.ActiveNavItem.Should().Be("Workflow");
    }

    // ── Emergency command tests ─────────────────────────────────────────────

    [Fact]
    public void EmergencyCommand_SetsActiveNavItemToEmergency()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);

        vm.EmergencyCommand.Execute(null);

        vm.ActiveNavItem.Should().Be("Emergency");
    }

    // ── Session timer tests ─────────────────────────────────────────────────

    [Fact]
    public void ResetSessionTimer_ResetsCountdown()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);

        vm.ResetSessionTimer();

        vm.SessionTimeoutCountdown.Should().Be(0);
        vm.IsTimeoutWarningVisible.Should().BeFalse();
    }

    // ── TLS inactive property ───────────────────────────────────────────────

    [Fact]
    public void IsTlsInactive_DefaultIsFalse()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);

        vm.IsTlsInactive.Should().BeFalse();
    }

    [Fact]
    public void IsTlsInactive_CanBeSet()
    {
        var context = Substitute.For<ISecurityContext>();
        var vm = CreateSut(context);
        vm.IsTlsInactive = true;

        vm.IsTlsInactive.Should().BeTrue();
    }

    // ── Constructor null guard for all parameters ────────────────────────────

    [Fact]
    public void Constructor_WhenSecurityServiceIsNull_ThrowsArgumentNullException()
    {
        var act = () => new MainViewModel(
            Substitute.For<ISecurityContext>(),
            null!,
            new PatientListViewModel(Substitute.For<IPatientService>(), Substitute.For<IStudylistViewModel>()),
            new ImageViewerViewModel(Substitute.For<IImageProcessor>()),
            new WorkflowViewModel(Substitute.For<IWorkflowEngine>(), Substitute.For<ISecurityContext>()),
            new DoseDisplayViewModel(Substitute.For<IDoseService>()),
            new CDBurnViewModel(Substitute.For<ICDDVDBurnService>()),
            new SystemAdminViewModel(Substitute.For<ISystemAdminService>(), Substitute.For<ISecurityContext>()),
            Substitute.For<IStudylistViewModel>(),
            Substitute.For<IMergeViewModel>(),
            Substitute.For<ISettingsViewModel>());
        act.Should().Throw<ArgumentNullException>().WithParameterName("securityService");
    }

    [Fact]
    public void Constructor_WhenPatientListViewModelIsNull_ThrowsArgumentNullException()
    {
        var act = () => new MainViewModel(
            Substitute.For<ISecurityContext>(),
            Substitute.For<ISecurityService>(),
            null!,
            new ImageViewerViewModel(Substitute.For<IImageProcessor>()),
            new WorkflowViewModel(Substitute.For<IWorkflowEngine>(), Substitute.For<ISecurityContext>()),
            new DoseDisplayViewModel(Substitute.For<IDoseService>()),
            new CDBurnViewModel(Substitute.For<ICDDVDBurnService>()),
            new SystemAdminViewModel(Substitute.For<ISystemAdminService>(), Substitute.For<ISecurityContext>()),
            Substitute.For<IStudylistViewModel>(),
            Substitute.For<IMergeViewModel>(),
            Substitute.For<ISettingsViewModel>());
        act.Should().Throw<ArgumentNullException>().WithParameterName("patientListViewModel");
    }
}
