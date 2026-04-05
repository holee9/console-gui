using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Security;

namespace HnVue.Workflow;

/// <summary>
/// Implements the acquisition workflow state machine for the HnVue X-ray console.
/// Enforces IEC 62304 §5.3.6 state-transition safety rules through
/// <see cref="WorkflowStateMachine"/>.
/// </summary>
/// <remarks>
/// The engine coordinates the generator, FPD detector, and dose service to
/// orchestrate safe radiation exposures. All state transitions are validated
/// before execution. The abort path is always available from any state.
/// </remarks>
public sealed class WorkflowEngine : IWorkflowEngine
{
    private readonly WorkflowStateMachine _stateMachine = new();
    private readonly IDoseService _doseService;
    private readonly IGeneratorInterface _generator;
    private readonly ISecurityContext _securityContext;
    private readonly object _lock = new();

    private SafeState _safeState = SafeState.Idle;
    private string? _currentPatientId;
    private string? _currentStudyInstanceUid;

    /// <summary>
    /// Initialises a new <see cref="WorkflowEngine"/> with required dependencies.
    /// </summary>
    /// <param name="doseService">Dose validation service for pre-exposure checks.</param>
    /// <param name="generator">Generator hardware interface (real or simulated).</param>
    /// <param name="securityContext">Security context for RBAC enforcement.</param>
    public WorkflowEngine(IDoseService doseService, IGeneratorInterface generator, ISecurityContext securityContext)
    {
        _doseService = doseService ?? throw new ArgumentNullException(nameof(doseService));
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _securityContext = securityContext ?? throw new ArgumentNullException(nameof(securityContext));
    }

    /// <inheritdoc/>
    public WorkflowState CurrentState
    {
        get
        {
            lock (_lock)
                return _stateMachine.CurrentState;
        }
    }

    /// <inheritdoc/>
    public SafeState CurrentSafeState
    {
        get
        {
            lock (_lock)
                return _safeState;
        }
    }

    /// <inheritdoc/>
    public event EventHandler<WorkflowStateChangedEventArgs>? StateChanged;

    /// <inheritdoc/>
    public async Task<Result> StartAsync(
        string patientId,
        string studyInstanceUid,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(patientId);
        ArgumentNullException.ThrowIfNull(studyInstanceUid);

        WorkflowState previous;
        Result transitionResult;

        lock (_lock)
        {
            if (_safeState == SafeState.Emergency || _safeState == SafeState.Blocked)
                return Result.Failure(ErrorCode.InvalidStateTransition,
                    $"Cannot start workflow: system is in safe state '{_safeState}'.");

            previous = _stateMachine.CurrentState;
            transitionResult = _stateMachine.TryTransition(WorkflowState.PatientSelected);

            if (transitionResult.IsSuccess)
            {
                _currentPatientId = patientId;
                _currentStudyInstanceUid = studyInstanceUid;
            }
        }

        if (transitionResult.IsSuccess)
            RaiseStateChanged(previous, WorkflowState.PatientSelected);

        return await Task.FromResult(transitionResult).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result> TransitionAsync(
        WorkflowState targetState,
        CancellationToken cancellationToken = default)
    {
        // SWR-IP-RBAC-001: Enforce RBAC before radiation exposure.
        if (targetState == WorkflowState.Exposing)
        {
            if (!_securityContext.CurrentRole.HasValue)
                return Result.Failure(ErrorCode.AuthenticationFailed,
                    "User must be authenticated to perform exposure.");

            var rbacResult = RbacPolicy.Check(_securityContext.CurrentRole.Value, Permissions.PerformExposure);
            if (rbacResult.IsFailure)
                return rbacResult;
        }

        WorkflowState previous;
        Result transitionResult;

        lock (_lock)
        {
            if (_safeState == SafeState.Emergency)
                return Result.Failure(ErrorCode.InvalidStateTransition,
                    "System is in EMERGENCY safe state. Manual intervention required.");

            previous = _stateMachine.CurrentState;
            transitionResult = _stateMachine.TryTransition(targetState);
        }

        if (transitionResult.IsSuccess)
            RaiseStateChanged(previous, targetState);

        return await Task.FromResult(transitionResult).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result> AbortAsync(
        string reason,
        CancellationToken cancellationToken = default)
    {
        WorkflowState previous;

        lock (_lock)
        {
            previous = _stateMachine.CurrentState;

            if (previous == WorkflowState.Error)
                return Result.Success(); // Already in error state

            _stateMachine.ForceError();
            _currentPatientId = null;
            _currentStudyInstanceUid = null;
        }

        // Attempt to abort generator if it is active
        if (previous is WorkflowState.Exposing or WorkflowState.ReadyToExpose)
        {
            try
            {
                await _generator.AbortAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                // Escalate to emergency if generator abort fails
                lock (_lock)
                    _safeState = SafeState.Emergency;
            }
        }

        RaiseStateChanged(previous, WorkflowState.Error, reason);
        return Result.Success();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void RaiseStateChanged(WorkflowState previous, WorkflowState next, string? reason = null)
    {
        var args = new WorkflowStateChangedEventArgs(previous, next, reason);
        StateChanged?.Invoke(this, args);
    }
}
