using HnVue.Common.Enums;
using HnVue.Common.Results;

namespace HnVue.Workflow;

/// <summary>
/// Encodes the allowed state transition table for the acquisition workflow.
/// Enforces IEC 62304 §5.3.6 state machine safety — only valid transitions are permitted.
/// </summary>
/// <remarks>
/// The 9-state machine follows this directed graph:
///   Idle → PatientSelected → ProtocolLoaded → ReadyToExpose → Exposing
///     → ImageAcquiring → ImageProcessing → ImageReview → Completed
///   Any state → Error (abort path)
///   Error → Idle (reset path)
/// </remarks>
public sealed class WorkflowStateMachine
{
    // ── Allowed transitions table ─────────────────────────────────────────────
    // @MX:NOTE IEC 62304 §5.3.6 state machine safety - only valid transitions permitted, abort path always available

    private static readonly Dictionary<WorkflowState, IReadOnlySet<WorkflowState>> _allowedTransitions
        = new Dictionary<WorkflowState, IReadOnlySet<WorkflowState>>
        {
            [WorkflowState.Idle] = new HashSet<WorkflowState>
            {
                WorkflowState.PatientSelected,
                WorkflowState.Error,
            },
            [WorkflowState.PatientSelected] = new HashSet<WorkflowState>
            {
                WorkflowState.ProtocolLoaded,
                WorkflowState.Idle,   // Deselect patient → back to Idle
                WorkflowState.Error,
            },
            [WorkflowState.ProtocolLoaded] = new HashSet<WorkflowState>
            {
                WorkflowState.ReadyToExpose,
                WorkflowState.PatientSelected, // Protocol change
                WorkflowState.Error,
            },
            [WorkflowState.ReadyToExpose] = new HashSet<WorkflowState>
            {
                WorkflowState.Exposing,
                WorkflowState.ProtocolLoaded, // Unarm / parameter change
                WorkflowState.Error,
            },
            [WorkflowState.Exposing] = new HashSet<WorkflowState>
            {
                WorkflowState.ImageAcquiring,
                WorkflowState.Error,
            },
            [WorkflowState.ImageAcquiring] = new HashSet<WorkflowState>
            {
                WorkflowState.ImageProcessing,
                WorkflowState.Error,
            },
            [WorkflowState.ImageProcessing] = new HashSet<WorkflowState>
            {
                WorkflowState.ImageReview,
                WorkflowState.Error,
            },
            [WorkflowState.ImageReview] = new HashSet<WorkflowState>
            {
                WorkflowState.Completed,
                WorkflowState.ReadyToExpose, // Retake
                WorkflowState.Error,
            },
            [WorkflowState.Completed] = new HashSet<WorkflowState>
            {
                WorkflowState.Idle,  // Begin new session
                WorkflowState.Error,
            },
            [WorkflowState.Error] = new HashSet<WorkflowState>
            {
                WorkflowState.Idle, // Manual reset after error
            },
        };

    private WorkflowState _currentState = WorkflowState.Idle;

    /// <summary>Gets the current workflow state.</summary>
    public WorkflowState CurrentState => _currentState;

    // @MX:ANCHOR TryTransition - @MX:REASON: Core state machine transition, called by WorkflowEngine (3 paths), enforces IEC 62304 §5.3.6 safety
    /// <summary>
    /// Attempts to transition to <paramref name="targetState"/>.
    /// </summary>
    /// <returns>
    /// Success if the transition is allowed; failure with
    /// <see cref="ErrorCode.InvalidStateTransition"/> otherwise.
    /// </returns>
    public Result TryTransition(WorkflowState targetState)
    {
        if (_allowedTransitions.TryGetValue(_currentState, out var allowed) && allowed.Contains(targetState))
        {
            _currentState = targetState;
            return Result.Success();
        }

        return Result.Failure(
            ErrorCode.InvalidStateTransition,
            $"Transition from '{_currentState}' to '{targetState}' is not allowed.");
    }

    /// <summary>
    /// Forces the state to <see cref="WorkflowState.Error"/> regardless of the current state.
    /// Used by the abort path — always succeeds.
    /// </summary>
    // @MX:WARN ForceError - @MX:REASON: Safety-critical abort path, bypasses state validation, only callable from WorkflowEngine.AbortAsync
    public void ForceError()
    {
        _currentState = WorkflowState.Error;
    }

    /// <summary>
    /// Forces the state to <see cref="WorkflowState.Exposing"/> regardless of the current state.
    /// Used exclusively by the emergency fast-path (SWR-WF-026~027) which bypasses normal
    /// state transitions for trauma care scenarios.
    /// </summary>
    // @MX:WARN ForceExposing - @MX:REASON: Emergency fast-path only, bypasses state validation, only callable from WorkflowEngine.StartEmergencyExposureAsync
    public void ForceExposing()
    {
        _currentState = WorkflowState.Exposing;
    }

    /// <summary>
    /// Resets the state machine to <see cref="WorkflowState.Idle"/>.
    /// Only valid from <see cref="WorkflowState.Error"/> or <see cref="WorkflowState.Completed"/>.
    /// </summary>
    public Result Reset()
    {
        if (_currentState is WorkflowState.Error or WorkflowState.Completed)
        {
            _currentState = WorkflowState.Idle;
            return Result.Success();
        }
        return Result.Failure(
            ErrorCode.InvalidStateTransition,
            $"Cannot reset from state '{_currentState}'. Must be in Error or Completed.");
    }

    /// <summary>
    /// Returns all valid target states reachable from the current state.
    /// </summary>
    public IReadOnlySet<WorkflowState> GetAllowedTransitions()
    {
        return _allowedTransitions.TryGetValue(_currentState, out var allowed)
            ? allowed
            : new HashSet<WorkflowState>();
    }

    /// <summary>
    /// Checks whether transitioning to <paramref name="targetState"/> is currently valid
    /// without performing the transition.
    /// </summary>
    public bool CanTransitionTo(WorkflowState targetState)
    {
        return _allowedTransitions.TryGetValue(_currentState, out var allowed)
               && allowed.Contains(targetState);
    }
}
