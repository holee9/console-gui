using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Workflow;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace HnVue.Workflow.Tests;

/// <summary>
/// Additional tests for <see cref="WorkflowEngine"/> covering SafeState transitions,
/// emergency exposure, AbortAsync edge cases, and event publication.
/// REQ-COV-003: Extends Workflow coverage from 81.9% towards 85%.
/// </summary>
[Trait("SWR", "SWR-WF-020")]
public sealed class WorkflowEngineAdditionalTests
{
    private readonly IDoseService _doseService;
    private readonly IGeneratorInterface _generator;
    private readonly IDetectorInterface _detector;
    private readonly ISecurityContext _securityContext;
    private readonly WorkflowEngine _sut;

    private static ExposureParameters MakeParams(string bodyPart = "CHEST", double kvp = 80.0, double mas = 5.0)
        => new(bodyPart, kvp, mas, "1.2.3");

    public WorkflowEngineAdditionalTests()
    {
        _doseService = Substitute.For<IDoseService>();
        _generator = Substitute.For<IGeneratorInterface>();
        _detector = Substitute.For<IDetectorInterface>();
        _securityContext = Substitute.For<ISecurityContext>();
        _securityContext.CurrentRole.Returns(UserRole.Radiographer);
        _securityContext.IsAuthenticated.Returns(true);
        _sut = new WorkflowEngine(_doseService, _generator, _securityContext);
    }

    // ── StartAsync – safe state guards ───────────────────────────────────────

    [Fact]
    public async Task StartAsync_WhenEmergencySafeState_ReturnsFailure()
    {
        // Force Emergency state via dose interlock
        var emergencyValidation = new DoseValidationResult(
            IsAllowed: false, Level: DoseValidationLevel.Emergency,
            Message: "Critical", EstimatedDap: 9.9, EstimatedEsd: 9.9, ExposureIndex: 9.9);
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(emergencyValidation));

        await _sut.StartAsync("P001", "1.2.3");
        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await _sut.TransitionAsync(WorkflowState.ReadyToExpose);
        await _sut.PrepareExposureAsync(MakeParams()); // sets Emergency

