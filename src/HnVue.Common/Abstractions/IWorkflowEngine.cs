using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

/// <summary>
/// Controls and monitors the acquisition workflow state machine.
/// Implemented by the HnVue.Workflow module.
/// State transitions are validated against an allowed-transitions table to ensure
/// IEC 62304-compliant safety interlocks.
/// </summary>
public interface IWorkflowEngine
{
    /// <summary>Gets the current acquisition workflow state.</summary>
    WorkflowState CurrentState { get; }

    /// <summary>Gets the current system-wide safety state.</summary>
    SafeState CurrentSafeState { get; }

    /// <summary>
    /// Initiates a new acquisition session for the specified patient and study.
    /// Transitions from <see cref="WorkflowState.Idle"/> to <see cref="WorkflowState.PatientSelected"/>.
    /// </summary>
    /// <param name="patientId">Identifier of the patient for this session.</param>
    /// <param name="studyInstanceUid">DICOM Study Instance UID for the study being performed.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result> StartAsync(
        string patientId,
        string studyInstanceUid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests a transition to the specified target state.
    /// Returns a failure with <see cref="ErrorCode.InvalidStateTransition"/> when the
    /// transition is not allowed from the current state.
    /// </summary>
    /// <param name="targetState">The desired next workflow state.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result> TransitionAsync(
        WorkflowState targetState,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates exposure parameters against dose reference levels (DRL) and, when permitted,
    /// transitions from <see cref="WorkflowState.ReadyToExpose"/> to
    /// <see cref="WorkflowState.Exposing"/>.
    /// Implements SWR-WF-023~025 dose interlock (ALLOW / WARN / BLOCK / EMERGENCY).
    /// </summary>
    /// <param name="parameters">Technique factors for the planned exposure.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// Success with <see cref="DoseValidationResult"/> when the exposure is allowed (Allow or Warn).
    /// Failure when the dose check blocks the exposure.
    /// </returns>
    Task<Result<DoseValidationResult>> PrepareExposureAsync(
        ExposureParameters parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aborts the current workflow session and transitions to <see cref="WorkflowState.Error"/>.
    /// Safe to call from any state.
    /// </summary>
    /// <param name="reason">Human-readable reason for the abort (e.g., patient request or hardware fault).</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result> AbortAsync(
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Raised whenever the workflow state changes.
    /// Subscribers receive a <see cref="WorkflowStateChangedEventArgs"/> describing the transition.
    /// </summary>
    event EventHandler<WorkflowStateChangedEventArgs>? StateChanged;
}
