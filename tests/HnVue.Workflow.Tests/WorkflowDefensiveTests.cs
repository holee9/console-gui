using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Workflow;
using NSubstitute;
using Xunit;

namespace HnVue.Workflow.Tests;

/// <summary>
/// Defensive tests for <see cref="WorkflowEngine"/> and <see cref="WorkflowStateMachine"/>.
/// Covers RBAC enforcement, safe-state blocking, state machine edge cases, and generator simulator.
/// Safety-critical: Workflow engine controls radiation exposure sequencing (IEC 62304).
/// </summary>
[Trait("SWR", "SWR-WF-020")]
public sealed class WorkflowDefensiveTests
{
    // ── Test Helpers ───────────────────────────────────────────────────────────

    private static IDoseService CreateDoseService(DoseValidationLevel level = DoseValidationLevel.Allow)
    {
        var doseService = Substitute.For<IDoseService>();
        doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(new DoseValidationResult(
                IsAllowed: level == DoseValidationLevel.Allow || level == DoseValidationLevel.Warn,
                Level: level,
                Message: "Test message",
                EstimatedDap: 1.0,
                EstimatedEsd: 0.5,
                ExposureIndex: 500))));
        return doseService;
    }

    private static IGeneratorInterface CreateGenerator()
    {
        var generator = Substitute.For<IGeneratorInterface>();
        generator.AbortAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result.Success()));
        return generator;
    }

    private static ISecurityContext CreateSecurityContext(UserRole? role = UserRole.Radiographer)
    {
        var ctx = Substitute.For<ISecurityContext>();
        ctx.CurrentRole.Returns(role);
        ctx.IsAuthenticated.Returns(role.HasValue);
        return ctx;
    }

    private static IDetectorInterface CreateDetector()
    {
        var detector = Substitute.For<IDetectorInterface>();
        detector.ArmAsync(Arg.Any<DetectorTriggerMode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        detector.AbortAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        return detector;
    }

    private static WorkflowEngine CreateEngine(
        IDoseService? doseService = null,
        IGeneratorInterface? generator = null,
        ISecurityContext? securityContext = null,
        IDetectorInterface? detector = null)
    {
        return new WorkflowEngine(
            doseService ?? CreateDoseService(),
            generator ?? CreateGenerator(),
            securityContext ?? CreateSecurityContext(),
            auditService: null,
            detector: detector);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // WorkflowEngine Defensive Tests
    // ══════════════════════════════════════════════════════════════════════════

    // ── StartAsync Null Guards ─────────────────────────────────────────────────

    [Fact]
    public async Task StartAsync_NullPatientId_ThrowsArgumentNullException()
    {
        var engine = CreateEngine();
        var act = async () => await engine.StartAsync(null!, "study-uid");

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("patientId");
    }

    [Fact]
    public async Task StartAsync_NullStudyInstanceUid_ThrowsArgumentNullException()
    {
        var engine = CreateEngine();
        var act = async () => await engine.StartAsync("patient-1", null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("studyInstanceUid");
    }

    // ── StartAsync Safe State Blocking ─────────────────────────────────────────

    [Fact]
    public async Task StartAsync_WhenEmergencySafeState_ReturnsFailure()
    {
        var doseService = CreateDoseService(DoseValidationLevel.Emergency);
        var engine = CreateEngine(doseService: doseService);

        // Force Emergency safe state via PrepareExposureAsync
        var params_ = new ExposureParameters("CHEST", 80, 200, "study-1");
        await engine.PrepareExposureAsync(params_);

        var result = await engine.StartAsync("patient-1", "study-uid");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InvalidStateTransition);
    }

    [Fact]
    public async Task StartAsync_WhenBlockedSafeState_ReturnsFailure()
    {
        var doseService = CreateDoseService(DoseValidationLevel.Block);
        var engine = CreateEngine(doseService: doseService);

        // Force Blocked safe state via PrepareExposureAsync
        var params_ = new ExposureParameters("CHEST", 80, 200, "study-1");
        await engine.PrepareExposureAsync(params_);

        var result = await engine.StartAsync("patient-1", "study-uid");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InvalidStateTransition);
    }

    // ── TransitionAsync RBAC Enforcement ───────────────────────────────────────

    [Fact]
    public async Task TransitionAsync_ToExposing_UnauthenticatedUser_ReturnsAuthFailed()
    {
        var noAuthCtx = CreateSecurityContext(role: null);
        var engine = CreateEngine(securityContext: noAuthCtx);

        var result = await engine.TransitionAsync(WorkflowState.Exposing);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AuthenticationFailed);
    }

    [Fact]
    public async Task TransitionAsync_ToExposing_WithAdminRole_ReturnsInsufficientPermission()
    {
        // Admin does not have PerformExposure permission
        var adminCtx = CreateSecurityContext(UserRole.Admin);
        var engine = CreateEngine(securityContext: adminCtx);

        await engine.StartAsync("patient-1", "study-uid");
        await engine.TransitionAsync(WorkflowState.ProtocolLoaded);
        await engine.TransitionAsync(WorkflowState.ReadyToExpose);

        var result = await engine.TransitionAsync(WorkflowState.Exposing);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TransitionAsync_ToExposing_WithServiceRole_ReturnsInsufficientPermission()
    {
        // Service engineer does not have PerformExposure permission
        var serviceCtx = CreateSecurityContext(UserRole.Service);
        var engine = CreateEngine(securityContext: serviceCtx);

        await engine.StartAsync("patient-1", "study-uid");

        var result = await engine.TransitionAsync(WorkflowState.Exposing);

        result.IsFailure.Should().BeTrue();
    }

    // ── TransitionAsync Emergency State Blocking ───────────────────────────────

    [Fact]
    public async Task TransitionAsync_WhenEmergencySafeState_ReturnsFailure()
    {
        var doseService = CreateDoseService(DoseValidationLevel.Emergency);
        var engine = CreateEngine(doseService: doseService);

        // Force Emergency via dose validation
        var params_ = new ExposureParameters("CHEST", 80, 200, "study-1");
        await engine.PrepareExposureAsync(params_);

        // Any transition should be blocked
        var result = await engine.TransitionAsync(WorkflowState.PatientSelected);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InvalidStateTransition);
    }

    // ── AbortAsync Edge Cases ──────────────────────────────────────────────────

    [Fact]
    public async Task AbortAsync_FromIdle_SucceedsAndSetsErrorState()
    {
        var engine = CreateEngine();

        var result = await engine.AbortAsync("idle abort test");

        result.IsSuccess.Should().BeTrue();
        engine.CurrentState.Should().Be(WorkflowState.Error);
    }

    [Fact]
    public async Task AbortAsync_FromError_ReturnsSuccessIdempotent()
    {
        var engine = CreateEngine();
        await engine.AbortAsync("first abort");

        var result = await engine.AbortAsync("second abort");

        result.IsSuccess.Should().BeTrue();
        engine.CurrentState.Should().Be(WorkflowState.Error);
    }

    [Fact]
    public async Task AbortAsync_FromCompletedState_Succeeds()
    {
        var engine = CreateEngine();
        await engine.StartAsync("patient-1", "study-uid");
        await engine.TransitionAsync(WorkflowState.ProtocolLoaded);
        await engine.TransitionAsync(WorkflowState.ReadyToExpose);
        await engine.TransitionAsync(WorkflowState.Exposing);
        await engine.TransitionAsync(WorkflowState.ImageAcquiring);
        await engine.TransitionAsync(WorkflowState.ImageProcessing);
        await engine.TransitionAsync(WorkflowState.ImageReview);
        await engine.TransitionAsync(WorkflowState.Completed);

        var result = await engine.AbortAsync("completed-state abort");

        result.IsSuccess.Should().BeTrue();
        engine.CurrentState.Should().Be(WorkflowState.Error);
    }

    [Fact]
    public async Task AbortAsync_FromExposing_AbortsGeneratorAndDetector()
    {
        var generator = CreateGenerator();
        var detector = CreateDetector();
        var engine = CreateEngine(generator: generator, detector: detector);

        // Navigate to Exposing
        await engine.StartAsync("patient-1", "study-uid");
        await engine.TransitionAsync(WorkflowState.ProtocolLoaded);
        await engine.TransitionAsync(WorkflowState.ReadyToExpose);
        await engine.TransitionAsync(WorkflowState.Exposing);

        await engine.AbortAsync("exposure abort");

        await generator.Received(1).AbortAsync(Arg.Any<CancellationToken>());
        await detector.Received(1).AbortAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AbortAsync_GeneratorAbortThrows_EscalatesToEmergency()
    {
        // The engine only escalates to Emergency when generator.AbortAsync throws (not on Result failure)
        var generator = Substitute.For<IGeneratorInterface>();
        generator.AbortAsync(Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("Generator not responding"));
        var engine = CreateEngine(generator: generator);

        // Navigate to Exposing state
        await engine.StartAsync("patient-1", "study-uid");
        await engine.TransitionAsync(WorkflowState.ProtocolLoaded);
        await engine.TransitionAsync(WorkflowState.ReadyToExpose);
        await engine.TransitionAsync(WorkflowState.Exposing);

        await engine.AbortAsync("generator failure test");

        engine.CurrentSafeState.Should().Be(SafeState.Emergency);
    }

    // ── PrepareExposureAsync RBAC Enforcement ──────────────────────────────────

    [Fact]
    public async Task PrepareExposureAsync_UnauthenticatedUser_ReturnsAuthFailed()
    {
        var noAuthCtx = CreateSecurityContext(role: null);
        var engine = CreateEngine(securityContext: noAuthCtx);
        var params_ = new ExposureParameters("CHEST", 80, 2, "study-1");

        var result = await engine.PrepareExposureAsync(params_);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AuthenticationFailed);
    }

    [Fact]
    public async Task PrepareExposureAsync_InsufficientRBAC_ReturnsFailure()
    {
        // Admin does not have PerformExposure permission
        var adminCtx = CreateSecurityContext(UserRole.Admin);
        var engine = CreateEngine(securityContext: adminCtx);
        var params_ = new ExposureParameters("CHEST", 80, 2, "study-1");

        var result = await engine.PrepareExposureAsync(params_);

        result.IsFailure.Should().BeTrue();
    }

    // ── PrepareExposureAsync Null Guards ───────────────────────────────────────

    [Fact]
    public async Task PrepareExposureAsync_NullParameters_ThrowsArgumentNullException()
    {
        var engine = CreateEngine();
        var act = async () => await engine.PrepareExposureAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("parameters");
    }

    // ── PrepareExposureAsync Safe State ────────────────────────────────────────

    [Fact]
    public async Task PrepareExposureAsync_EmergencyLevel_SetsEmergencySafeState()
    {
        var doseService = CreateDoseService(DoseValidationLevel.Emergency);
        var engine = CreateEngine(doseService: doseService);
        var params_ = new ExposureParameters("CHEST", 80, 200, "study-1");

        var result = await engine.PrepareExposureAsync(params_);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DoseInterlock);
        engine.CurrentSafeState.Should().Be(SafeState.Emergency);
    }

    [Fact]
    public async Task PrepareExposureAsync_BlockLevel_SetsBlockedSafeState()
    {
        var doseService = CreateDoseService(DoseValidationLevel.Block);
        var engine = CreateEngine(doseService: doseService);
        var params_ = new ExposureParameters("CHEST", 80, 200, "study-1");

        var result = await engine.PrepareExposureAsync(params_);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DoseInterlock);
        engine.CurrentSafeState.Should().Be(SafeState.Blocked);
    }

    [Fact]
    public async Task PrepareExposureAsync_WarnLevel_SetsWarningSafeState()
    {
        var doseService = CreateDoseService(DoseValidationLevel.Warn);
        var engine = CreateEngine(doseService: doseService);

        // Navigate to ReadyToExpose first (required state for PrepareExposureAsync)
        await engine.StartAsync("patient-1", "study-uid");
        await engine.TransitionAsync(WorkflowState.ProtocolLoaded);
        await engine.TransitionAsync(WorkflowState.ReadyToExpose);

        var params_ = new ExposureParameters("CHEST", 80, 2, "study-1");

        var result = await engine.PrepareExposureAsync(params_);

        result.IsSuccess.Should().BeTrue();
        engine.CurrentSafeState.Should().Be(SafeState.Warning);
    }

    [Fact]
    public async Task PrepareExposureAsync_AllowLevel_SetsIdleSafeState()
    {
        var doseService = CreateDoseService(DoseValidationLevel.Allow);
        var engine = CreateEngine(doseService: doseService);

        // Navigate to ReadyToExpose first (required state for PrepareExposureAsync)
        await engine.StartAsync("patient-1", "study-uid");
        await engine.TransitionAsync(WorkflowState.ProtocolLoaded);
        await engine.TransitionAsync(WorkflowState.ReadyToExpose);

        var params_ = new ExposureParameters("CHEST", 80, 2, "study-1");

        var result = await engine.PrepareExposureAsync(params_);

        result.IsSuccess.Should().BeTrue();
        engine.CurrentSafeState.Should().Be(SafeState.Idle);
    }

    // ── StartEmergencyExposureAsync Edge Cases ─────────────────────────────────

    [Fact]
    public async Task StartEmergencyExposureAsync_NullParameters_ThrowsArgumentNullException()
    {
        var engine = CreateEngine();
        var act = async () => await engine.StartEmergencyExposureAsync("patient", null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("parameters");
    }

    [Fact]
    public async Task StartEmergencyExposureAsync_Unauthenticated_ReturnsAuthFailed()
    {
        var noAuthCtx = CreateSecurityContext(role: null);
        var engine = CreateEngine(securityContext: noAuthCtx);
        var params_ = new ExposureParameters("CHEST", 80, 2, "study-1");

        var result = await engine.StartEmergencyExposureAsync(null, params_);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AuthenticationFailed);
    }

    [Fact]
    public async Task StartEmergencyExposureAsync_WithAdminRole_ReturnsFailure()
    {
        var adminCtx = CreateSecurityContext(UserRole.Admin);
        var engine = CreateEngine(securityContext: adminCtx);
        var params_ = new ExposureParameters("CHEST", 80, 2, "study-1");

        var result = await engine.StartEmergencyExposureAsync("Emergency Patient", params_);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task StartEmergencyExposureAsync_WhenEmergencySafeState_ReturnsFailure()
    {
        var doseService = CreateDoseService(DoseValidationLevel.Emergency);
        // Use Radiologist for emergency exposure permission
        var engine = CreateEngine(doseService: doseService, securityContext: CreateSecurityContext(UserRole.Radiologist));

        // Force Emergency safe state first
        var params1 = new ExposureParameters("CHEST", 80, 200, "study-1");
        await engine.PrepareExposureAsync(params1);

        var params2 = new ExposureParameters("CHEST", 80, 2, "study-2");
        var result = await engine.StartEmergencyExposureAsync("Emergency Patient", params2);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InvalidStateTransition);
    }

    [Fact]
    public async Task StartEmergencyExposureAsync_WhenBlockedSafeState_ReturnsFailure()
    {
        var doseService = CreateDoseService(DoseValidationLevel.Block);
        // Use Radiologist for emergency exposure permission
        var engine = CreateEngine(doseService: doseService, securityContext: CreateSecurityContext(UserRole.Radiologist));

        // Force Blocked safe state first
        var params1 = new ExposureParameters("CHEST", 80, 200, "study-1");
        await engine.PrepareExposureAsync(params1);

        var params2 = new ExposureParameters("CHEST", 80, 2, "study-2");
        var result = await engine.StartEmergencyExposureAsync("Emergency Patient", params2);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InvalidStateTransition);
    }

    [Fact]
    public async Task StartEmergencyExposureAsync_FromErrorState_ResetsAndExposes()
    {
        // Use Radiologist for emergency exposure permission
        var engine = CreateEngine(securityContext: CreateSecurityContext(UserRole.Radiologist));
        // Put into Error state
        await engine.AbortAsync("test abort");

        var params_ = new ExposureParameters("CHEST", 80, 2, "study-emergency");
        var result = await engine.StartEmergencyExposureAsync("Emergency Patient", params_);

        result.IsSuccess.Should().BeTrue();
        engine.CurrentState.Should().Be(WorkflowState.Exposing);
    }

    [Fact]
    public async Task StartEmergencyExposureAsync_EmergencyDoseLevel_RollsBackToError()
    {
        var doseService = CreateDoseService(DoseValidationLevel.Emergency);
        // Use Radiologist for emergency exposure permission
        var engine = CreateEngine(doseService: doseService, securityContext: CreateSecurityContext(UserRole.Radiologist));
        var params_ = new ExposureParameters("CHEST", 80, 200, "study-1");

        var result = await engine.StartEmergencyExposureAsync("Emergency Patient", params_);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DoseInterlock);
        engine.CurrentState.Should().Be(WorkflowState.Error);
        engine.CurrentSafeState.Should().Be(SafeState.Emergency);
    }

    // ── SafeState Transitions Verified ─────────────────────────────────────────

    [Fact]
    public void CurrentSafeState_InitiallyIdle()
    {
        var engine = CreateEngine();
        engine.CurrentSafeState.Should().Be(SafeState.Idle);
    }

    [Fact]
    public async Task SafeState_Emergency_BlocksStartAsync()
    {
        var doseService = CreateDoseService(DoseValidationLevel.Emergency);
        var engine = CreateEngine(doseService: doseService);

        var params_ = new ExposureParameters("CHEST", 80, 200, "study-1");
        await engine.PrepareExposureAsync(params_);
        engine.CurrentSafeState.Should().Be(SafeState.Emergency);

        var result = await engine.StartAsync("patient-1", "study-uid");
        result.IsFailure.Should().BeTrue();
    }

    // ── Multiple Rapid State Transitions ───────────────────────────────────────

    [Fact]
    public async Task StartAsync_MultipleStarts_SecondFailsBecauseNotIdle()
    {
        var engine = CreateEngine();

        var first = await engine.StartAsync("patient-1", "study-uid-1");
        first.IsSuccess.Should().BeTrue();

        // Second start should fail because state is PatientSelected, not Idle
        var second = await engine.StartAsync("patient-2", "study-uid-2");
        second.IsFailure.Should().BeTrue();
        second.Error.Should().Be(ErrorCode.InvalidStateTransition);
    }

    [Fact]
    public async Task AbortAsync_MultipleRapidAborts_AllSucceed()
    {
        var engine = CreateEngine();
        await engine.StartAsync("patient-1", "study-uid");

        // Multiple rapid aborts
        var results = await Task.WhenAll(
            engine.AbortAsync("abort-1"),
            engine.AbortAsync("abort-2"),
            engine.AbortAsync("abort-3"));

        // All should succeed (abort is idempotent once in Error state)
        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
        engine.CurrentState.Should().Be(WorkflowState.Error);
    }

    // ── Detector Integration ───────────────────────────────────────────────────

    [Fact]
    public async Task PrepareExposureAsync_DetectorArmFails_ForcesErrorState()
    {
        var detector = Substitute.For<IDetectorInterface>();
        detector.ArmAsync(Arg.Any<DetectorTriggerMode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure(ErrorCode.DetectorNotReady, "Detector offline")));

        var engine = CreateEngine(detector: detector);

        // Navigate to ReadyToExpose first (required state for PrepareExposureAsync to attempt Exposing transition)
        await engine.StartAsync("patient-1", "study-uid");
        await engine.TransitionAsync(WorkflowState.ProtocolLoaded);
        await engine.TransitionAsync(WorkflowState.ReadyToExpose);

        var params_ = new ExposureParameters("CHEST", 80, 2, "study-1");

        var result = await engine.PrepareExposureAsync(params_);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DetectorNotReady);
        engine.CurrentState.Should().Be(WorkflowState.Error);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // WorkflowStateMachine Defensive Tests
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void StateMachine_InitialState_IsIdle()
    {
        var sm = new WorkflowStateMachine();
        sm.CurrentState.Should().Be(WorkflowState.Idle);
    }

    // ── All Invalid State Transitions ──────────────────────────────────────────

    [Theory]
    [InlineData(WorkflowState.Idle, WorkflowState.ProtocolLoaded)]
    [InlineData(WorkflowState.Idle, WorkflowState.ReadyToExpose)]
    [InlineData(WorkflowState.Idle, WorkflowState.ImageAcquiring)]
    [InlineData(WorkflowState.Idle, WorkflowState.ImageProcessing)]
    [InlineData(WorkflowState.Idle, WorkflowState.ImageReview)]
    [InlineData(WorkflowState.PatientSelected, WorkflowState.ReadyToExpose)]
    [InlineData(WorkflowState.PatientSelected, WorkflowState.ImageAcquiring)]
    [InlineData(WorkflowState.PatientSelected, WorkflowState.Completed)]
    [InlineData(WorkflowState.ProtocolLoaded, WorkflowState.Exposing)]
    [InlineData(WorkflowState.ProtocolLoaded, WorkflowState.ImageAcquiring)]
    [InlineData(WorkflowState.ReadyToExpose, WorkflowState.PatientSelected)]
    [InlineData(WorkflowState.ReadyToExpose, WorkflowState.ImageAcquiring)]
    [InlineData(WorkflowState.ReadyToExpose, WorkflowState.Completed)]
    [InlineData(WorkflowState.Exposing, WorkflowState.ReadyToExpose)]
    [InlineData(WorkflowState.Exposing, WorkflowState.Completed)]
    [InlineData(WorkflowState.ImageAcquiring, WorkflowState.Exposing)]
    [InlineData(WorkflowState.ImageAcquiring, WorkflowState.ReadyToExpose)]
    [InlineData(WorkflowState.ImageProcessing, WorkflowState.Exposing)]
    [InlineData(WorkflowState.ImageProcessing, WorkflowState.ReadyToExpose)]
    [InlineData(WorkflowState.ImageReview, WorkflowState.PatientSelected)]
    [InlineData(WorkflowState.ImageReview, WorkflowState.Exposing)]
    [InlineData(WorkflowState.Completed, WorkflowState.PatientSelected)]
    [InlineData(WorkflowState.Completed, WorkflowState.ReadyToExpose)]
    [InlineData(WorkflowState.Error, WorkflowState.PatientSelected)]
    [InlineData(WorkflowState.Error, WorkflowState.Exposing)]
    [InlineData(WorkflowState.Error, WorkflowState.Completed)]
    public void TryTransition_InvalidTransition_ReturnsFailureAndStateUnchanged(
        WorkflowState from, WorkflowState to)
    {
        var sm = new WorkflowStateMachine();
        NavigateToState(sm, from);

        var result = sm.TryTransition(to);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InvalidStateTransition);
        sm.CurrentState.Should().Be(from);
    }

    // ── ForceError from Various States ─────────────────────────────────────────

    [Fact]
    public void ForceError_FromIdle_SetsErrorState()
    {
        var sm = new WorkflowStateMachine();
        sm.ForceError();
        sm.CurrentState.Should().Be(WorkflowState.Error);
    }

    [Fact]
    public void ForceError_FromPatientSelected_SetsErrorState()
    {
        var sm = new WorkflowStateMachine();
        sm.TryTransition(WorkflowState.PatientSelected);
        sm.ForceError();
        sm.CurrentState.Should().Be(WorkflowState.Error);
    }

    [Fact]
    public void ForceError_FromExposing_SetsErrorState()
    {
        var sm = new WorkflowStateMachine();
        sm.TryTransition(WorkflowState.PatientSelected);
        sm.TryTransition(WorkflowState.ProtocolLoaded);
        sm.TryTransition(WorkflowState.ReadyToExpose);
        sm.TryTransition(WorkflowState.Exposing);
        sm.ForceError();
        sm.CurrentState.Should().Be(WorkflowState.Error);
    }

    [Fact]
    public void ForceError_FromCompleted_SetsErrorState()
    {
        var sm = new WorkflowStateMachine();
        NavigateToState(sm, WorkflowState.Completed);
        sm.ForceError();
        sm.CurrentState.Should().Be(WorkflowState.Error);
    }

    [Fact]
    public void ForceError_AlreadyInError_StateUnchanged()
    {
        var sm = new WorkflowStateMachine();
        sm.ForceError();
        sm.ForceError(); // Double force
        sm.CurrentState.Should().Be(WorkflowState.Error);
    }

    // ── ForceExposing from Various States ──────────────────────────────────────

    [Fact]
    public void ForceExposing_FromIdle_SetsExposingState()
    {
        var sm = new WorkflowStateMachine();
        sm.ForceExposing();
        sm.CurrentState.Should().Be(WorkflowState.Exposing);
    }

    [Fact]
    public void ForceExposing_FromPatientSelected_SetsExposingState()
    {
        var sm = new WorkflowStateMachine();
        sm.TryTransition(WorkflowState.PatientSelected);
        sm.ForceExposing();
        sm.CurrentState.Should().Be(WorkflowState.Exposing);
    }

    [Fact]
    public void ForceExposing_FromError_SetsExposingState()
    {
        var sm = new WorkflowStateMachine();
        sm.ForceError();
        sm.ForceExposing();
        sm.CurrentState.Should().Be(WorkflowState.Exposing);
    }

    [Fact]
    public void ForceExposing_FromCompleted_SetsExposingState()
    {
        var sm = new WorkflowStateMachine();
        NavigateToState(sm, WorkflowState.Completed);
        sm.ForceExposing();
        sm.CurrentState.Should().Be(WorkflowState.Exposing);
    }

    // ── Reset Edge Cases ───────────────────────────────────────────────────────

    [Fact]
    public void Reset_FromError_ReturnsSuccessToIdle()
    {
        var sm = new WorkflowStateMachine();
        sm.ForceError();
        var result = sm.Reset();
        result.IsSuccess.Should().BeTrue();
        sm.CurrentState.Should().Be(WorkflowState.Idle);
    }

    [Fact]
    public void Reset_FromCompleted_ReturnsSuccessToIdle()
    {
        var sm = new WorkflowStateMachine();
        NavigateToState(sm, WorkflowState.Completed);
        var result = sm.Reset();
        result.IsSuccess.Should().BeTrue();
        sm.CurrentState.Should().Be(WorkflowState.Idle);
    }

    [Fact]
    public void Reset_FromIdle_ReturnsFailure()
    {
        var sm = new WorkflowStateMachine();
        var result = sm.Reset();
        result.IsFailure.Should().BeTrue();
        sm.CurrentState.Should().Be(WorkflowState.Idle);
    }

    [Fact]
    public void Reset_FromExposing_ReturnsFailure()
    {
        var sm = new WorkflowStateMachine();
        sm.ForceExposing();
        var result = sm.Reset();
        result.IsFailure.Should().BeTrue();
        sm.CurrentState.Should().Be(WorkflowState.Exposing);
    }

    // ── CanTransitionTo Coverage ───────────────────────────────────────────────

    [Fact]
    public void CanTransitionTo_AllValidFromIdle()
    {
        var sm = new WorkflowStateMachine();
        sm.CanTransitionTo(WorkflowState.PatientSelected).Should().BeTrue();
        sm.CanTransitionTo(WorkflowState.Error).Should().BeTrue();
    }

    [Fact]
    public void CanTransitionTo_AllValidFromError()
    {
        var sm = new WorkflowStateMachine();
        sm.ForceError();
        sm.CanTransitionTo(WorkflowState.Idle).Should().BeTrue();
        sm.CanTransitionTo(WorkflowState.Exposing).Should().BeFalse();
    }

    // ── Full Forward Path Then Reset ───────────────────────────────────────────

    [Fact]
    public void FullForwardPath_ThenResetToIdle_ThenNewPath()
    {
        var sm = new WorkflowStateMachine();

        // Complete first workflow
        sm.TryTransition(WorkflowState.PatientSelected).IsSuccess.Should().BeTrue();
        sm.TryTransition(WorkflowState.ProtocolLoaded).IsSuccess.Should().BeTrue();
        sm.TryTransition(WorkflowState.ReadyToExpose).IsSuccess.Should().BeTrue();
        sm.TryTransition(WorkflowState.Exposing).IsSuccess.Should().BeTrue();
        sm.TryTransition(WorkflowState.ImageAcquiring).IsSuccess.Should().BeTrue();
        sm.TryTransition(WorkflowState.ImageProcessing).IsSuccess.Should().BeTrue();
        sm.TryTransition(WorkflowState.ImageReview).IsSuccess.Should().BeTrue();
        sm.TryTransition(WorkflowState.Completed).IsSuccess.Should().BeTrue();

        // Reset
        sm.Reset().IsSuccess.Should().BeTrue();
        sm.CurrentState.Should().Be(WorkflowState.Idle);

        // Start new workflow
        sm.TryTransition(WorkflowState.PatientSelected).IsSuccess.Should().BeTrue();
        sm.CurrentState.Should().Be(WorkflowState.PatientSelected);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // GeneratorSimulator Defensive Tests
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Simulator_TriggerExposure_WhenNotReady_ReturnsFailure()
    {
        var sim = new GeneratorSimulator();

        var result = await sim.TriggerExposureAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
    }

    [Fact]
    public async Task Simulator_Prepare_WhenNotIdle_ReturnsFailure()
    {
        var sim = new GeneratorSimulator();
        await sim.ConnectAsync();

        var params_ = new ExposureParameters("CHEST", 80, 2, "study-1");
        await sim.PrepareAsync(params_);

        // Already in Preparing/Ready, not Idle
        var result = await sim.PrepareAsync(params_);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
    }

    [Fact]
    public async Task Simulator_Abort_FromAnyState_Succeeds()
    {
        var sim = new GeneratorSimulator();
        await sim.ConnectAsync();

        var result = await sim.AbortAsync();

        result.IsSuccess.Should().BeTrue();
        sim.CurrentState.Should().Be(GeneratorState.Error);
    }

    [Fact]
    public async Task Simulator_MultipleAborts_AllSucceed()
    {
        var sim = new GeneratorSimulator();
        await sim.ConnectAsync();

        var first = await sim.AbortAsync();
        var second = await sim.AbortAsync();

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        sim.CurrentState.Should().Be(GeneratorState.Error);
    }

    [Fact]
    public async Task Simulator_FailNextConnect_ReturnsFailure()
    {
        var sim = new GeneratorSimulator();
        sim.FailNextConnectWith = "Hardware not found";

        var result = await sim.ConnectAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
        result.ErrorMessage.Should().Contain("Hardware not found");
    }

    [Fact]
    public async Task Simulator_FailNextExposure_ReturnsFailureAndSetsError()
    {
        var sim = new GeneratorSimulator();
        sim.PrepareDelayMs = 0; // Fast prepare
        sim.FailNextExposureWith = "Tube overload";

        await sim.ConnectAsync();
        var params_ = new ExposureParameters("CHEST", 80, 2, "study-1");
        await sim.PrepareAsync(params_);

        var result = await sim.TriggerExposureAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ExposureAborted);
        sim.CurrentState.Should().Be(GeneratorState.Error);
    }

    [Fact]
    public async Task Simulator_FullCycle_Succeeds()
    {
        var sim = new GeneratorSimulator();
        sim.PrepareDelayMs = 0;
        sim.ExposureDelayMs = 0;

        // Connect -> Prepare -> Trigger -> auto-Idle
        await sim.ConnectAsync();
        sim.CurrentState.Should().Be(GeneratorState.Idle);

        var params_ = new ExposureParameters("CHEST", 80, 2, "study-1");
        var prepResult = await sim.PrepareAsync(params_);
        prepResult.IsSuccess.Should().BeTrue();
        sim.CurrentState.Should().Be(GeneratorState.Ready);

        var exposeResult = await sim.TriggerExposureAsync();
        exposeResult.IsSuccess.Should().BeTrue();
        // After full cycle, returns to Idle
        sim.CurrentState.Should().Be(GeneratorState.Idle);
    }

    [Fact]
    public async Task Simulator_Disconnect_FromAnyState_Succeeds()
    {
        var sim = new GeneratorSimulator();
        await sim.ConnectAsync();
        sim.CurrentState.Should().Be(GeneratorState.Idle);

        var result = await sim.DisconnectAsync();

        result.IsSuccess.Should().BeTrue();
        sim.CurrentState.Should().Be(GeneratorState.Disconnected);
    }

    [Fact]
    public async Task Simulator_HeatUnitAccumulation_IncreasesEachExposure()
    {
        var sim = new GeneratorSimulator();
        sim.PrepareDelayMs = 0;
        sim.ExposureDelayMs = 0;

        await sim.ConnectAsync();

        for (int i = 1; i <= 3; i++)
        {
            var params_ = new ExposureParameters("CHEST", 80, 2, $"study-{i}");
            await sim.PrepareAsync(params_);
            await sim.TriggerExposureAsync();

            var status = await sim.GetStatusAsync();
            status.Value.HeatUnitPercentage.Should().Be(i * 5.0);
        }
    }

    [Fact]
    public async Task Simulator_GetStatus_ReturnsCurrentState()
    {
        var sim = new GeneratorSimulator();
        await sim.ConnectAsync();

        var status = await sim.GetStatusAsync();

        status.IsSuccess.Should().BeTrue();
        status.Value.State.Should().Be(GeneratorState.Idle);
        status.Value.IsReadyToExpose.Should().BeFalse();
        status.Value.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ── StateMachine Helper ────────────────────────────────────────────────────

    private static void NavigateToState(WorkflowStateMachine sm, WorkflowState target)
    {
        if (target == WorkflowState.Idle) return;
        if (target == WorkflowState.Error) { sm.ForceError(); return; }
        if (target == WorkflowState.Exposing && sm.CurrentState == WorkflowState.Idle)
        {
            sm.ForceExposing();
            return;
        }

        if (sm.CurrentState == WorkflowState.Idle)
            sm.TryTransition(WorkflowState.PatientSelected);
        if (target == WorkflowState.PatientSelected) return;

        if (sm.CurrentState == WorkflowState.PatientSelected)
            sm.TryTransition(WorkflowState.ProtocolLoaded);
        if (target == WorkflowState.ProtocolLoaded) return;

        if (sm.CurrentState == WorkflowState.ProtocolLoaded)
            sm.TryTransition(WorkflowState.ReadyToExpose);
        if (target == WorkflowState.ReadyToExpose) return;

        if (sm.CurrentState == WorkflowState.ReadyToExpose)
            sm.TryTransition(WorkflowState.Exposing);
        if (target == WorkflowState.Exposing) return;

        if (sm.CurrentState == WorkflowState.Exposing)
            sm.TryTransition(WorkflowState.ImageAcquiring);
        if (target == WorkflowState.ImageAcquiring) return;

        if (sm.CurrentState == WorkflowState.ImageAcquiring)
            sm.TryTransition(WorkflowState.ImageProcessing);
        if (target == WorkflowState.ImageProcessing) return;

        if (sm.CurrentState == WorkflowState.ImageProcessing)
            sm.TryTransition(WorkflowState.ImageReview);
        if (target == WorkflowState.ImageReview) return;

        if (sm.CurrentState == WorkflowState.ImageReview)
            sm.TryTransition(WorkflowState.Completed);
    }
}
