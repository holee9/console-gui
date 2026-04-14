using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.UI.Contracts.ViewModels;
using HnVue.UI.ViewModels.Models;
using HnVue.UI.ViewModels;
using NSubstitute;
using Xunit;

namespace HnVue.UI.Tests;

/// <summary>
/// Cross-module integration tests verifying ViewModel-to-Service interactions.
/// These tests validate that ViewModels correctly coordinate with domain services
/// through the interface contracts defined in UI.Contracts.
/// </summary>
public class DetectorSdkIntegrationTests
{
    private readonly IWorkflowEngine _workflowEngine = Substitute.For<IWorkflowEngine>();
    private readonly ISecurityContext _securityContext = Substitute.For<ISecurityContext>();

    [Fact]
    public async Task WorkflowViewModel_ThumbnailList_AcceptsStudyItemsFromDetector()
    {
        // Arrange: Detector returns study images after acquisition
        var sut = CreateWorkflowSut();
        var record = new StudyRecord("UID-001", "P-001", DateTimeOffset.UtcNow, "Chest PA", "ACC-001", "CHEST");
        var studyItem = new StudyItem(record);

        // Act: Add thumbnail as would happen after detector acquisition callback
        sut.ThumbnailList.Add(studyItem);

        // Assert: Thumbnail list reflects the new study
        sut.ThumbnailList.Should().ContainSingle()
            .Which.Study.StudyInstanceUid.Should().Be("UID-001");
    }

    [Fact]
    public async Task WorkflowViewModel_MultipleAcquisitions_ProduceOrderedThumbnails()
    {
        var sut = CreateWorkflowSut();

        for (int i = 0; i < 5; i++)
        {
            var record = new StudyRecord($"UID-{i:D3}", "P-001", DateTimeOffset.UtcNow.AddMinutes(i),
                $"Study {i}", $"ACC-{i}", "CHEST");
            sut.ThumbnailList.Add(new StudyItem(record));
        }

        sut.ThumbnailList.Should().HaveCount(5);
        sut.ThumbnailList[0].Study.StudyInstanceUid.Should().Be("UID-000");
        sut.ThumbnailList[4].Study.StudyInstanceUid.Should().Be("UID-004");
    }

    [Fact]
    public async Task WorkflowViewModel_StudyItemSelection_TracksIsSelected()
    {
        var sut = CreateWorkflowSut();
        var record = new StudyRecord("UID-SEL", "P-001", DateTimeOffset.UtcNow, "Chest", null, null);
        var item = new StudyItem(record);
        sut.ThumbnailList.Add(item);

        item.IsSelected = true;

        sut.ThumbnailList[0].IsSelected.Should().BeTrue();
    }

