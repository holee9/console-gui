using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.UI.Contracts.ViewModels;

namespace HnVue.UI.ViewModels;

// @MX:NOTE MergeViewModel — Sync Study dialog ViewModel. PPT 슬라이드 13.
//          Searches two independent patient lists (A and B) via IPatientService.
//          MergeAsync placeholder: real merge logic to be supplied by domain service.
/// <summary>
/// ViewModel for the Sync Study (Merge) dialog.
/// PPT 슬라이드 13: dual-patient search with center preview and thumbnail strip.
/// </summary>
public sealed partial class MergeViewModel : ObservableObject, IMergeViewModel
{
    private readonly IPatientService _patientService;

    /// <summary>Initialises a new instance of <see cref="MergeViewModel"/>.</summary>
    /// <param name="patientService">Service used to search patient records.</param>
    public MergeViewModel(IPatientService patientService) => _patientService = patientService;

    // IViewModelBase explicit mapping (source-generated properties are not virtual)
    bool IViewModelBase.IsLoading => IsLoading;
    ICommand IMergeViewModel.SearchACommand => SearchACommand;
    ICommand IMergeViewModel.SearchBCommand => SearchBCommand;
    ICommand IMergeViewModel.MergeCommand => MergeCommand;
    ICommand IMergeViewModel.CancelCommand => CancelCommand;

    [ObservableProperty] private string _searchQueryA = string.Empty;
    [ObservableProperty] private string _searchQueryB = string.Empty;
    [ObservableProperty] private PatientRecord? _selectedPatientA;
    [ObservableProperty] private PatientRecord? _selectedPatientB;
    [ObservableProperty] private StudyRecord? _selectedPreviewStudy;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _errorMessage;

    /// <inheritdoc/>
    public ObservableCollection<PatientRecord> PatientsA { get; } = new();

    /// <inheritdoc/>
    public ObservableCollection<PatientRecord> PatientsB { get; } = new();

    /// <inheritdoc/>
    public ObservableCollection<StudyRecord> PreviewStudiesA { get; } = new();

    /// <inheritdoc/>
    public ObservableCollection<StudyRecord> PreviewStudiesB { get; } = new();

    /// <inheritdoc/>
    public event EventHandler? MergeCompleted;

    /// <inheritdoc/>
    public event EventHandler? Cancelled;

    [RelayCommand]
    private async Task SearchAAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var result = await _patientService.SearchAsync(SearchQueryA, cancellationToken);
            PatientsA.Clear();
            if (result.IsSuccess)
                foreach (var p in result.Value)
                    PatientsA.Add(p);
            else
                ErrorMessage = result.ErrorMessage;
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task SearchBAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var result = await _patientService.SearchAsync(SearchQueryB, cancellationToken);
            PatientsB.Clear();
            if (result.IsSuccess)
                foreach (var p in result.Value)
                    PatientsB.Add(p);
            else
                ErrorMessage = result.ErrorMessage;
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task MergeAsync()
    {
        if (SelectedPatientA is null || SelectedPatientB is null)
        {
            ErrorMessage = "Please select a patient on both sides before syncing.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        try
        {
            // TODO: replace with actual IStudyMergeService call when available
            await Task.Delay(1);
            MergeCompleted?.Invoke(this, EventArgs.Empty);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke(this, EventArgs.Empty);
}
