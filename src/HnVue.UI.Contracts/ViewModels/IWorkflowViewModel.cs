using System.Collections.ObjectModel;
using System.Windows.Input;
using HnVue.Common.Enums;
using HnVue.Common.Models;

namespace HnVue.UI.Contracts.ViewModels;

/// <summary>Contract for the workflow control panel ViewModel.</summary>
public interface IWorkflowViewModel : IViewModelBase
{
    /// <summary>Gets the string representation of the current workflow state.</summary>
    string CurrentState { get; }

    /// <summary>Gets a value indicating whether the system is ready to accept an exposure trigger.</summary>
    bool IsExposureReady { get; }

    /// <summary>Gets the human-readable status message displayed in the control panel.</summary>
    string StatusMessage { get; }

    /// <summary>Gets the current system-wide safety state.</summary>
    SafeState CurrentSafeState { get; }

    /// <summary>Gets the localised display label for the current safety state.</summary>
    string SafeStateLabel { get; }

    /// <summary>Gets the file path of the current preview image for the acquisition preview panel.</summary>
    string? PreviewImagePath { get; }

    /// <summary>Gets the thumbnail strip items for the acquisition workflow.</summary>
    ObservableCollection<StudyRecord> ThumbnailList { get; }

    /// <summary>Gets or sets the currently selected patient for the patient info panel.</summary>
    PatientRecord? SelectedPatient { get; set; }

    /// <summary>Gets the command that transitions the system into exposure-ready state.</summary>
    ICommand PrepareExposureCommand { get; }

    /// <summary>Gets the command that triggers the actual exposure.</summary>
    ICommand TriggerExposureCommand { get; }

    /// <summary>Gets the command that aborts the current workflow step.</summary>
    ICommand AbortCommand { get; }
}
