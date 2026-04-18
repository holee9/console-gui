using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.UI.Contracts.ViewModels;

namespace HnVue.UI.ViewModels;

// @MX:NOTE StudylistViewModel — Study list with PACS server selection and period filters.
//          PPT 슬라이드 7 (Studylist 2안): prev/next navigation, PACS dropdown, period filters.
/// <summary>
/// ViewModel for the study list panel.
/// PPT 슬라이드 7 (Studylist 2안): prev/next nav, PACS server dropdown, period filters.
/// </summary>
public sealed partial class StudylistViewModel : ObservableObject, IStudylistViewModel
{
    private readonly IStudyRepository _studyRepository;

    /// <summary>Initialises a new instance of <see cref="StudylistViewModel"/>.</summary>
    /// <param name="studyRepository">Repository used to query study records.</param>
    public StudylistViewModel(IStudyRepository studyRepository)
    {
        _studyRepository = studyRepository;
        _selectedPacsServer = _pacsServers[0];
    }

    // Explicit IViewModelBase bridge
    bool IViewModelBase.IsLoading => IsLoading;

    // Explicit IStudylistViewModel ICommand bridge (matches PatientListViewModel pattern)
    ICommand IStudylistViewModel.NavigatePreviousCommand => NavigatePreviousCommand;
    ICommand IStudylistViewModel.NavigateNextCommand => NavigateNextCommand;
    ICommand IStudylistViewModel.FilterByPeriodCommand => FilterByPeriodCommand;
    ICommand IStudylistViewModel.LoadStudiesCommand => LoadStudiesCommand;
    ICommand IStudylistViewModel.SelectStudyCommand => SelectStudyCommand;

    /// <summary>Gets the collection of study records matching the current query.</summary>
    public ObservableCollection<StudyRecord> Studies { get; } = new();

    /// <summary>Gets or sets the study record currently selected in the list.</summary>
    [ObservableProperty]
    private StudyRecord? _selectedStudy;

    /// <summary>Gets or sets the free-text query used to filter studies.</summary>
    [ObservableProperty]
    private string _searchQuery = string.Empty;

    /// <summary>Gets or sets the active period filter key ("Today","3Days","1Week","All","1Month").</summary>
    [ObservableProperty]
    private string _activePeriodFilter = "All";

    /// <summary>Gets or sets the currently selected PACS server name.</summary>
    [ObservableProperty]
    private string? _selectedPacsServer;

    /// <summary>Gets or sets a value indicating whether a load operation is in progress.</summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>Gets or sets a message describing the most recent error, or <see langword="null"/> on success.</summary>
    [ObservableProperty]
    private string? _errorMessage;

    private readonly List<string> _pacsServers = new() { "LOCAL", "PACS-01", "PACS-02" };

    /// <summary>Gets the list of available PACS server names.</summary>
    public IReadOnlyList<string> PacsServers => _pacsServers;

    /// <summary>Navigates to the previous page of studies.</summary>
    /// <remarks>
    /// @MX:TODO Implement paged navigation backed by IStudyRepository paging API (SWR-UI-SL-006).
    ///          Paging API is pending PACS/C-FIND cursor spec — blocked until Team A exposes
    ///          <c>GetStudiesAsync(offset, limit)</c> or equivalent cursor. Command remains wired
    ///          so XAML bindings do not throw; it is a no-op until the repository contract ships.
    /// </remarks>
    [RelayCommand]
    private static void NavigatePrevious()
    {
        // Intentional no-op placeholder — see XML doc above for @MX:TODO.
    }

    /// <summary>Navigates to the next page of studies.</summary>
    /// <remarks>
    /// @MX:TODO Implement paged navigation backed by IStudyRepository paging API (SWR-UI-SL-006).
    ///          Paired with <see cref="NavigatePrevious"/> — both block on the same repository work.
    /// </remarks>
    [RelayCommand]
    private static void NavigateNext()
    {
        // Intentional no-op placeholder — see XML doc above for @MX:TODO.
    }

    /// <summary>Applies the given period filter and reloads studies.</summary>
    [RelayCommand]
    private async Task FilterByPeriodAsync(string? period)
    {
        ActivePeriodFilter = period ?? "All";
        await LoadStudiesAsync();
    }

    /// <summary>Loads studies from the selected PACS server filtered by active period.</summary>
    [RelayCommand]
    private async Task LoadStudiesAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            // @MX:TODO Replace the placeholder delay with a PACS/C-FIND query scoped to
            //          <see cref="SelectedPacsServer"/> and <see cref="ActivePeriodFilter"/>.
            //          IStudyRepository currently only exposes local SQLite queries;
            //          PACS integration is tracked under SWR-UI-SL-009 (blocked pending Team B
            //          DICOM C-FIND SCU surface). Current behavior keeps UI responsive for tests.
            await Task.Delay(1);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Sets the selected study record.</summary>
    [RelayCommand]
    private void SelectStudy(StudyRecord? study)
    {
        SelectedStudy = study;
    }
}
