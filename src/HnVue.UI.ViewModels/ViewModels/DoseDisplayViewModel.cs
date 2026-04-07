using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.UI.Contracts.ViewModels;

namespace HnVue.UI.ViewModels;

// @MX:NOTE Diagnostic Reference Level (DRL) alerts when dose exceeds 150 mGy·cm² for chest exams (radiation safety)
/// <summary>
/// ViewModel for the dose display panel.
/// Shows current DAP, historical dose records, and alerts when the DRL reference level is exceeded.
/// </summary>
public sealed partial class DoseDisplayViewModel : ObservableObject, IDoseDisplayViewModel
{
    // Default DRL reference level in mGy·cm² for a general chest examination.
    private const double DefaultDrlReferenceLevel = 150.0;

    private readonly IDoseService _doseService;

    /// <summary>Initialises a new instance of <see cref="DoseDisplayViewModel"/>.</summary>
    /// <param name="doseService">Provides dose records and validation.</param>
    public DoseDisplayViewModel(IDoseService doseService)
    {
        _doseService = doseService;
    }

    /// <summary>
    /// Implements <see cref="IViewModelBase.IsLoading"/> by mapping to <see cref="IsRefreshing"/>.
    /// </summary>
    bool IViewModelBase.IsLoading => IsRefreshing;

    // Explicit IDoseDisplayViewModel ICommand bridge — see LoginViewModel for rationale.
    ICommand IDoseDisplayViewModel.RefreshCommand => RefreshCommand;

    /// <summary>Gets or sets the dose-area product (DAP) for the current exposure in mGy·cm².</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDoseAlert))]
    [NotifyPropertyChangedFor(nameof(DrlPercentage))]
    private double _currentDoseDap;

    /// <summary>Gets the collection of historical dose records for the current session.</summary>
    public ObservableCollection<DoseRecord> DoseHistory { get; } = new();

    /// <summary>
    /// Gets or sets the Diagnostic Reference Level (DRL) threshold in mGy·cm².
    /// A dose alert is raised when <see cref="CurrentDoseDap"/> exceeds this value.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDoseAlert))]
    [NotifyPropertyChangedFor(nameof(DrlPercentage))]
    private double _drlReferenceLevel = DefaultDrlReferenceLevel;

    /// <summary>Gets or sets a message describing the most recent error, or <see langword="null"/> on success.</summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>Gets or sets a value indicating whether a refresh is in progress.</summary>
    [ObservableProperty]
    private bool _isRefreshing;

    /// <summary>
    /// Gets a value indicating whether the current DAP exceeds the DRL reference level.
    /// </summary>
    public bool IsDoseAlert => CurrentDoseDap > DrlReferenceLevel;

    /// <summary>
    /// Gets the current dose as a percentage of the DRL reference level (0–100+).
    /// Used by <c>DoseDisplayView.xaml</c> to drive the DRL gauge bar width.
    /// Returns 0 when <see cref="DrlReferenceLevel"/> is zero to avoid division by zero.
    /// FR-DM-001 / FR-DM-015: thresholds 70%, 90%, 100%.
    /// </summary>
    public double DrlPercentage =>
        DrlReferenceLevel > 0 ? Math.Min(CurrentDoseDap / DrlReferenceLevel * 100.0, 100.0) : 0.0;

    /// <summary>Refreshes the dose history for the given study UID.</summary>
    /// <param name="studyInstanceUid">DICOM Study Instance UID to retrieve dose data for.</param>
    [RelayCommand]
    private async Task RefreshAsync(string? studyInstanceUid = null)
    {
        if (string.IsNullOrWhiteSpace(studyInstanceUid)) return;

        IsRefreshing = true;
        ErrorMessage = null;

        try
        {
            var result = await _doseService.GetDoseByStudyAsync(studyInstanceUid);

            if (result.IsSuccess && result.Value is not null)
            {
                DoseHistory.Clear();
                DoseHistory.Add(result.Value);
                CurrentDoseDap = result.Value.Dap;
            }
            else if (result.IsFailure)
            {
                ErrorMessage = result.ErrorMessage;
            }
        }
        finally
        {
            IsRefreshing = false;
        }
    }
}
