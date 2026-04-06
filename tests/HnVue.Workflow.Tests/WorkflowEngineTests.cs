using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Workflow;
using NSubstitute;
using Xunit;

namespace HnVue.Workflow.Tests;

[Trait("SWR", "SWR-WF-020")]
public sealed class WorkflowEngineTests
{
    private readonly IDoseService _doseService;
    private readonly IGeneratorInterface _generator;
    private readonly ISecurityContext _securityContext;
    private readonly WorkflowEngine _sut;

    public WorkflowEngineTests()
    {
        _doseService = Substitute.For<IDoseService>();
        _generator = Substitute.For<IGeneratorInterface>();
        _securityContext = Substitute.For<ISecurityContext>();
        // Default: authenticated Radiographer with PerformExposure permission.
        _securityContext.CurrentRole.Returns(UserRole.Radiographer);
        _securityContext.IsAuthenticated.Returns(true);
        _sut = new WorkflowEngine(_doseService, _generator, _securityContext);
    }

    // ── Constructor guards ────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullDoseService_ThrowsArgumentNullException()
    {
        var act = () => new WorkflowEngine(null!, _generator, _securityContext);

        act.Should().Throw<ArgumentNullException>().WithParameterName("doseService");
    }

    [Fact]
    public void Constructor_NullGenerator_ThrowsArgumentNullException()
    {
        var act = () => new WorkflowEngine(_doseService, null!, _securityContext);

        act.Should().Throw<ArgumentNullException>().WithParameterName("generator");
    }

    [Fact]
    public void Constructor_NullSecurityContext_ThrowsArgumentNullException()
    {
        var act = () => new WorkflowEngine(_doseService, _generator, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("securityContext");
    }

    // ── Initial state ─────────────────────────────────────────────────────────

    [Fact]
    public void InitialState_IsIdle()
    {
        _sut.CurrentState.Should().Be(WorkflowState.Idle);
    }

    [Fact]
    public void InitialSafeState_IsIdle()
    {
        _sut.CurrentSafeState.Should().Be(SafeState.Idle);
    }

    // ── StartAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task StartAsync_FromIdle_TransitionsToPatientSelected()
    {
        var result = await _sut.StartAsync("P001", "1.2.3.4.5");

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(WorkflowState.PatientSelected);
    }

    [Fact]
    public async Task StartAsync_RaisesStateChangedEvent()
    {
        WorkflowStateChangedEventArgs? eventArgs = null;
        _sut.StateChanged += (_, e) => eventArgs = e;

        await _sut.StartAsync("P001", "1.2.3.4.5");

        eventArgs.Should().NotBeNull();
        eventArgs!.PreviousState.Should().Be(WorkflowState.Idle);
        eventArgs.NewState.Should().Be(WorkflowState.PatientSelected);
    }

    [Fact]
    public async Task StartAsync_FromPatientSelected_Fails()
    {
        await _sut.StartAsync("P001", "1.2.3.4.5"); // move to PatientSelected

        var result = await _sut.StartAsync("P002", "1.2.3.4.6");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InvalidStateTransition);
    }

    [Fact]
    public async Task StartAsync_NullPatientId_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.StartAsync(null!, "1.2.3.4.5");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task StartAsync_NullStudyUid_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.StartAsync("P001", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── TransitionAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task TransitionAsync_ValidNextState_Succeeds()
    {
        await _sut.StartAsync("P001", "1.2.3");

        var result = await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(WorkflowState.ProtocolLoaded);
    }

    [Fact]
    public async Task TransitionAsync_InvalidTransition_ReturnsFailure()
    {
        // From Idle, cannot go directly to Completed
        var result = await _sut.TransitionAsync(WorkflowState.Completed);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InvalidStateTransition);
    }

    [Fact]
    public async Task TransitionAsync_RaisesStateChangedEvent()
    {
        await _sut.StartAsync("P001", "1.2.3");
        WorkflowStateChangedEventArgs? eventArgs = null;
        _sut.StateChanged += (_, e) => eventArgs = e;

        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);

