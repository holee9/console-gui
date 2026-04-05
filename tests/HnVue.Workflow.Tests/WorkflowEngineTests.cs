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
}
