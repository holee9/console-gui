using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;

namespace HnVue.UI.ViewModels;

/// <summary>
/// ViewModel for the acquisition workflow control panel.
/// Manages workflow state transitions and enforces role-based exposure permissions.
/// Only users with the <see cref="UserRole.Radiographer"/> or <see cref="UserRole.Radiologist"/> role
/// may trigger an exposure.
/// </summary>
public sealed partial class WorkflowViewModel : ObservableObject, IDisposable
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
        UpdateDerivedProperties();
    }

    /// <summary>Gets or sets the string representation of the current workflow state.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TriggerExposureCommand))]
    private string _currentState = WorkflowState.Idle.ToString();

    /// <summary>Gets or sets a value indicating whether the system is ready to expose.</summary>
    [ObservableProperty]
    private bool _isExposureReady;

    /// <summary>Gets or sets a human-readable status message for the operator.</summary>
    [ObservableProperty]
    private string _statusMessage = "System idle. Select a patient to begin.";

    /// <summary>
    /// Prepares the generator and detector for an exposure.
    /// Transitions the workflow to <see cref="WorkflowState.ReadyToExpose"/>.
    /// </summary>
    [RelayCommand]
    private async Task PrepareExposureAsync()
    {
        StatusMessage = "Preparing exposure…";
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
        StatusMessage = "Exposing…";
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
        StatusMessage = "Aborting workflow…";
        var result = await _workflowEngine.AbortAsync("Operator abort.");
        if (result.IsFailure)
        {
            StatusMessage = $"Abort failed: {result.ErrorMessage}";
        }
    }

    private void OnWorkflowStateChanged(object? sender, WorkflowStateChangedEventArgs e)
    {
        CurrentState = e.NewState.ToString();
        UpdateDerivedProperties();

        if (!string.IsNullOrEmpty(e.Reason))
        {
            StatusMessage = e.Reason;
        }
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
            WorkflowState.Exposing => "Exposure in progress…",
            WorkflowState.ImageAcquiring => "Acquiring image…",
            WorkflowState.ImageProcessing => "Processing image…",
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
