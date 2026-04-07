using System.Collections.ObjectModel;
using System.Windows.Input;
using HnVue.Common.Models;

namespace HnVue.UI.Contracts.ViewModels;

// @MX:NOTE IMergeViewModel — "Sync Study" dialog contract. PPT 슬라이드 13.
//          Renamed from "Same Studylist" to "Sync Study".
//          Left: Patient A, Center: Preview + thumbnail strip, Right: Patient B.
/// <summary>Contract for the Sync Study (Merge) ViewModel. PPT 슬라이드 13.</summary>
public interface IMergeViewModel : IViewModelBase
{
    /// <summary>Gets or sets the search query text for Patient A.</summary>
    string SearchQueryA { get; set; }

    /// <summary>Gets or sets the search query text for Patient B.</summary>
    string SearchQueryB { get; set; }

    /// <summary>Gets the list of patients matching the Patient A search.</summary>
    ObservableCollection<PatientRecord> PatientsA { get; }

    /// <summary>Gets the list of patients matching the Patient B search.</summary>
    ObservableCollection<PatientRecord> PatientsB { get; }

    /// <summary>Gets or sets the selected patient from the Patient A list.</summary>
    PatientRecord? SelectedPatientA { get; set; }

    /// <summary>Gets or sets the selected patient from the Patient B list.</summary>
    PatientRecord? SelectedPatientB { get; set; }

    /// <summary>Gets the studies to display in the center preview for Patient A.</summary>
    ObservableCollection<StudyRecord> PreviewStudiesA { get; }

    /// <summary>Gets the studies to display in the center preview for Patient B.</summary>
    ObservableCollection<StudyRecord> PreviewStudiesB { get; }

    /// <summary>Gets or sets the study currently selected for large preview.</summary>
    StudyRecord? SelectedPreviewStudy { get; set; }

    /// <summary>Gets the command that searches patients for side A.</summary>
    ICommand SearchACommand { get; }

    /// <summary>Gets the command that searches patients for side B.</summary>
    ICommand SearchBCommand { get; }

    /// <summary>Gets the command that executes the merge (Sync Study) operation.</summary>
    ICommand MergeCommand { get; }

    /// <summary>Gets the command that cancels the dialog.</summary>
    ICommand CancelCommand { get; }

    /// <summary>Raised when the merge operation completes successfully.</summary>
    event EventHandler? MergeCompleted;

    /// <summary>Raised when the user cancels the dialog.</summary>
    event EventHandler? Cancelled;
}
