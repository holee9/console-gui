using FluentAssertions;
using HnVue.UI.DesignTime;
using Xunit;

namespace HnVue.UI.Tests;

/// <summary>
/// Unit tests for DesignTime ViewModels.
/// Verifies mock data initialization, property defaults, and command execution.
/// These ViewModels keep the VS2022 designer usable without running the application.
/// </summary>
public class DesignTimeViewModelTests
{
    // ====================================================================
    // DesignTimeMergeViewModel
    // ====================================================================

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeMergeViewModel")]
    public void DesignTimeMergeViewModel_Constructor_PopulatesPatientsA()
    {
        var vm = new DesignTimeMergeViewModel();
        vm.PatientsA.Should().NotBeEmpty();
        vm.PatientsA.Count.Should().Be(5);
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeMergeViewModel")]
    public void DesignTimeMergeViewModel_Constructor_PopulatesPatientsB()
    {
        var vm = new DesignTimeMergeViewModel();
        vm.PatientsB.Should().NotBeEmpty();
        vm.PatientsB.Count.Should().Be(5);
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeMergeViewModel")]
    public void DesignTimeMergeViewModel_Constructor_PopulatesStudiesA()
    {
        var vm = new DesignTimeMergeViewModel();
        vm.StudiesA.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeMergeViewModel")]
    public void DesignTimeMergeViewModel_Constructor_PopulatesStudiesB()
    {
        var vm = new DesignTimeMergeViewModel();
        vm.StudiesB.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeMergeViewModel")]
    public void DesignTimeMergeViewModel_Constructor_PopulatesPreviewStudiesA()
    {
        var vm = new DesignTimeMergeViewModel();
        vm.PreviewStudiesA.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeMergeViewModel")]
    public void DesignTimeMergeViewModel_Constructor_PopulatesPreviewStudiesB()
    {
        var vm = new DesignTimeMergeViewModel();
        vm.PreviewStudiesB.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeMergeViewModel")]
    public void DesignTimeMergeViewModel_Constructor_SetsSelectedPatientA()
    {
        var vm = new DesignTimeMergeViewModel();
        vm.SelectedPatientA.Should().NotBeNull();
        vm.SelectedPatientA.Should().Be(vm.PatientsA[0]);
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeMergeViewModel")]
    public void DesignTimeMergeViewModel_Constructor_SetsSelectedPatientB()
    {
        var vm = new DesignTimeMergeViewModel();
        vm.SelectedPatientB.Should().NotBeNull();
        vm.SelectedPatientB.Should().Be(vm.PatientsB[1]);
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeMergeViewModel")]
    public void DesignTimeMergeViewModel_Constructor_SetsSelectedStudyA()
    {
        var vm = new DesignTimeMergeViewModel();
        vm.SelectedStudyA.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeMergeViewModel")]
    public void DesignTimeMergeViewModel_Constructor_SetsSelectedStudyB()
    {
        var vm = new DesignTimeMergeViewModel();
        vm.SelectedStudyB.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeMergeViewModel")]
    public void DesignTimeMergeViewModel_Defaults_SearchQueriesEmpty()
    {
        var vm = new DesignTimeMergeViewModel();
        vm.SearchQueryA.Should().BeEmpty();
        vm.SearchQueryB.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeMergeViewModel")]
    public void DesignTimeMergeViewModel_Defaults_ErrorMessageNull()
    {
        var vm = new DesignTimeMergeViewModel();
        vm.ErrorMessage.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeMergeViewModel")]
    public void DesignTimeMergeViewModel_Defaults_IsSyncEnabledTrue()
    {
        var vm = new DesignTimeMergeViewModel();
        vm.IsSyncEnabled.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeMergeViewModel")]
    public void DesignTimeMergeViewModel_Defaults_OnlyWorkListFalse()
    {
        var vm = new DesignTimeMergeViewModel();
        vm.OnlyWorkListA.Should().BeFalse();
        vm.OnlyWorkListB.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeMergeViewModel")]
    public void DesignTimeMergeViewModel_Commands_NotNull()
    {
        var vm = new DesignTimeMergeViewModel();
        vm.SearchACommand.Should().NotBeNull();
        vm.SearchBCommand.Should().NotBeNull();
        vm.CancelCommand.Should().NotBeNull();
        vm.MergeCommand.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeMergeViewModel")]
    public void DesignTimeMergeViewModel_Commands_CanExecute()
    {
        var vm = new DesignTimeMergeViewModel();
        vm.SearchACommand.CanExecute(null).Should().BeTrue();
        vm.SearchBCommand.CanExecute(null).Should().BeTrue();
        vm.CancelCommand.CanExecute(null).Should().BeTrue();
        vm.MergeCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeMergeViewModel")]
    public void DesignTimeMergeViewModel_PatientName_HasDicomFormat()
    {
        var vm = new DesignTimeMergeViewModel();
        vm.PatientsA[0].Name.Should().Contain("^"); // DICOM PN: family^given
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeMergeViewModel")]
    public void DesignTimeMergeViewModel_StudyItem_HasRequiredFields()
    {
        var vm = new DesignTimeMergeViewModel();
        var study = vm.StudiesA[0];
        study.Description.Should().NotBeNullOrEmpty();
        study.BodyPart.Should().NotBeNullOrEmpty();
        study.StudyDate.Should().NotBeNullOrEmpty();
    }

    // ====================================================================
    // DesignTimeSettingsViewModel
    // ====================================================================

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeSettingsViewModel")]
    public void DesignTimeSettingsViewModel_Constructor_DefaultsActiveTab()
    {
        var vm = new DesignTimeSettingsViewModel();
        vm.ActiveTab.Should().Be("System");
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeSettingsViewModel")]
    public void DesignTimeSettingsViewModel_Constructor_PopulatesTabs()
    {
        var vm = new DesignTimeSettingsViewModel();
        vm.Tabs.Should().NotBeEmpty();
        vm.Tabs.Count.Should().Be(10);
        vm.Tabs.Should().Contain("System");
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeSettingsViewModel")]
    public void DesignTimeSettingsViewModel_Constructor_DefaultsNetworkSettings()
    {
        var vm = new DesignTimeSettingsViewModel();
        vm.PacsServerAddress.Should().NotBeNullOrEmpty();
        vm.PacsServerPort.Should().Be(104);
        vm.WorklistServerAddress.Should().NotBeNullOrEmpty();
        vm.WorklistServerPort.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeSettingsViewModel")]
    public void DesignTimeSettingsViewModel_Constructor_DefaultsAccountSettings()
    {
        var vm = new DesignTimeSettingsViewModel();
        vm.NewAccountId.Should().NotBeNullOrEmpty();
        vm.NewAccountRole.Should().NotBeNullOrEmpty();
        vm.AvailableRoles.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeSettingsViewModel")]
    public void DesignTimeSettingsViewModel_Constructor_DefaultsErrorMessageNull()
    {
        var vm = new DesignTimeSettingsViewModel();
        vm.ErrorMessage.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeSettingsViewModel")]
    public void DesignTimeSettingsViewModel_Commands_NotNull()
    {
        var vm = new DesignTimeSettingsViewModel();
        vm.SelectTabCommand.Should().NotBeNull();
        vm.SaveCommand.Should().NotBeNull();
        vm.CancelCommand.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeSettingsViewModel")]
    public void DesignTimeSettingsViewModel_SelectTabCommand_UpdatesActiveTab()
    {
        var vm = new DesignTimeSettingsViewModel();
        vm.SelectTabCommand.Execute("Network");
        vm.ActiveTab.Should().Be("Network");
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeSettingsViewModel")]
    public void DesignTimeSettingsViewModel_SelectTabCommand_IgnoresNullTab()
    {
        var vm = new DesignTimeSettingsViewModel();
        var originalTab = vm.ActiveTab;
        vm.SelectTabCommand.Execute(null);
        vm.ActiveTab.Should().Be(originalTab);
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeSettingsViewModel")]
    public void DesignTimeSettingsViewModel_SelectTabCommand_IgnoresEmptyTab()
    {
        var vm = new DesignTimeSettingsViewModel();
        var originalTab = vm.ActiveTab;
        vm.SelectTabCommand.Execute(string.Empty);
        vm.ActiveTab.Should().Be(originalTab);
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeSettingsViewModel")]
    public void DesignTimeSettingsViewModel_AccessNoticeText_NotEmpty()
    {
        var vm = new DesignTimeSettingsViewModel();
        vm.AccessNoticeText.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeSettingsViewModel")]
    public void DesignTimeSettingsViewModel_RISTab_DefaultsToMatching()
    {
        var vm = new DesignTimeSettingsViewModel();
        vm.ActiveRisTab.Should().Be("Matching");
    }

    // ====================================================================
    // DesignTimeStudylistViewModel
    // ====================================================================

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeStudylistViewModel")]
    public void DesignTimeStudylistViewModel_Constructor_PopulatesStudies()
    {
        var vm = new DesignTimeStudylistViewModel();
        vm.Studies.Should().NotBeEmpty();
        vm.Studies.Count.Should().Be(10);
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeStudylistViewModel")]
    public void DesignTimeStudylistViewModel_Constructor_DefaultsSearchQueryEmpty()
    {
        var vm = new DesignTimeStudylistViewModel();
        vm.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeStudylistViewModel")]
    public void DesignTimeStudylistViewModel_Constructor_DefaultsPeriodFilter()
    {
        var vm = new DesignTimeStudylistViewModel();
        vm.ActivePeriodFilter.Should().Be("All");
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeStudylistViewModel")]
    public void DesignTimeStudylistViewModel_Constructor_DefaultsPacsServer()
    {
        var vm = new DesignTimeStudylistViewModel();
        vm.SelectedPacsServer.Should().Be("LOCAL");
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeStudylistViewModel")]
    public void DesignTimeStudylistViewModel_Constructor_PopulatesPacsServers()
    {
        var vm = new DesignTimeStudylistViewModel();
        vm.PacsServers.Should().NotBeEmpty();
        vm.PacsServers.Should().Contain("LOCAL");
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeStudylistViewModel")]
    public void DesignTimeStudylistViewModel_Constructor_SummaryCountsPositive()
    {
        var vm = new DesignTimeStudylistViewModel();
        vm.CompletedCount.Should().BeGreaterThan(0);
        vm.InProgressCount.Should().BeGreaterThan(0);
        vm.ReportedCount.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeStudylistViewModel")]
    public void DesignTimeStudylistViewModel_Constructor_IsLoadingFalse()
    {
        var vm = new DesignTimeStudylistViewModel();
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeStudylistViewModel")]
    public void DesignTimeStudylistViewModel_Constructor_ErrorMessageNull()
    {
        var vm = new DesignTimeStudylistViewModel();
        vm.ErrorMessage.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeStudylistViewModel")]
    public void DesignTimeStudylistViewModel_Commands_NotNull()
    {
        var vm = new DesignTimeStudylistViewModel();
        vm.NavigatePreviousCommand.Should().NotBeNull();
        vm.NavigateNextCommand.Should().NotBeNull();
        vm.FilterByPeriodCommand.Should().NotBeNull();
        vm.LoadStudiesCommand.Should().NotBeNull();
        vm.SelectStudyCommand.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeStudylistViewModel")]
    public void DesignTimeStudylistViewModel_Constructor_SelectedStudyNull()
    {
        var vm = new DesignTimeStudylistViewModel();
        vm.SelectedStudy.Should().BeNull();
    }

    // ====================================================================
    // DesignTimeWorkflowViewModel
    // ====================================================================

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeWorkflowViewModel")]
    public void DesignTimeWorkflowViewModel_Constructor_SetsSelectedPatient()
    {
        var vm = new DesignTimeWorkflowViewModel();
        vm.SelectedPatient.Should().NotBeNull();
        vm.SelectedPatient!.Name.Should().NotBeNullOrEmpty();
        vm.SelectedPatient.PatientId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeWorkflowViewModel")]
    public void DesignTimeWorkflowViewModel_Constructor_PopulatesThumbnails()
    {
        var vm = new DesignTimeWorkflowViewModel();
        vm.ThumbnailList.Should().NotBeEmpty();
        vm.ThumbnailList.Count.Should().Be(6);
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeWorkflowViewModel")]
    public void DesignTimeWorkflowViewModel_Constructor_DefaultsIsAcquiringFalse()
    {
        var vm = new DesignTimeWorkflowViewModel();
        vm.IsAcquiring.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeWorkflowViewModel")]
    public void DesignTimeWorkflowViewModel_Constructor_DefaultsStatusMessage()
    {
        var vm = new DesignTimeWorkflowViewModel();
        vm.StatusMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeWorkflowViewModel")]
    public void DesignTimeWorkflowViewModel_Constructor_DefaultsSelectedThumbnailIndexZero()
    {
        var vm = new DesignTimeWorkflowViewModel();
        vm.SelectedThumbnailIndex.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeWorkflowViewModel")]
    public void DesignTimeWorkflowViewModel_Constructor_AcquisitionTimeNotEmpty()
    {
        var vm = new DesignTimeWorkflowViewModel();
        vm.AcquisitionTime.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeWorkflowViewModel")]
    public void DesignTimeWorkflowViewModel_Commands_NotNull()
    {
        var vm = new DesignTimeWorkflowViewModel();
        vm.StartAcquisitionCommand.Should().NotBeNull();
        vm.StopAcquisitionCommand.Should().NotBeNull();
        vm.SaveCommand.Should().NotBeNull();
        vm.CancelCommand.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeWorkflowViewModel")]
    public void DesignTimeWorkflowViewModel_Patient_HasRequiredFields()
    {
        var vm = new DesignTimeWorkflowViewModel();
        var patient = vm.SelectedPatient!;
        patient.Name.Should().NotBeNullOrEmpty();
        patient.PatientId.Should().NotBeNullOrEmpty();
        patient.DateOfBirth.Should().NotBe(default);
        patient.Sex.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "DesignTime")]
    [Trait("ViewModel", "DesignTimeWorkflowViewModel")]
    public void DesignTimeWorkflowViewModel_Thumbnail_HasRequiredFields()
    {
        var vm = new DesignTimeWorkflowViewModel();
        var thumbnail = vm.ThumbnailList[0];
        thumbnail.Description.Should().NotBeNullOrEmpty();
        thumbnail.ImageCount.Should().BeGreaterThan(0);
    }
}