        var result = await _sut.StartAsync("P002", "1.2.3.4");

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Contain("safe state");
    }

    [Fact]
    public async Task StartAsync_WhenBlockedSafeState_ReturnsFailure()
    {
        var blockValidation = new DoseValidationResult(
            IsAllowed: false, Level: DoseValidationLevel.Block,
            Message: "Blocked", EstimatedDap: 0.5, EstimatedEsd: 0.5, ExposureIndex: 0.5);
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(blockValidation));

        await _sut.StartAsync("P001", "1.2.3");
        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await _sut.TransitionAsync(WorkflowState.ReadyToExpose);
        await _sut.PrepareExposureAsync(MakeParams()); // sets Blocked

        var result = await _sut.StartAsync("P002", "1.2.3.4");

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Contain("safe state");
    }

    // ── TransitionAsync – emergency safe state guard ─────────────────────────

    [Fact]
    public async Task TransitionAsync_WhenEmergencySafeState_ReturnsFailure()
    {
        var emergencyValidation = new DoseValidationResult(
            IsAllowed: false, Level: DoseValidationLevel.Emergency,
            Message: "Critical", EstimatedDap: 9.9, EstimatedEsd: 9.9, ExposureIndex: 9.9);
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(emergencyValidation));

        await _sut.StartAsync("P001", "1.2.3");
        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await _sut.TransitionAsync(WorkflowState.ReadyToExpose);
        await _sut.PrepareExposureAsync(MakeParams()); // sets Emergency

        var result = await _sut.TransitionAsync(WorkflowState.ImageAcquiring);

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Contain("EMERGENCY");
    }

    // ── AbortAsync – generator abort failure escalates to emergency ──────────

    [Fact]
    public async Task AbortAsync_GeneratorAbortFails_EscalatesToEmergency()
    {
        _generator.AbortAsync(Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Generator hardware failure"));

        await _sut.StartAsync("P001", "1.2.3");
        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await _sut.TransitionAsync(WorkflowState.ReadyToExpose);
        await _sut.TransitionAsync(WorkflowState.Exposing);

        await _sut.AbortAsync("Hardware fault");

        _sut.CurrentState.Should().Be(WorkflowState.Error);
        _sut.CurrentSafeState.Should().Be(SafeState.Emergency);
    }

    [Fact]
    public async Task AbortAsync_FromReadyToExpose_CallsGeneratorAbort()
    {
        _generator.AbortAsync(Arg.Any<CancellationToken>()).Returns(Result.Success());

        await _sut.StartAsync("P001", "1.2.3");
        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await _sut.TransitionAsync(WorkflowState.ReadyToExpose);

        await _sut.AbortAsync("User abort from ReadyToExpose");

        await _generator.Received(1).AbortAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AbortAsync_FromImageAcquiring_CallsGeneratorAbort()
    {
        _generator.AbortAsync(Arg.Any<CancellationToken>()).Returns(Result.Success());

        await _sut.StartAsync("P001", "1.2.3");
        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await _sut.TransitionAsync(WorkflowState.ReadyToExpose);
        await _sut.TransitionAsync(WorkflowState.Exposing);
        await _sut.TransitionAsync(WorkflowState.ImageAcquiring);

        await _sut.AbortAsync("Abort from ImageAcquiring");

        await _generator.Received(1).AbortAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AbortAsync_FromPatientSelected_DoesNotCallGeneratorAbort()
    {
        await _sut.StartAsync("P001", "1.2.3");

        await _sut.AbortAsync("User cancel");

        await _generator.DidNotReceive().AbortAsync(Arg.Any<CancellationToken>());
    }

    // ── PrepareExposureAsync – with detector ──────────────────────────────────

    [Fact]
    public async Task PrepareExposureAsync_WithDetector_ArmsDetector()
    {
        var allowValidation = new DoseValidationResult(
            IsAllowed: true, Level: DoseValidationLevel.Allow,
            Message: null, EstimatedDap: 0.5, EstimatedEsd: 0.5, ExposureIndex: 0.5);
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(allowValidation));
        _detector.ArmAsync(Arg.Any<DetectorTriggerMode>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = new WorkflowEngine(_doseService, _generator, _securityContext, null, _detector);
        await sut.StartAsync("P001", "1.2.3");
        await sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await sut.TransitionAsync(WorkflowState.ReadyToExpose);

        var result = await sut.PrepareExposureAsync(MakeParams());

        result.IsSuccess.Should().BeTrue();
        await _detector.Received(1).ArmAsync(DetectorTriggerMode.Sync, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PrepareExposureAsync_DetectorArmFails_ReturnsDetectorNotReady()
    {
        var allowValidation = new DoseValidationResult(
            IsAllowed: true, Level: DoseValidationLevel.Allow,
            Message: null, EstimatedDap: 0.5, EstimatedEsd: 0.5, ExposureIndex: 0.5);
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(allowValidation));
        _detector.ArmAsync(Arg.Any<DetectorTriggerMode>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DetectorNotReady, "Detector offline"));

        var sut = new WorkflowEngine(_doseService, _generator, _securityContext, null, _detector);
        await sut.StartAsync("P001", "1.2.3");
        await sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await sut.TransitionAsync(WorkflowState.ReadyToExpose);

        var result = await sut.PrepareExposureAsync(MakeParams());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DetectorNotReady);
        sut.CurrentState.Should().Be(WorkflowState.Error);
    }

    // ── PrepareExposureAsync – RBAC with Radiologist ──────────────────────────

    [Fact]
    public async Task PrepareExposureAsync_Radiologist_Succeeds()
    {
        _securityContext.CurrentRole.Returns(UserRole.Radiologist);
        var allowValidation = new DoseValidationResult(
            IsAllowed: true, Level: DoseValidationLevel.Allow,
            Message: null, EstimatedDap: 0.5, EstimatedEsd: 0.5, ExposureIndex: 0.5);
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(allowValidation));

        await _sut.StartAsync("P001", "1.2.3");
        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await _sut.TransitionAsync(WorkflowState.ReadyToExpose);

        var result = await _sut.PrepareExposureAsync(MakeParams());

        result.IsSuccess.Should().BeTrue();
    }

    // ── StartEmergencyExposureAsync ───────────────────────────────────────────

    [Fact]
    public async Task StartEmergencyExposureAsync_AllowDose_TransitionsToExposing()
    {
        var allowValidation = new DoseValidationResult(
            IsAllowed: true, Level: DoseValidationLevel.Allow,
            Message: null, EstimatedDap: 0.5, EstimatedEsd: 0.5, ExposureIndex: 0.5);
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(allowValidation));

        var result = await _sut.StartEmergencyExposureAsync("Trauma Patient", MakeParams());

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(WorkflowState.Exposing);
    }

    [Fact]
    public async Task StartEmergencyExposureAsync_Unauthenticated_ReturnsAuthenticationFailed()
    {
        _securityContext.CurrentRole.Returns((UserRole?)null);

        var result = await _sut.StartEmergencyExposureAsync("Patient", MakeParams());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AuthenticationFailed);
    }

    [Fact]
    public async Task StartEmergencyExposureAsync_InsufficientRole_ReturnsFailure()
    {
        _securityContext.CurrentRole.Returns(UserRole.Admin); // Admin cannot perform emergency exposure

        var result = await _sut.StartEmergencyExposureAsync("Patient", MakeParams());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InsufficientPermission);
    }

    [Fact]
    public async Task StartEmergencyExposureAsync_EmergencyDose_SetsEmergencyStateAndReturnsFailure()
    {
        var emergencyValidation = new DoseValidationResult(
            IsAllowed: false, Level: DoseValidationLevel.Emergency,
            Message: "Critical dose", EstimatedDap: 9.9, EstimatedEsd: 9.9, ExposureIndex: 9.9);
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(emergencyValidation));

        var result = await _sut.StartEmergencyExposureAsync("Trauma", MakeParams());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DoseInterlock);
        _sut.CurrentSafeState.Should().Be(SafeState.Emergency);
    }

    [Fact]
    public async Task StartEmergencyExposureAsync_BlockDose_SetsBlockedStateAndReturnsFailure()
    {
        var blockValidation = new DoseValidationResult(
            IsAllowed: false, Level: DoseValidationLevel.Block,
            Message: "Blocked", EstimatedDap: 5.0, EstimatedEsd: 5.0, ExposureIndex: 5.0);
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(blockValidation));

        var result = await _sut.StartEmergencyExposureAsync("Trauma", MakeParams());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DoseInterlock);
        _sut.CurrentSafeState.Should().Be(SafeState.Blocked);
    }

    [Fact]
    public async Task StartEmergencyExposureAsync_WarnDose_SucceedsWithWarningSafeState()
    {
        var warnValidation = new DoseValidationResult(
            IsAllowed: true, Level: DoseValidationLevel.Warn,
            Message: "DRL exceeded", EstimatedDap: 1.5, EstimatedEsd: 1.5, ExposureIndex: 1.5);
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(warnValidation));

        var result = await _sut.StartEmergencyExposureAsync("Trauma", MakeParams());

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
        _sut.CurrentSafeState.Should().Be(SafeState.Warning);
    }

    [Fact]
    public async Task StartEmergencyExposureAsync_NullParameters_ThrowsArgumentNull()
    {
        var act = async () => await _sut.StartEmergencyExposureAsync("Patient", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task StartEmergencyExposureAsync_FromErrorState_ResetsAndTransitionsToExposing()
    {
        // Arrange: put engine in Error state first
        await _sut.AbortAsync("Initial abort");
        _sut.CurrentState.Should().Be(WorkflowState.Error);

        var allowValidation = new DoseValidationResult(
            IsAllowed: true, Level: DoseValidationLevel.Allow,
            Message: null, EstimatedDap: 0.5, EstimatedEsd: 0.5, ExposureIndex: 0.5);
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(allowValidation));

        var result = await _sut.StartEmergencyExposureAsync("Trauma", MakeParams());

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(WorkflowState.Exposing);
    }

    [Fact]
    public async Task StartEmergencyExposureAsync_WhenEmergencySafeState_ReturnsFailure()
    {
        var emergencyValidation = new DoseValidationResult(
            IsAllowed: false, Level: DoseValidationLevel.Emergency,
            Message: "Critical", EstimatedDap: 9.9, EstimatedEsd: 9.9, ExposureIndex: 9.9);
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(emergencyValidation));

        // First call sets Emergency
        await _sut.StartAsync("P001", "1.2.3");
        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);
        await _sut.TransitionAsync(WorkflowState.ReadyToExpose);
        await _sut.PrepareExposureAsync(MakeParams());
        _sut.CurrentSafeState.Should().Be(SafeState.Emergency);

        // Second call should be blocked by Emergency safe state
        var result = await _sut.StartEmergencyExposureAsync("Trauma", MakeParams());

        result.IsFailure.Should().BeTrue();
    }

    // ── StateChanged event firing ─────────────────────────────────────────────

    [Fact]
    public async Task StartEmergencyExposureAsync_Succeeds_RaisesStateChangedEvent()
    {
        var allowValidation = new DoseValidationResult(
            IsAllowed: true, Level: DoseValidationLevel.Allow,
            Message: null, EstimatedDap: 0.5, EstimatedEsd: 0.5, ExposureIndex: 0.5);
        _doseService.ValidateExposureAsync(Arg.Any<ExposureParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(allowValidation));

        WorkflowStateChangedEventArgs? eventArgs = null;
        _sut.StateChanged += (_, e) => eventArgs = e;

        await _sut.StartEmergencyExposureAsync("Trauma", MakeParams());

        eventArgs.Should().NotBeNull();
        eventArgs!.NewState.Should().Be(WorkflowState.Exposing);
    }

    [Fact]
    public async Task TransitionAsync_ValidStateChange_RaisesEventWithCorrectPreviousState()
    {
        await _sut.StartAsync("P001", "1.2.3");
        WorkflowStateChangedEventArgs? eventArgs = null;
        _sut.StateChanged += (_, e) => eventArgs = e;

        await _sut.TransitionAsync(WorkflowState.ProtocolLoaded);

        eventArgs.Should().NotBeNull();
        eventArgs!.PreviousState.Should().Be(WorkflowState.PatientSelected);
        eventArgs.NewState.Should().Be(WorkflowState.ProtocolLoaded);
    }
}
