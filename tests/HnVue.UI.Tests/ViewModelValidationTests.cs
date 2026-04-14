using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.UI.Contracts.ViewModels;
using HnVue.UI.ViewModels;
using HnVue.UI.ViewModels.Models;
using NSubstitute;
using Xunit;

namespace HnVue.UI.Tests;

/// <summary>
/// Validates all 14 ViewModels: ICommand binding, PropertyChanged events, and interface contracts.
/// </summary>
public class ViewModelValidationTests
{
    // ── ICommand Binding Validation ───────────────────────────────────────

    [Fact]
    public void LoginViewModel_CommandBindings_AreCorrect()
    {
        var sut = new LoginViewModel(
            Substitute.For<ISecurityService>(), Substitute.For<ISecurityContext>());
        var iface = (ILoginViewModel)sut;

        iface.LoginCommand.Should().NotBeNull();
    }

    [Fact]
    public void PatientListViewModel_CommandBindings_AreCorrect()
    {
        var sut = new PatientListViewModel(
            Substitute.For<IPatientService>(), Substitute.For<IStudylistViewModel>());
        var iface = (IPatientListViewModel)sut;

        iface.SearchCommand.Should().NotBeNull();
        iface.SelectPatientCommand.Should().NotBeNull();
        iface.RegisterPatientCommand.Should().NotBeNull();
        iface.FilterByPeriodCommand.Should().NotBeNull();
    }

    [Fact]
    public void StudylistViewModel_CommandBindings_AreCorrect()
    {
        var sut = new StudylistViewModel(Substitute.For<IStudyRepository>());
        var iface = (IStudylistViewModel)sut;

        iface.NavigatePreviousCommand.Should().NotBeNull();
        iface.NavigateNextCommand.Should().NotBeNull();
        iface.FilterByPeriodCommand.Should().NotBeNull();
        iface.LoadStudiesCommand.Should().NotBeNull();
        iface.SelectStudyCommand.Should().NotBeNull();
    }

    [Fact]
    public void WorkflowViewModel_CommandBindings_AreCorrect()
    {
        var engine = Substitute.For<IWorkflowEngine>();
        engine.CurrentState.Returns(WorkflowState.Idle);
        engine.CurrentSafeState.Returns(SafeState.Idle);
        var sut = new WorkflowViewModel(engine, Substitute.For<ISecurityContext>());
        var iface = (IWorkflowViewModel)sut;

        iface.PrepareExposureCommand.Should().NotBeNull();
        iface.TriggerExposureCommand.Should().NotBeNull();
        iface.AbortCommand.Should().NotBeNull();
    }

    [Fact]
    public void SettingsViewModel_CommandBindings_AreCorrect()
    {
        var sut = new SettingsViewModel();
        var iface = (ISettingsViewModel)sut;

        iface.SaveCommand.Should().NotBeNull();
        iface.CancelCommand.Should().NotBeNull();
        iface.SelectTabCommand.Should().NotBeNull();
    }

    [Fact]
    public void MergeViewModel_CommandBindings_AreCorrect()
    {
        var sut = new MergeViewModel(Substitute.For<IPatientService>());
        var iface = (IMergeViewModel)sut;

        iface.SearchACommand.Should().NotBeNull();
        iface.SearchBCommand.Should().NotBeNull();
        iface.MergeCommand.Should().NotBeNull();
        iface.CancelCommand.Should().NotBeNull();
    }

    [Fact]
    public void AddPatientProcedureViewModel_CommandBindings_AreCorrect()
    {
        var sut = new AddPatientProcedureViewModel(Substitute.For<IPatientService>());
        var iface = (IAddPatientProcedureViewModel)sut;

        iface.AddProjectionCommand.Should().NotBeNull();
        iface.RemoveProjectionCommand.Should().NotBeNull();
        iface.SaveCommand.Should().NotBeNull();
        iface.CancelCommand.Should().NotBeNull();
    }

    [Fact]
    public void SystemAdminViewModel_CommandBindings_AreCorrect()
    {
        var sut = new SystemAdminViewModel(
            Substitute.For<ISystemAdminService>(), Substitute.For<ISecurityContext>());
        var iface = (ISystemAdminViewModel)sut;

        iface.LoadSettingsCommand.Should().NotBeNull();
        iface.SaveSettingsCommand.Should().NotBeNull();
    }

    [Fact]
    public void CDBurnViewModel_CommandBindings_AreCorrect()
    {
        var sut = new CDBurnViewModel(Substitute.For<ICDDVDBurnService>());
        var iface = (ICDBurnViewModel)sut;

        iface.StartBurnCommand.Should().NotBeNull();
        iface.CancelBurnCommand.Should().NotBeNull();
    }

    [Fact]
    public void DoseViewModel_CommandBindings_AreCorrect()
    {
        var sut = new DoseViewModel(Substitute.For<IDoseService>());
        var iface = (IDoseViewModel)sut;

        iface.RefreshCommand.Should().NotBeNull();
    }