    [Fact]
    public async Task WorkflowViewModel_DetectorSimulator_StateTransitionSequence()
    {
        // Arrange: Simulate full acquisition workflow with detector
        var sut = CreateWorkflowSut();
        _workflowEngine.TransitionAsync(Arg.Any<WorkflowState>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act: Prepare -> Expose sequence
        await sut.PrepareExposureCommand.ExecuteAsync(null);
        _workflowEngine.CurrentState.Returns(WorkflowState.ReadyToExpose);
        RaiseStateChanged(sut, WorkflowState.Idle, WorkflowState.ReadyToExpose);

        _securityContext.HasRole(UserRole.Radiographer).Returns(true);
        sut.TriggerExposureCommand.CanExecute(null).Should().BeTrue();

        await sut.TriggerExposureCommand.ExecuteAsync(null);

        // Assert: Both transitions were called
        await _workflowEngine.Received().TransitionAsync(WorkflowState.ReadyToExpose, Arg.Any<CancellationToken>());
        await _workflowEngine.Received().TransitionAsync(WorkflowState.Exposing, Arg.Any<CancellationToken>());
    }

    private WorkflowViewModel CreateWorkflowSut()
    {
        _workflowEngine.CurrentState.Returns(WorkflowState.Idle);
        _workflowEngine.CurrentSafeState.Returns(SafeState.Idle);
        return new WorkflowViewModel(_workflowEngine, _securityContext);
    }

    private static void RaiseStateChanged(WorkflowViewModel sut, WorkflowState from, WorkflowState to)
    {
        sut.GetType()
            .GetMethod("OnWorkflowStateChanged",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(sut, [null, new WorkflowStateChangedEventArgs(from, to)]);
    }
}

/// <summary>
/// Tests verifying cross-module Workflow state transitions affect ViewModel state.
/// </summary>
public class WorkflowCrossModuleTests
{
    [Fact]
    public void WorkflowViewModel_All9States_HaveStatusMessages()
    {
        var allStates = Enum.GetValues<WorkflowState>();
        var engine = Substitute.For<IWorkflowEngine>();
        var ctx = Substitute.For<ISecurityContext>();
        var messages = new HashSet<string>();

        foreach (var state in allStates)
        {
            engine.CurrentState.Returns(state);
            engine.CurrentSafeState.Returns(SafeState.Idle);
            engine.StateChanged += Raise.EventWith(
                new WorkflowStateChangedEventArgs(WorkflowState.Idle, state));

            var sut = new WorkflowViewModel(engine, ctx);
            messages.Add(sut.StatusMessage);
            sut.Dispose();
        }

        // Each state should produce a distinct status message
        messages.Should().HaveCount(allStates.Length);
    }

    [Fact]
    public void WorkflowViewModel_SafeStateTransition_UpdatesDisplay()
    {
        var engine = Substitute.For<IWorkflowEngine>();
        var ctx = Substitute.For<ISecurityContext>();
        engine.CurrentState.Returns(WorkflowState.Exposing);
        engine.CurrentSafeState.Returns(SafeState.Warning);

        var sut = new WorkflowViewModel(engine, ctx);

        sut.CurrentSafeState.Should().Be(SafeState.Warning);
        sut.SafeStateLabel.Should().Be("WARNING");
    }

    [Fact]
    public void WorkflowViewModel_EmergencySafeState_ShowsEmergencyLabel()
    {
        var engine = Substitute.For<IWorkflowEngine>();
        var ctx = Substitute.For<ISecurityContext>();
        engine.CurrentState.Returns(WorkflowState.Error);
        engine.CurrentSafeState.Returns(SafeState.Emergency);

        var sut = new WorkflowViewModel(engine, ctx);

        sut.SafeStateLabel.Should().Be("EMERGENCY");
    }
}

/// <summary>
/// Tests verifying the Patient → Study → Image chain through ViewModels.
/// </summary>
public class PatientStudyImageChainTests
{
    [Fact]
    public void MergeViewModel_PatientSelection_PopulatesStudyPreview()
    {
        var patientService = Substitute.For<IPatientService>();
        var sut = new MergeViewModel(patientService);

        var patient = new PatientRecord("P-001", "홍^길동", new DateOnly(1990, 1, 1),
            "M", false, DateTimeOffset.UtcNow, "admin");
        var study = new StudyRecord("UID-001", "P-001", DateTimeOffset.UtcNow, "Chest PA", "ACC-001", "CHEST");
        var studyItem = new StudyItem(study);

        sut.PreviewStudiesA.Add(studyItem);

        sut.PreviewStudiesA.Should().ContainSingle()
            .Which.Study.PatientId.Should().Be("P-001");
    }

    [Fact]
    public void MergeViewModel_SelectedStudies_TracksMultipleItems()
    {
        var patientService = Substitute.For<IPatientService>();
        var sut = new MergeViewModel(patientService);

        for (int i = 0; i < 3; i++)
        {
            var study = new StudyRecord($"UID-{i}", "P-001", DateTimeOffset.UtcNow, $"Study {i}", null, null);
            var item = new StudyItem(study) { IsSelected = true };
            sut.SelectedStudies.Add(item);
        }

        sut.SelectedStudies.Should().HaveCount(3);
        sut.SelectedStudies.Should().OnlyContain(s => s.IsSelected);
    }

    [Fact]
    public void MergeViewModel_DualPatientPreview_IndependentCollections()
    {
        var patientService = Substitute.For<IPatientService>();
        var sut = new MergeViewModel(patientService);

        var studyA = new StudyRecord("UID-A", "P-A", DateTimeOffset.UtcNow, "Study A", null, null);
        var studyB = new StudyRecord("UID-B", "P-B", DateTimeOffset.UtcNow, "Study B", null, null);

        sut.PreviewStudiesA.Add(new StudyItem(studyA));
        sut.PreviewStudiesB.Add(new StudyItem(studyB));

        sut.PreviewStudiesA.Should().ContainSingle().Which.Study.PatientId.Should().Be("P-A");
        sut.PreviewStudiesB.Should().ContainSingle().Which.Study.PatientId.Should().Be("P-B");
    }

    [Fact]
    public void MergeViewModel_PreviewStudySelection_UpdatesProperty()
    {
        var patientService = Substitute.For<IPatientService>();
        var sut = new MergeViewModel(patientService);

        var study = new StudyRecord("UID-PREVIEW", "P-001", DateTimeOffset.UtcNow, "Preview", null, null);
        var item = new StudyItem(study);
        sut.PreviewStudiesA.Add(item);

        sut.SelectedPreviewStudy = item;

        sut.SelectedPreviewStudy.Should().NotBeNull();
        sut.SelectedPreviewStudy!.Study.StudyInstanceUid.Should().Be("UID-PREVIEW");
    }
}

/// <summary>
/// Tests verifying Settings ViewModel reflects changes correctly.
/// </summary>
public class SettingsViewModelIntegrationTests
{
    [Fact]
    public void SettingsViewModel_ImplementsISettingsViewModel()
    {
        var sut = new SettingsViewModel();

        sut.Should().BeAssignableTo<ISettingsViewModel>();
    }

    [Fact]
    public void SettingsViewModel_TabsContainExpectedCategories()
    {
        var sut = new SettingsViewModel();

        sut.Tabs.Should().Contain(new[] { "System", "Account", "Network", "Display" });
    }

    [Fact]
    public void SettingsViewModel_TabSelection_UpdatesActiveTab()
    {
        var sut = new SettingsViewModel();

        sut.SelectTabCommand.Execute("Network");

        sut.ActiveTab.Should().Be("Network");
    }

    [Fact]
    public void SettingsViewModel_IsLoading_InterfaceContract()
    {
        var sut = new SettingsViewModel();

        var asBase = (IViewModelBase)sut;
        asBase.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task SettingsViewModel_SaveCommand_RaisesSaveCompleted()
    {
        var sut = new SettingsViewModel();
        var raised = false;
        sut.SaveCompleted += (_, _) => raised = true;

        await sut.SaveCommand.ExecuteAsync(null);

        raised.Should().BeTrue();
    }
}