        eventArgs.Should().NotBeNull();
        eventArgs!.NewState.Should().Be(WorkflowState.ProtocolLoaded);
    }

    // ── AbortAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AbortAsync_FromActiveState_TransitionsToError()
    {
        await _sut.StartAsync("P001", "1.2.3");

        var result = await _sut.AbortAsync("Test abort");

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(WorkflowState.Error);
    }

    [Fact]
    public async Task AbortAsync_FromIdle_SucceedsAndMovesToError()
    {
        var result = await _sut.AbortAsync("Emergency stop from Idle");

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(WorkflowState.Error);
    }

    [Fact]
    public async Task AbortAsync_FromError_ReturnsSuccessImmediately()
    {
        await _sut.AbortAsync("First abort");

        var result = await _sut.AbortAsync("Second abort");

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(WorkflowState.Error);
    }

    [Fact]
    public async Task AbortAsync_WhileExposing_CallsGeneratorAbort()
    {
        _generator.AbortAsync(Arg.Any<CancellationToken>()).Returns(Result.Success());
        await _sut.StartAsync("P001", "1.2.3");
        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await _sut.TransitionAsync(WorkflowState.ReadyToExpose);
        await _sut.TransitionAsync(WorkflowState.Exposing);

        await _sut.AbortAsync("Exposing abort");

        await _generator.Received(1).AbortAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AbortAsync_RaisesStateChangedWithReason()
    {
        await _sut.StartAsync("P001", "1.2.3");
        WorkflowStateChangedEventArgs? eventArgs = null;
        _sut.StateChanged += (_, e) => eventArgs = e;

        await _sut.AbortAsync("Patient requested abort");

        eventArgs.Should().NotBeNull();
        eventArgs!.NewState.Should().Be(WorkflowState.Error);
        eventArgs.Reason.Should().Be("Patient requested abort");
    }

    // ── RBAC enforcement ─────────────────────────────────────────────────────

    [Fact]
    public async Task TransitionAsync_ToExposing_WhenRoleIsNull_ReturnsUnauthorized()
    {
        _securityContext.CurrentRole.Returns((UserRole?)null);
        await _sut.StartAsync("P001", "1.2.3");
        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await _sut.TransitionAsync(WorkflowState.ReadyToExpose);

        var result = await _sut.TransitionAsync(WorkflowState.Exposing);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AuthenticationFailed);
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Service)]
    public async Task TransitionAsync_ToExposing_WhenRoleHasNoExposurePermission_ReturnsFailure(UserRole role)
    {
        _securityContext.CurrentRole.Returns(role);
        await _sut.StartAsync("P001", "1.2.3");
        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await _sut.TransitionAsync(WorkflowState.ReadyToExpose);

        var result = await _sut.TransitionAsync(WorkflowState.Exposing);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InsufficientPermission);
    }

    [Theory]
    [InlineData(UserRole.Radiographer)]
    [InlineData(UserRole.Radiologist)]
    public async Task TransitionAsync_ToExposing_WhenRoleHasExposurePermission_Succeeds(UserRole role)
    {
        _securityContext.CurrentRole.Returns(role);
        await _sut.StartAsync("P001", "1.2.3");
        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await _sut.TransitionAsync(WorkflowState.ReadyToExpose);

        var result = await _sut.TransitionAsync(WorkflowState.Exposing);

        result.IsSuccess.Should().BeTrue();
    }

    // ── Full happy path ───────────────────────────────────────────────────────

    [Fact]
    public async Task FullWorkflow_HappyPath_ReachesCompleted()
    {
        await _sut.StartAsync("P001", "1.2.3");
        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await _sut.TransitionAsync(WorkflowState.ReadyToExpose);
        await _sut.TransitionAsync(WorkflowState.Exposing);
        await _sut.TransitionAsync(WorkflowState.ImageAcquiring);
        await _sut.TransitionAsync(WorkflowState.ImageProcessing);
        await _sut.TransitionAsync(WorkflowState.ImageReview);
        var result = await _sut.TransitionAsync(WorkflowState.Completed);

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(WorkflowState.Completed);
    }

    // ── Audit logging (SWR-NF-SC-041, Issue #30) ─────────────────────────────

    [Fact]
    public async Task PrepareExposureAsync_WithAuditService_WritesExposurePrepareAuditEntry()
    {
        // Arrange
        var auditService = Substitute.For<IAuditService>();
        auditService.WriteAuditAsync(Arg.Any<HnVue.Common.Models.AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = new WorkflowEngine(_doseService, _generator, _securityContext, auditService);

        var validation = new DoseValidationResult(IsAllowed: true, Level: DoseValidationLevel.Allow, Message: null);
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(validation));

        await sut.StartAsync("P001", "1.2.3");
        await sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await sut.TransitionAsync(WorkflowState.ReadyToExpose);

        // Act
        var result = await sut.PrepareExposureAsync(MakeParams());

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Allow brief propagation of fire-and-forget task
        await Task.Delay(50);
        await auditService.Received(1).WriteAuditAsync(
            Arg.Is<HnVue.Common.Models.AuditEntry>(e => e.Action == "EXPOSURE_PREPARE"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PrepareExposureAsync_WithNullAuditService_DoesNotThrow()
    {
        // Arrange — sut created without audit service (default null)
        var validation = new DoseValidationResult(IsAllowed: true, Level: DoseValidationLevel.Allow, Message: null);
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(validation));

        await _sut.StartAsync("P001", "1.2.3");
        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await _sut.TransitionAsync(WorkflowState.ReadyToExpose);

        // Act & Assert — must not throw even though IAuditService is absent
        var act = async () => await _sut.PrepareExposureAsync(MakeParams());
        await act.Should().NotThrowAsync();
    }

    // ── PrepareExposureAsync — Dose Interlock (SWR-WF-023~025, Issue #21) ──────

    private static ExposureParameters MakeParams(string bodyPart = "CHEST", double kvp = 80.0, double mas = 5.0)
        => new(bodyPart, kvp, mas, "1.2.3");

    [Fact]
    public async Task PrepareExposureAsync_DoseAllow_TransitionsToExposing()
    {
        var validation = new DoseValidationResult(IsAllowed: true, Level: DoseValidationLevel.Allow, Message: null);
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(validation));

        await _sut.StartAsync("P001", "1.2.3");
        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await _sut.TransitionAsync(WorkflowState.ReadyToExpose);

        var result = await _sut.PrepareExposureAsync(MakeParams());

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
        _sut.CurrentState.Should().Be(WorkflowState.Exposing);
    }

    [Fact]
    public async Task PrepareExposureAsync_DoseWarn_TransitionsToExposingWithWarning()
    {
        var validation = new DoseValidationResult(IsAllowed: true, Level: DoseValidationLevel.Warn, Message: "DRL exceeded by 10%");
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(validation));

        await _sut.StartAsync("P001", "1.2.3");
        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await _sut.TransitionAsync(WorkflowState.ReadyToExpose);

        var result = await _sut.PrepareExposureAsync(MakeParams());

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
        _sut.CurrentState.Should().Be(WorkflowState.Exposing);
    }

    [Fact]
    public async Task PrepareExposureAsync_DoseBlock_ReturnsDoseInterlockFailureAndSetsBlockedState()
    {
        var validation = new DoseValidationResult(IsAllowed: false, Level: DoseValidationLevel.Block, Message: "Dose exceeds 3× DRL");
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(validation));

        await _sut.StartAsync("P001", "1.2.3");
        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await _sut.TransitionAsync(WorkflowState.ReadyToExpose);

        var result = await _sut.PrepareExposureAsync(MakeParams());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DoseInterlock);
        _sut.CurrentState.Should().Be(WorkflowState.ReadyToExpose); // state unchanged
        _sut.CurrentSafeState.Should().Be(SafeState.Blocked);
    }

    [Fact]
    public async Task PrepareExposureAsync_DoseEmergency_SetsEmergencyStateAndReturnsFailure()
    {
        var validation = new DoseValidationResult(IsAllowed: false, Level: DoseValidationLevel.Emergency, Message: "Critical dose threshold");
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(validation));

        await _sut.StartAsync("P001", "1.2.3");
        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await _sut.TransitionAsync(WorkflowState.ReadyToExpose);

        var result = await _sut.PrepareExposureAsync(MakeParams());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DoseInterlock);
        _sut.CurrentSafeState.Should().Be(SafeState.Emergency);
    }

    [Fact]
    public async Task PrepareExposureAsync_Unauthenticated_ReturnsAuthenticationFailed()
    {
        _securityContext.CurrentRole.Returns((UserRole?)null);

        await _sut.StartAsync("P001", "1.2.3");

        var result = await _sut.PrepareExposureAsync(MakeParams());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AuthenticationFailed);
    }
}