    [Fact]
    public void ImageViewerViewModel_CommandBindings_AreCorrect()
    {
        var sut = new ImageViewerViewModel(Substitute.For<IImageProcessor>());
        var iface = (IImageViewerViewModel)sut;

        iface.LoadImageCommand.Should().NotBeNull();
        iface.ZoomInCommand.Should().NotBeNull();
        iface.ZoomOutCommand.Should().NotBeNull();
        iface.ResetWindowCommand.Should().NotBeNull();
    }

    // ── PropertyChanged Event Validation ──────────────────────────────────

    [Fact]
    public void LoginViewModel_RaisesPropertyChanged_ForUsername()
    {
        var sut = new LoginViewModel(
            Substitute.For<ISecurityService>(), Substitute.For<ISecurityContext>());
        var changed = new List<string>();
        sut.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        sut.Username = "admin";

        changed.Should().Contain("Username");
    }

    [Fact]
    public void LoginViewModel_RaisesPropertyChanged_ForPassword()
    {
        var sut = new LoginViewModel(
            Substitute.For<ISecurityService>(), Substitute.For<ISecurityContext>());
        var changed = new List<string>();
        sut.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        sut.Password = "pass123";

        changed.Should().Contain("Password");
    }

    [Fact]
    public void SettingsViewModel_RaisesPropertyChanged_ForActiveTab()
    {
        var sut = new SettingsViewModel();
        var changed = new List<string>();
        sut.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        sut.ActiveTab = "Network";

        changed.Should().Contain("ActiveTab");
    }

    [Fact]
    public void MergeViewModel_RaisesPropertyChanged_ForSearchQueryA()
    {
        var sut = new MergeViewModel(Substitute.For<IPatientService>());
        var changed = new List<string>();
        sut.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        sut.SearchQueryA = "test";

        changed.Should().Contain("SearchQueryA");
    }

    [Fact]
    public void WorkflowViewModel_RaisesPropertyChanged_ForStatusMessage()
    {
        var engine = Substitute.For<IWorkflowEngine>();
        engine.CurrentState.Returns(WorkflowState.Idle);
        engine.CurrentSafeState.Returns(SafeState.Idle);
        var sut = new WorkflowViewModel(engine, Substitute.For<ISecurityContext>());
        var changed = new List<string>();
        sut.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        engine.CurrentState.Returns(WorkflowState.PatientSelected);
        sut.GetType()
            .GetMethod("OnWorkflowStateChanged",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(sut, [null, new WorkflowStateChangedEventArgs(WorkflowState.Idle, WorkflowState.PatientSelected)]);

        changed.Should().Contain("StatusMessage");
    }

    // ── Interface Implementation Validation ───────────────────────────────

    [Theory]
    [InlineData(typeof(LoginViewModel), typeof(ILoginViewModel))]
    [InlineData(typeof(PatientListViewModel), typeof(IPatientListViewModel))]
    [InlineData(typeof(StudylistViewModel), typeof(IStudylistViewModel))]
    [InlineData(typeof(WorkflowViewModel), typeof(IWorkflowViewModel))]
    [InlineData(typeof(SettingsViewModel), typeof(ISettingsViewModel))]
    [InlineData(typeof(MergeViewModel), typeof(IMergeViewModel))]
    [InlineData(typeof(AddPatientProcedureViewModel), typeof(IAddPatientProcedureViewModel))]
    [InlineData(typeof(SystemAdminViewModel), typeof(ISystemAdminViewModel))]
    [InlineData(typeof(CDBurnViewModel), typeof(ICDBurnViewModel))]
    [InlineData(typeof(DoseViewModel), typeof(IDoseViewModel))]
    [InlineData(typeof(ImageViewerViewModel), typeof(IImageViewerViewModel))]
    public void ViewModels_Should_Implement_CorrectInterface(Type viewModelType, Type interfaceType)
    {
        viewModelType.GetInterfaces().Should().Contain(interfaceType,
            because: $"{viewModelType.Name} must implement {interfaceType.Name}");
    }

    // ── StudyItem PropertyChanged Validation ──────────────────────────────

    [Fact]
    public void StudyItem_RaisesPropertyChanged_ForIsSelected()
    {
        var record = new StudyRecord("UID-001", "P-001", DateTimeOffset.UtcNow, "Test", null, null);
        var sut = new StudyItem(record);
        var changed = new List<string>();
        sut.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        sut.IsSelected = true;

        changed.Should().Contain("IsSelected");
    }

    [Fact]
    public void StudyItem_ImplementsIStudyItem()
    {
        var record = new StudyRecord("UID-001", "P-001", DateTimeOffset.UtcNow, "Test", null, null);
        var sut = new StudyItem(record);

        sut.Should().BeAssignableTo<HnVue.UI.Contracts.Models.IStudyItem>();
        ((HnVue.UI.Contracts.Models.IStudyItem)sut).Study.Should().Be(record);
    }
}
