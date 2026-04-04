using FluentAssertions;
using HnVue.Common.Abstractions;
using Xunit;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.UI.ViewModels;
using NSubstitute;

namespace HnVue.UI.Tests;

/// <summary>
/// Tests for <see cref="WorkflowViewModel"/>.
/// </summary>
public sealed class WorkflowViewModelTests
{
    private readonly IWorkflowEngine _workflowEngine = Substitute.For<IWorkflowEngine>();
    private readonly ISecurityContext _securityContext = Substitute.For<ISecurityContext>();

    private WorkflowViewModel CreateSut()
    {
        _workflowEngine.CurrentState.Returns(WorkflowState.Idle);
        _workflowEngine.CurrentSafeState.Returns(SafeState.Idle);
        return new WorkflowViewModel(_workflowEngine, _securityContext);
    }

    [Fact]
    public void Constructor_SetsCurrentStateFromEngine()
    {
        _workflowEngine.CurrentState.Returns(WorkflowState.PatientSelected);
        _workflowEngine.CurrentSafeState.Returns(SafeState.Idle);

        var sut = new WorkflowViewModel(_workflowEngine, _securityContext);

        sut.CurrentState.Should().Be(WorkflowState.PatientSelected.ToString());
    }

    [Fact]
    public void TriggerExposureCommand_CannotExecute_WhenNotReadyToExpose()
    {
        _workflowEngine.CurrentState.Returns(WorkflowState.Idle);
        _workflowEngine.CurrentSafeState.Returns(SafeState.Idle);
        _securityContext.HasRole(UserRole.Radiographer).Returns(true);

        var sut = new WorkflowViewModel(_workflowEngine, _securityContext);

        sut.TriggerExposureCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void TriggerExposureCommand_CannotExecute_WhenReadyButUserIsAdmin()
    {
        // Admin role cannot trigger exposure.
        _workflowEngine.CurrentState.Returns(WorkflowState.Idle);
        _workflowEngine.CurrentSafeState.Returns(SafeState.Idle);

        _securityContext.HasRole(UserRole.Radiographer).Returns(false);
        _securityContext.HasRole(UserRole.Radiologist).Returns(false);
        _securityContext.HasRole(UserRole.Admin).Returns(true);

        var sut = new WorkflowViewModel(_workflowEngine, _securityContext);
        // Simulate state changed to ReadyToExpose.
        _workflowEngine.CurrentState.Returns(WorkflowState.ReadyToExpose);
        sut.GetType()
            .GetMethod("OnWorkflowStateChanged",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(sut, [null, new WorkflowStateChangedEventArgs(WorkflowState.Idle, WorkflowState.ReadyToExpose)]);

        sut.TriggerExposureCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void TriggerExposureCommand_CanExecute_WhenReadyAndUserIsRadiographer()
    {
        _workflowEngine.CurrentState.Returns(WorkflowState.Idle);
        _workflowEngine.CurrentSafeState.Returns(SafeState.Idle);
        _securityContext.HasRole(UserRole.Radiographer).Returns(true);

        var sut = new WorkflowViewModel(_workflowEngine, _securityContext);
        _workflowEngine.CurrentState.Returns(WorkflowState.ReadyToExpose);
        RaiseStateChanged(sut, WorkflowState.Idle, WorkflowState.ReadyToExpose);

        sut.TriggerExposureCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void TriggerExposureCommand_CanExecute_WhenReadyAndUserIsRadiologist()
    {
        _workflowEngine.CurrentState.Returns(WorkflowState.Idle);
        _workflowEngine.CurrentSafeState.Returns(SafeState.Idle);
        _securityContext.HasRole(UserRole.Radiographer).Returns(false);
        _securityContext.HasRole(UserRole.Radiologist).Returns(true);

        var sut = new WorkflowViewModel(_workflowEngine, _securityContext);
        _workflowEngine.CurrentState.Returns(WorkflowState.ReadyToExpose);
        RaiseStateChanged(sut, WorkflowState.Idle, WorkflowState.ReadyToExpose);

        sut.TriggerExposureCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task PrepareExposureCommand_CallsTransitionAsync()
    {
        _workflowEngine.TransitionAsync(Arg.Any<WorkflowState>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = CreateSut();
        await sut.PrepareExposureCommand.ExecuteAsync(null);

        await _workflowEngine.Received(1)
            .TransitionAsync(WorkflowState.ReadyToExpose, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AbortCommand_CallsAbortAsync()
    {
        _workflowEngine.AbortAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = CreateSut();
        await sut.AbortCommand.ExecuteAsync(null);

        await _workflowEngine.Received(1).AbortAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void StateChanged_Event_UpdatesCurrentState()
    {
        var sut = CreateSut();

        _workflowEngine.CurrentState.Returns(WorkflowState.Exposing);
        RaiseStateChanged(sut, WorkflowState.ReadyToExpose, WorkflowState.Exposing);

        sut.CurrentState.Should().Be(WorkflowState.Exposing.ToString());
    }

    [Fact]
    public void Dispose_UnsubscribesFromStateChanged()
    {
        var sut = CreateSut();
        sut.Dispose();

        // After dispose, raising StateChanged should not throw.
        var act = () => _workflowEngine.StateChanged +=
            Raise.EventWith(new WorkflowStateChangedEventArgs(WorkflowState.Idle, WorkflowState.Error));
        act.Should().NotThrow();
    }

    private static void RaiseStateChanged(WorkflowViewModel sut, WorkflowState from, WorkflowState to)
    {
        sut.GetType()
            .GetMethod("OnWorkflowStateChanged",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(sut, [null, new WorkflowStateChangedEventArgs(from, to)]);
    }
}
