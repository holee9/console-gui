using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.UI.Contracts.Models;
using HnVue.UI.Contracts.ViewModels;

namespace HnVue.UI.ViewModels;

// @MX:NOTE Role-based exposure control: only Radiographer/Radiologist roles can trigger exposure (safety-critical)
/// <summary>
/// ViewModel for the acquisition workflow control panel.
/// Manages workflow state transitions and enforces role-based exposure permissions.
/// Only users with the <see cref="UserRole.Radiographer"/> or <see cref="UserRole.Radiologist"/> role
/// may trigger an exposure.
/// </summary>
public sealed partial class WorkflowViewModel : ObservableObject, IWorkflowViewModel, IDisposable
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly ISecurityContext _securityContext;

    /// <summary>Initialises a new instance of <see cref="WorkflowViewModel"/>.</summary>
    /// <param name="workflowEngine">Manages the acquisition workflow state machine.</param>
    /// <param name="securityContext">Provides the current user's role.</param>
    public WorkflowViewModel(IWorkflowEngine workflowEngine, ISecurityContext securityContext)
    {
        _workflowEngine = workflowEngine;
        _securityContext = securityContext;
        _workflowEngine.StateChanged += OnWorkflowStateChanged;

        // Initialise state from engine.
        CurrentState = _workflowEngine.CurrentState.ToString();
        WorkflowState = _workflowEngine.CurrentState;
        UpdateDerivedProperties();
        UpdateSafeStateDisplay();
    }

    /// <summary>
    /// Gets a value indicating whether an operation is in progress.
    /// The workflow panel does not expose a generic loading indicator; this always returns <see langword="false"/>.
    /// </summary>
    public bool IsLoading => false;

    /// <summary>
    /// Gets the current error message.
    /// Workflow errors are surfaced via <see cref="StatusMessage"/> instead; this always returns <see langword="null"/>.
    /// </summary>
    public string? ErrorMessage => null;

    // Explicit IWorkflowViewModel ICommand bridge — see LoginViewModel for rationale.
    ICommand IWorkflowViewModel.PrepareExposureCommand => PrepareExposureCommand;
    ICommand IWorkflowViewModel.TriggerExposureCommand => TriggerExposureCommand;
    ICommand IWorkflowViewModel.AbortCommand => AbortCommand;

    /// <summary>Gets or sets the string representation of the current workflow state.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TriggerExposureCommand))]
    private string _currentState = WorkflowState.Idle.ToString();

    /// <summary>
    /// Gets or sets the current workflow state as the strongly-typed enum.
    /// Used by XAML DataTriggers for state-dependent visual changes.
    /// </summary>
    [ObservableProperty]
    private WorkflowState _workflowState = WorkflowState.Idle;

    /// <summary>Gets or sets a value indicating whether the system is ready to expose.</summary>
    [ObservableProperty]
    private bool _isExposureReady;

    /// <summary>Gets or sets a human-readable status message for the operator.</summary>
    [ObservableProperty]
    private string _statusMessage = "System idle. Select a patient to begin.";

    /// <summary>
    /// Gets or sets the current system-wide safety state.
    /// Bound to the SafeState indicator bar in the view for colour-coded status display.
    /// SWR-NF-SC-041 / Issue #31.
    /// </summary>
    [ObservableProperty]
    private SafeState _currentSafeState = SafeState.Idle;

    /// <summary>
    /// Gets or sets the display label for the current safety state shown in the indicator bar.
    /// </summary>
    [ObservableProperty]
    private string _safeStateLabel = "IDLE";

    /// <summary>Gets or sets the file path of the current preview image for the acquisition preview panel.</summary>
    [ObservableProperty]
    private string? _previewImagePath;

    /// <summary>
    /// Gets or sets the WPF image source for the acquisition preview.
    /// Not in <see cref="IWorkflowViewModel"/> because BitmapSource is a WPF-specific type;
    /// the View binds to this via DataContext (see IImageViewerViewModel pattern).
    /// </summary>
    [ObservableProperty]
    private BitmapSource? _previewImage;

    /// <summary>Gets or sets the currently selected patient for the patient info panel.</summary>
    [ObservableProperty]
    private PatientRecord? _selectedPatient;

    /// <summary>Gets the thumbnail strip items for the acquisition workflow.</summary>
    public ObservableCollection<StudyItem> ThumbnailList { get; } = new();

    /// <summary>
    /// Prepares the generator and detector for an exposure.
    /// Transitions the workflow to <see cref="WorkflowState.ReadyToExpose"/>.
    /// </summary>
    [RelayCommand]
    private async Task PrepareExposureAsync()
    {
        StatusMessage = "Preparing exposure\u2026";
        var result = await _workflowEngine.TransitionAsync(WorkflowState.ReadyToExpose);
        if (result.IsFailure)
        {
            StatusMessage = $"Prepare failed: {result.ErrorMessage}";
        }
    }

    /// <summary>
    /// Triggers the X-ray exposure.
    /// Only users with <see cref="UserRole.Radiographer"/> or <see cref="UserRole.Radiologist"/> role may call this.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanTriggerExposure))]
    private async Task TriggerExposureAsync()
    {
        StatusMessage = "Exposing\u2026";
        var result = await _workflowEngine.TransitionAsync(WorkflowState.Exposing);
        if (result.IsFailure)
        {
            StatusMessage = $"Exposure failed: {result.ErrorMessage}";
        }
    }

    private bool CanTriggerExposure() =>
        IsExposureReady &&
        (_securityContext.HasRole(UserRole.Radiographer) || _securityContext.HasRole(UserRole.Radiologist));

    /// <summary>Aborts the current workflow session.</summary>
    [RelayCommand]
    private async Task AbortAsync()
    {
        StatusMessage = "Aborting workflow\u2026";
        var result = await _workflowEngine.AbortAsync("Operator abort.");
        if (result.IsFailure)
        {
            StatusMessage = $"Abort failed: {result.ErrorMessage}";
        }
    }

    private void OnWorkflowStateChanged(object? sender, WorkflowStateChangedEventArgs e)
    {
        CurrentState = e.NewState.ToString();
        WorkflowState = e.NewState;
        UpdateDerivedProperties();
        UpdateSafeStateDisplay();

        if (!string.IsNullOrEmpty(e.Reason))
        {
            StatusMessage = e.Reason;
        }
    }

    private void UpdateSafeStateDisplay()
    {
        CurrentSafeState = _workflowEngine.CurrentSafeState;
        SafeStateLabel = CurrentSafeState switch
        {
            SafeState.Idle => "IDLE",
            SafeState.Warning => "WARNING",
            SafeState.Degraded => "DEGRADED",
            SafeState.Blocked => "BLOCKED",
            SafeState.Emergency => "EMERGENCY",
            _ => CurrentSafeState.ToString().ToUpperInvariant()
        };
    }

    private void UpdateDerivedProperties()
    {
        var engineState = _workflowEngine.CurrentState;
        IsExposureReady = engineState == WorkflowState.ReadyToExpose;
        TriggerExposureCommand.NotifyCanExecuteChanged();

        StatusMessage = engineState switch
        {
            WorkflowState.Idle => "System idle. Select a patient to begin.",
            WorkflowState.PatientSelected => "Patient selected. Load a protocol.",
            WorkflowState.ProtocolLoaded => "Protocol loaded. Prepare for exposure.",
            WorkflowState.ReadyToExpose => "Ready to expose. Trigger when clear.",
            WorkflowState.Exposing => "Exposure in progress\u2026",
            WorkflowState.ImageAcquiring => "Acquiring image\u2026",
            WorkflowState.ImageProcessing => "Processing image\u2026",
            WorkflowState.ImageReview => "Image ready for review.",
            WorkflowState.Completed => "Session complete.",
            WorkflowState.Error => "Error state. Manual intervention required.",
            _ => StatusMessage
        };
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _workflowEngine.StateChanged -= OnWorkflowStateChanged;
    }
}
