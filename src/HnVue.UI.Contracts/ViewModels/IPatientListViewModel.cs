using System.Collections.ObjectModel;
using System.Windows.Input;
using HnVue.Common.Models;

namespace HnVue.UI.Contracts.ViewModels;

/// <summary>Contract for the patient list ViewModel.</summary>
public interface IPatientListViewModel : IViewModelBase
{
    /// <summary>Gets or sets the current search query string.</summary>
    string SearchQuery { get; set; }

    /// <summary>Gets the observable collection of matching patient records.</summary>
    ObservableCollection<PatientRecord> Patients { get; }

    /// <summary>Gets or sets the currently selected patient.</summary>
    PatientRecord? SelectedPatient { get; set; }

    /// <summary>Gets the command that executes a patient search.</summary>
    ICommand SearchCommand { get; }

    /// <summary>Gets the command that selects a patient and proceeds to workflow.</summary>
    ICommand SelectPatientCommand { get; }

    /// <summary>Gets the command that opens the patient registration form.</summary>
    ICommand RegisterPatientCommand { get; }

    /// <summary>Gets the command that filters patients by date period ("Today","3Days","1Week","All","1Month").</summary>
    ICommand FilterByPeriodCommand { get; }

    /// <summary>Gets or sets the active period filter ("Today","3Days","1Week","All","1Month").</summary>
    string ActivePeriodFilter { get; set; }
}
