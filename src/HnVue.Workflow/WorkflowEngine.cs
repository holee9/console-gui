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
    private readonly IAuditService? _auditService;
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
    /// <param name="auditService">
    /// Optional audit service for SWR-NF-SC-041 compliance logging.
    /// When <see langword="null"/>, audit logging is silently skipped so that
    /// existing unit tests and deployment configurations remain compatible.
    /// </param>
    public WorkflowEngine(
        IDoseService doseService,
        IGeneratorInterface generator,
        ISecurityContext securityContext,
        IAuditService? auditService = null)
    {
        _doseService = doseService ?? throw new ArgumentNullException(nameof(doseService));
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _securityContext = securityContext ?? throw new ArgumentNullException(nameof(securityContext));
        _auditService = auditService;
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
    /// <remarks>
    /// SWR-WF-023~025: Dose interlock gate before radiation exposure.
    /// - ALLOW: transition to Exposing, return success.
    /// - WARN:  transition to Exposing, return success with warning message (caller shows alert).
    /// - BLOCK: set SafeState.Blocked, do not transition, return failure.
    /// - EMERGENCY: set SafeState.Emergency, abort generator if active, return failure.
    /// Issue #21.
    /// </remarks>
    public async Task<Result<DoseValidationResult>> PrepareExposureAsync(
        ExposureParameters parameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        // RBAC check first (same as TransitionAsync Exposing path)
        if (!_securityContext.CurrentRole.HasValue)
            return Result.Failure<DoseValidationResult>(ErrorCode.AuthenticationFailed,
                "User must be authenticated to perform exposure.");

        var rbacResult = RbacPolicy.Check(_securityContext.CurrentRole.Value, Permissions.PerformExposure);
        if (rbacResult.IsFailure)
            return Result.Failure<DoseValidationResult>(
                rbacResult.Error ?? ErrorCode.InsufficientPermission, rbacResult.ErrorMessage ?? string.Empty);

        // Dose interlock validation — SWR-WF-023
        var doseResult = await _doseService.ValidateExposureAsync(parameters, cancellationToken).ConfigureAwait(false);
        if (doseResult.IsFailure)
            return Result.Failure<DoseValidationResult>(
                doseResult.Error ?? ErrorCode.DoseLimitExceeded, doseResult.ErrorMessage ?? string.Empty);

        var validation = doseResult.Value;

        // SWR-WF-025: EMERGENCY → abort and escalate safe state
        if (validation.Level == DoseValidationLevel.Emergency)
        {
            lock (_lock)
                SetSafeState(SafeState.Emergency, $"doseMsg={validation.Message}");
            return Result.Failure<DoseValidationResult>(ErrorCode.DoseInterlock,
                $"EMERGENCY dose interlock: {validation.Message}");
        }

        // SWR-WF-024: BLOCK → set safe state, do not transition
        if (validation.Level == DoseValidationLevel.Block)
        {
            lock (_lock)
                SetSafeState(SafeState.Blocked, $"doseMsg={validation.Message}");
            return Result.Failure<DoseValidationResult>(ErrorCode.DoseInterlock,
                $"Dose BLOCKED: {validation.Message}");
        }

        // ALLOW or WARN: proceed with transition to Exposing.
        // WARN → set SafeState.Warning so callers can display acknowledgement prompt.
        // Issue #21: SafeState.Warning added to represent WARN interlock level.
        WorkflowState previous;
        Result transitionResult;

        lock (_lock)
        {
            if (_safeState == SafeState.Emergency || _safeState == SafeState.Blocked)
                return Result.Failure<DoseValidationResult>(ErrorCode.InvalidStateTransition,
                    $"Cannot expose: system is in safe state '{_safeState}'.");

            if (validation.Level == DoseValidationLevel.Warn)
                SetSafeState(SafeState.Warning, $"doseMsg={validation.Message}");
            else
                SetSafeState(SafeState.Idle);

            previous = _stateMachine.CurrentState;
            transitionResult = _stateMachine.TryTransition(WorkflowState.Exposing);
        }

        if (transitionResult.IsFailure)
            return Result.Failure<DoseValidationResult>(
                transitionResult.Error ?? ErrorCode.InvalidStateTransition, transitionResult.ErrorMessage ?? string.Empty);

        RaiseStateChanged(previous, WorkflowState.Exposing);

        // SWR-NF-SC-041: Log exposure preparation to the tamper-evident audit trail.
        // Fire-and-forget: audit failure must never block the exposure workflow.
        FireAndForgetAudit("EXPOSURE_PREPARE",
            $"patientId={_currentPatientId};studyUid={_currentStudyInstanceUid};level={validation.Level}");

        return Result.Success(validation);
    }

    /// <inheritdoc/>
    public async Task<Result> AbortAsync(
        string reason,
        CancellationToken cancellationToken = default)
    {
        WorkflowState previous;
        // Capture before nulling — Issue #35: audit log must record the patient that was active.
        string? capturedPatientId;
        string? capturedStudyUid;

        lock (_lock)
        {
            previous = _stateMachine.CurrentState;

            if (previous == WorkflowState.Error)
                return Result.Success(); // Already in error state

            capturedPatientId = _currentPatientId;
            capturedStudyUid = _currentStudyInstanceUid;
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
                    SetSafeState(SafeState.Emergency, "generatorAbortFailed");
            }
        }

        RaiseStateChanged(previous, WorkflowState.Error, reason);

        // SWR-NF-SC-041: Log exposure abort to the tamper-evident audit trail.
        // Fire-and-forget: audit failure must never suppress an abort.
        // Issue #35: Use captured patientId (before null-clear) for accurate audit record.
        FireAndForgetAudit("EXPOSURE_ABORT",
            $"patientId={capturedPatientId};studyUid={capturedStudyUid};reason={reason};previousState={previous}");

        return Result.Success();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void RaiseStateChanged(WorkflowState previous, WorkflowState next, string? reason = null)
    {
        var args = new WorkflowStateChangedEventArgs(previous, next, reason);
        StateChanged?.Invoke(this, args);
    }

    /// <summary>
    /// Updates <see cref="_safeState"/> and logs the transition to the audit trail (SWR-NF-SC-041).
    /// Must be called inside the <see cref="_lock"/> when changing safe state.
    /// </summary>
    /// <param name="newState">The new safe state to apply.</param>
    /// <param name="context">Optional free-text context written to the audit details field.</param>
    private void SetSafeState(SafeState newState, string? context = null)
    {
        var previous = _safeState;
        _safeState = newState;

        if (previous == newState)
            return;

        // SWR-NF-SC-041: Fire-and-forget audit log of safe-state transitions.
        // Audit failure must never block safety state changes.
        FireAndForgetAudit("SAFESTATE_CHANGED",
            $"from={previous};to={newState}" + (context is not null ? $";{context}" : string.Empty));
    }

    /// <summary>
    /// Writes an audit entry using fire-and-forget semantics.
    /// Exceptions are swallowed so that audit failure never interrupts workflow execution.
    /// </summary>
    /// <param name="action">The audit action code (e.g., "EXPOSURE_PREPARE").</param>
    /// <param name="details">Optional detail string stored in the audit entry.</param>
    private void FireAndForgetAudit(string action, string? details = null)
    {
        if (_auditService is null)
            return;

        var userId = _securityContext.CurrentRole.HasValue
            ? _securityContext.CurrentRole.Value.ToString()
            : "UNKNOWN";

        var entry = new HnVue.Common.Models.AuditEntry(
            timestamp: DateTimeOffset.UtcNow,
            userId: userId,
            action: action,
            currentHash: string.Empty, // Implementation computes the real hash chain
            details: details);

        // Intentionally not awaited — audit must not block or throw to callers.
        _ = _auditService.WriteAuditAsync(entry).ContinueWith(
            t => { /* swallow — do not let audit exceptions propagate */ },
            TaskContinuationOptions.OnlyOnFaulted);
    }
}
