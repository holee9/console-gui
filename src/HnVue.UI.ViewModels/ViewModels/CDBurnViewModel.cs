using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Abstractions;
using HnVue.UI.Contracts.ViewModels;

namespace HnVue.UI.ViewModels;

/// <summary>
/// ViewModel for the CD/DVD burn screen.
/// Allows the operator to burn a DICOM study to an optical disc.
/// </summary>
public sealed partial class CDBurnViewModel : ObservableObject, ICDBurnViewModel, IDisposable
{
    private readonly ICDDVDBurnService _burnService;
    // @MX:NOTE CancellationTokenSource pattern for long-running async operations; Dispose() cleans up
    private CancellationTokenSource? _burnCts;

    /// <summary>Initialises a new instance of <see cref="CDBurnViewModel"/>.</summary>
    /// <param name="burnService">Service that performs the optical disc burn operation.</param>
    public CDBurnViewModel(ICDDVDBurnService burnService)
    {
        _burnService = burnService;
    }

    /// <summary>
    /// Implements <see cref="IViewModelBase.IsLoading"/> by mapping to <see cref="IsBurning"/>.
    /// </summary>
    bool IViewModelBase.IsLoading => IsBurning;

    // Explicit ICDBurnViewModel ICommand bridge — see LoginViewModel for rationale.
    ICommand ICDBurnViewModel.StartBurnCommand => StartBurnCommand;
    ICommand ICDBurnViewModel.CancelBurnCommand => CancelBurnCommand;

    /// <summary>Gets or sets the DICOM Study Instance UID of the study to burn.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartBurnCommand))]
    private string? _selectedStudyId;

    /// <summary>Gets or sets a value indicating whether a burn operation is currently running.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartBurnCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelBurnCommand))]
    private bool _isBurning;

    /// <summary>Gets or sets the burn progress as a percentage (0–100).</summary>
    [ObservableProperty]
    private int _burnProgress;

    /// <summary>Gets or sets a human-readable status message for the operator.</summary>
    [ObservableProperty]
    private string _statusMessage = "Select a study and insert a blank disc.";

    /// <summary>Gets or sets a message describing the most recent error, or <see langword="null"/> on success.</summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// Starts burning the selected study to the inserted disc.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStartBurn))]
    private async Task StartBurnAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedStudyId)) return;

        IsBurning = true;
        BurnProgress = 0;
        ErrorMessage = null;
        StatusMessage = "Burning disc\u2026";
        _burnCts = new CancellationTokenSource();

        try
        {
            var label = $"STUDY_{SelectedStudyId[..Math.Min(16, SelectedStudyId.Length)]}";
            var result = await _burnService.BurnStudyAsync(SelectedStudyId, label, _burnCts.Token);

            if (result.IsSuccess)
            {
                BurnProgress = 100;
                StatusMessage = "Burn completed successfully.";
            }
            else
            {
                ErrorMessage = result.ErrorMessage;
                StatusMessage = "Burn failed.";
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Burn cancelled.";
            BurnProgress = 0;
        }
        finally
        {
            IsBurning = false;
            _burnCts?.Dispose();
            _burnCts = null;
        }
    }

    private bool CanStartBurn() =>
        !string.IsNullOrWhiteSpace(SelectedStudyId) && !IsBurning;

    /// <summary>Cancels the running burn operation.</summary>
    [RelayCommand(CanExecute = nameof(CanCancelBurn))]
    private void CancelBurn()
    {
        _burnCts?.Cancel();
    }

    private bool CanCancelBurn() => IsBurning;

    /// <inheritdoc/>
    public void Dispose()
    {
        _burnCts?.Dispose();
        _burnCts = null;
    }
}
