using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.UI.Contracts.ViewModels;

namespace HnVue.UI.ViewModels;

/// <summary>
/// ViewModel for the dose monitor panel.
/// Displays dose records for the active study and the associated validation level.
/// </summary>
/// <remarks>SWR-UI-030: Dose monitor ViewModel surfaces IDoseService data for the operator.</remarks>
public sealed partial class DoseViewModel : ObservableObject, IDoseViewModel
{
    private readonly IDoseService _doseService;

    /// <summary>
    /// Initialises a new instance of <see cref="DoseViewModel"/>.
    /// </summary>
    /// <param name="doseService">Service for dose validation and retrieval operations.</param>
    public DoseViewModel(IDoseService doseService)
    {
        ArgumentNullException.ThrowIfNull(doseService);
        _doseService = doseService;
    }

    /// <summary>Gets or sets the dose record for the currently selected study, or <see langword="null"/> when none is loaded.</summary>
    [ObservableProperty]
    private DoseRecord? _currentDose;

    /// <summary>Gets or sets the dose validation level derived from the current dose record.</summary>
    [ObservableProperty]
    private DoseValidationLevel _validationLevel = DoseValidationLevel.Allow;

    /// <summary>Gets or sets the DICOM Study Instance UID used to query the dose record.</summary>
    [ObservableProperty]
    private string? _activeStudyInstanceUid;

    /// <summary>Gets or sets a value indicating whether a refresh operation is in progress.</summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>Gets or sets the error message displayed when a refresh fails.</summary>
    [ObservableProperty]
    private string? _errorMessage;

    // Explicit IDoseViewModel ICommand bridge — see LoginViewModel for rationale.
    ICommand IDoseViewModel.RefreshCommand => RefreshCommand;

    /// <summary>
    /// Refreshes the dose record for <see cref="ActiveStudyInstanceUid"/>.
    /// Clears <see cref="CurrentDose"/> when no record exists for the study.
    /// </summary>
    /// <remarks>SWR-UI-031: Refresh fetches latest dose data from IDoseService.</remarks>
    [RelayCommand]
    private async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ActiveStudyInstanceUid))
        {
            CurrentDose = null;
            ValidationLevel = DoseValidationLevel.Allow;
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var result = await _doseService.GetDoseByStudyAsync(
                ActiveStudyInstanceUid, cancellationToken);

            if (result.IsSuccess)
            {
                CurrentDose = result.Value;
            }
            else
            {
                CurrentDose = null;
                ErrorMessage = "선량 데이터를 불러오는 데 실패했습니다.";
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Loads the dose record for the specified study and updates the displayed values.
    /// </summary>
    /// <param name="studyInstanceUid">DICOM Study Instance UID to load.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <remarks>SWR-UI-032: LoadForStudy allows external components to drive the panel context.</remarks>
    public async Task LoadForStudyAsync(
        string studyInstanceUid,
        CancellationToken cancellationToken = default)
    {
        ActiveStudyInstanceUid = studyInstanceUid;
        await RefreshCommand.ExecuteAsync(cancellationToken);
    }
}
