using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HnVue.Common.Models;

namespace HnVue.UI.Contracts.ViewModels;

// @MX:NOTE IStudylistViewModel — Study list contract. PPT 슬라이드 7 (Studylist 2안):
//          prev/next nav, PACS server dropdown, period filters.
/// <summary>Contract for the study list ViewModel. PPT 슬라이드 7 (Studylist 2안).</summary>
public interface IStudylistViewModel : IViewModelBase
{
    /// <summary>Gets the study records for the current patient/query.</summary>
    ObservableCollection<StudyRecord> Studies { get; }

    /// <summary>Gets or sets the currently selected study.</summary>
    StudyRecord? SelectedStudy { get; set; }

    /// <summary>Gets or sets the free-text search query.</summary>
    string SearchQuery { get; set; }

    /// <summary>Gets or sets the active period filter ("Today","3Days","1Week","All","1Month").</summary>
    string ActivePeriodFilter { get; set; }

    /// <summary>Gets the list of available PACS server names.</summary>
    IReadOnlyList<string> PacsServers { get; }

    /// <summary>Gets or sets the currently selected PACS server.</summary>
    string? SelectedPacsServer { get; set; }

    /// <summary>Gets the command that navigates to the previous patient/study set.</summary>
    ICommand NavigatePreviousCommand { get; }

    /// <summary>Gets the command that navigates to the next patient/study set.</summary>
    ICommand NavigateNextCommand { get; }

    /// <summary>Gets the command that filters studies by date period.</summary>
    ICommand FilterByPeriodCommand { get; }

    /// <summary>Gets the command that loads studies for the current patient from selected PACS.</summary>
    ICommand LoadStudiesCommand { get; }

    /// <summary>Gets the command that selects a study and proceeds to workflow.</summary>
    ICommand SelectStudyCommand { get; }
}
