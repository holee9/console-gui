using System.IO;
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
/// Coverage tests for <see cref="WorkflowEngine"/>.
/// Targets StartAsync, TransitionAsync, AbortAsync edge cases.
/// </summary>
[Trait("SWR", "SWR-WF-020")]
public sealed class WorkflowEngineCoverageTests
{
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

    // ── StartAsync Edge Cases ────────────────────────────────────────────────

    [Fact]
    public async Task StartAsync_NullPatientId_ThrowsArgumentNullException()
    {
        var engine = CreateEngine();
        var act = async () => await engine.StartAsync(null!, "study-uid");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task StartAsync_NullStudyInstanceUid_ThrowsArgumentNullException()
    {
        var engine = CreateEngine();
        var act = async () => await engine.StartAsync("patient-1", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task StartAsync_WhenBlocked_ReturnsFailure()
    {
        var doseService = CreateDoseService(DoseValidationLevel.Block);
        var engine = CreateEngine(doseService: doseService);
        // Force Blocked safe state via PrepareExposureAsync
        var prepParams = new ExposureParameters("CHEST", 80, 2, "study-1");
        await engine.PrepareExposureAsync(prepParams);

        var result = await engine.StartAsync("patient-1", "study-uid");

        result.IsFailure.Should().BeTrue();
    }

    // ── TransitionAsync Edge Cases ───────────────────────────────────────────

    [Fact]
    public async Task TransitionAsync_ToExposing_WithoutAuth_ReturnsAuthFailed()
    {
        var noAuthCtx = Substitute.For<ISecurityContext>();
        noAuthCtx.CurrentRole.Returns((UserRole?)null);

        var engine = CreateEngine(securityContext: noAuthCtx);
        var result = await engine.TransitionAsync(WorkflowState.Exposing);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AuthenticationFailed);
    }

    [Fact]
    public async Task TransitionAsync_ToExposing_WithInsufficientRole_ReturnsFailure()
    {
        // Use Admin role which does not have PerformExposure permission
        var adminCtx = CreateSecurityContext(UserRole.Admin);
        var engine = CreateEngine(securityContext: adminCtx);

        // Navigate to ReadyToExpose first
        await engine.StartAsync("patient-1", "study-uid");
        var result = await engine.TransitionAsync(WorkflowState.Exposing);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TransitionAsync_InvalidFromIdle_ReturnsInvalidTransition()
    {
        var engine = CreateEngine();
        var result = await engine.TransitionAsync(WorkflowState.ImageAcquiring);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InvalidStateTransition);
    }

    [Fact]
    public async Task TransitionAsync_ForwardPath_Succeeds()
    {
        var engine = CreateEngine();
        await engine.StartAsync("patient-1", "study-uid");

        var result = await engine.TransitionAsync(WorkflowState.ProtocolLoaded);

        result.IsSuccess.Should().BeTrue();
        engine.CurrentState.Should().Be(WorkflowState.ProtocolLoaded);
    }

    // ── AbortAsync Edge Cases ────────────────────────────────────────────────

    [Fact]
    public async Task AbortAsync_FromIdle_Succeeds()
    {
        var engine = CreateEngine();
        var result = await engine.AbortAsync("test abort");

        result.IsSuccess.Should().BeTrue();
        engine.CurrentState.Should().Be(WorkflowState.Error);
    }

    [Fact]
    public async Task AbortAsync_FromError_ReturnsSuccess()
    {
        var engine = CreateEngine();
        await engine.AbortAsync("first abort");

        // Already in error state
        var result = await engine.AbortAsync("second abort");

        result.IsSuccess.Should().BeTrue();
        engine.CurrentState.Should().Be(WorkflowState.Error);
    }

    [Fact]
    public async Task AbortAsync_FromPatientSelected_TransitionsToError()
    {
        var engine = CreateEngine();
        await engine.StartAsync("patient-1", "study-uid");

        var result = await engine.AbortAsync("patient emergency");

        result.IsSuccess.Should().BeTrue();
        engine.CurrentState.Should().Be(WorkflowState.Error);
    }

    [Fact]
    public async Task AbortAsync_RaisesStateChangedEvent()
    {
        var engine = CreateEngine();
        await engine.StartAsync("patient-1", "study-uid");

        WorkflowStateChangedEventArgs? capturedArgs = null;
        engine.StateChanged += (_, args) => capturedArgs = args;

        await engine.AbortAsync("test event");

        capturedArgs.Should().NotBeNull();
        capturedArgs!.PreviousState.Should().Be(WorkflowState.PatientSelected);
        capturedArgs.NewState.Should().Be(WorkflowState.Error);
        capturedArgs.Reason.Should().Be("test event");
    }

    // ── StartEmergencyExposureAsync Edge Cases ───────────────────────────────

    [Fact]
    public async Task StartEmergencyExposureAsync_NullParameters_ThrowsArgumentNullException()
    {
        var engine = CreateEngine();
        var act = async () => await engine.StartEmergencyExposureAsync("John Doe", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task StartEmergencyExposureAsync_WithoutAuth_ReturnsAuthFailed()
    {
        var noAuthCtx = Substitute.For<ISecurityContext>();
        noAuthCtx.CurrentRole.Returns((UserRole?)null);

        var engine = CreateEngine(securityContext: noAuthCtx);
        var params_ = new ExposureParameters("CHEST", 80, 2, "study-1");
        var result = await engine.StartEmergencyExposureAsync(null, params_);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AuthenticationFailed);
    }

    [Fact]
    public async Task StartEmergencyExposureAsync_WithAdminRole_ReturnsFailure()
    {
        // Admin role does not have PerformEmergencyExposure permission
        var adminCtx = CreateSecurityContext(UserRole.Admin);
        var engine = CreateEngine(securityContext: adminCtx);
        var params_ = new ExposureParameters("CHEST", 80, 2, "study-1");
        var result = await engine.StartEmergencyExposureAsync("Test", params_);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task StartEmergencyExposureAsync_WhenBlocked_ReturnsFailure()
    {
        var doseService = CreateDoseService(DoseValidationLevel.Block);
        var engine = CreateEngine(doseService: doseService);
        // Force blocked state
        var prepParams = new ExposureParameters("CHEST", 80, 200, "study-1");
        await engine.PrepareExposureAsync(prepParams);

        var emergencyParams = new ExposureParameters("CHEST", 80, 2, "study-2");
        var result = await engine.StartEmergencyExposureAsync("Emergency Patient", emergencyParams);

        result.IsFailure.Should().BeTrue();
    }

    // ── CurrentSafeState Coverage ────────────────────────────────────────────

    [Fact]
    public void CurrentSafeState_InitiallyIdle()
    {
        var engine = CreateEngine();
        engine.CurrentSafeState.Should().Be(SafeState.Idle);
    }

    [Fact]
    public async Task PrepareExposureAsync_WarnLevel_SetsWarningSafeState()
    {
        var doseService = CreateDoseService(DoseValidationLevel.Warn);
        var engine = CreateEngine(doseService: doseService);

        // Navigate to ReadyToExpose (required state for PrepareExposureAsync)
        await engine.StartAsync("patient-1", "study-uid");
        await engine.TransitionAsync(WorkflowState.ProtocolLoaded);
        await engine.TransitionAsync(WorkflowState.ReadyToExpose);

        var params_ = new ExposureParameters("CHEST", 80, 2, "study-uid");
        var result = await engine.PrepareExposureAsync(params_);

        result.IsSuccess.Should().BeTrue();
        engine.CurrentSafeState.Should().Be(SafeState.Warning);
    }

    // ── StateChanged Event ──────────────────────────────────────────────────

    [Fact]
    public async Task StartAsync_RaisesStateChangedEvent()
    {
        var engine = CreateEngine();
        WorkflowStateChangedEventArgs? capturedArgs = null;
        engine.StateChanged += (_, args) => capturedArgs = args;

        await engine.StartAsync("patient-1", "study-uid");

        capturedArgs.Should().NotBeNull();
        capturedArgs!.PreviousState.Should().Be(WorkflowState.Idle);
        capturedArgs.NewState.Should().Be(WorkflowState.PatientSelected);
    }

    [Fact]
    public async Task TransitionAsync_RaisesStateChangedEvent()
    {
        var engine = CreateEngine();
        await engine.StartAsync("patient-1", "study-uid");

        WorkflowStateChangedEventArgs? capturedArgs = null;
        engine.StateChanged += (_, args) => capturedArgs = args;

        await engine.TransitionAsync(WorkflowState.ProtocolLoaded);

        capturedArgs.Should().NotBeNull();
        capturedArgs!.PreviousState.Should().Be(WorkflowState.PatientSelected);
        capturedArgs.NewState.Should().Be(WorkflowState.ProtocolLoaded);
    }
}
