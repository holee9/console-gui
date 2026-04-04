using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Results;
using HnVue.Workflow;
using Xunit;

namespace HnVue.Workflow.Tests;

[Trait("SWR", "SWR-WF-010")]
public sealed class WorkflowStateMachineTests
{
    private static WorkflowStateMachine CreateSut() => new();

    // ── Initial state ─────────────────────────────────────────────────────────

    [Fact]
    public void InitialState_IsIdle()
    {
        var sut = CreateSut();

        sut.CurrentState.Should().Be(WorkflowState.Idle);
    }

    // ── Happy path forward transitions ────────────────────────────────────────

    [Theory]
    [InlineData(WorkflowState.Idle,          WorkflowState.PatientSelected)]
    [InlineData(WorkflowState.PatientSelected, WorkflowState.ProtocolLoaded)]
    [InlineData(WorkflowState.ProtocolLoaded, WorkflowState.ReadyToExpose)]
    [InlineData(WorkflowState.ReadyToExpose, WorkflowState.Exposing)]
    [InlineData(WorkflowState.Exposing,      WorkflowState.ImageAcquiring)]
    [InlineData(WorkflowState.ImageAcquiring, WorkflowState.ImageProcessing)]
    [InlineData(WorkflowState.ImageProcessing, WorkflowState.ImageReview)]
    [InlineData(WorkflowState.ImageReview,   WorkflowState.Completed)]
    [InlineData(WorkflowState.Completed,     WorkflowState.Idle)]
    public void TryTransition_ValidForwardPath_Succeeds(WorkflowState from, WorkflowState to)
    {
        var sut = CreateSut();
        AdvanceTo(sut, from);

        var result = sut.TryTransition(to);

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(to);
    }

    // ── Abort path: any state → Error ─────────────────────────────────────────

    [Theory]
    [InlineData(WorkflowState.Idle)]
    [InlineData(WorkflowState.PatientSelected)]
    [InlineData(WorkflowState.ProtocolLoaded)]
    [InlineData(WorkflowState.ReadyToExpose)]
    [InlineData(WorkflowState.Exposing)]
    [InlineData(WorkflowState.ImageAcquiring)]
    [InlineData(WorkflowState.ImageProcessing)]
    [InlineData(WorkflowState.ImageReview)]
    [InlineData(WorkflowState.Completed)]
    public void TryTransition_AnyStateToError_Succeeds(WorkflowState from)
    {
        var sut = CreateSut();
        AdvanceTo(sut, from);

        var result = sut.TryTransition(WorkflowState.Error);

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(WorkflowState.Error);
    }

    // ── Error reset ───────────────────────────────────────────────────────────

    [Fact]
    public void TryTransition_ErrorToIdle_Succeeds()
    {
        var sut = CreateSut();
        sut.ForceError();

        var result = sut.TryTransition(WorkflowState.Idle);

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(WorkflowState.Idle);
    }

    // ── Invalid transitions ───────────────────────────────────────────────────

    [Fact]
    public void TryTransition_IdleToExposing_Fails()
    {
        var sut = CreateSut();

        var result = sut.TryTransition(WorkflowState.Exposing);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InvalidStateTransition);
        sut.CurrentState.Should().Be(WorkflowState.Idle); // Unchanged
    }

    [Fact]
    public void TryTransition_ExposingToIdle_Fails()
    {
        var sut = CreateSut();
        AdvanceTo(sut, WorkflowState.Exposing);

        var result = sut.TryTransition(WorkflowState.Idle);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void TryTransition_CompletedToExposing_Fails()
    {
        var sut = CreateSut();
        AdvanceTo(sut, WorkflowState.Completed);

        var result = sut.TryTransition(WorkflowState.Exposing);

        result.IsFailure.Should().BeTrue();
    }

    // ── ForceError ────────────────────────────────────────────────────────────

    [Fact]
    public void ForceError_FromAnyState_SetsErrorState()
    {
        var sut = CreateSut();
        AdvanceTo(sut, WorkflowState.ImageProcessing);

        sut.ForceError();

        sut.CurrentState.Should().Be(WorkflowState.Error);
    }

    // ── Reset ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Reset_FromError_ReturnsToIdle()
    {
        var sut = CreateSut();
        sut.ForceError();

        var result = sut.Reset();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(WorkflowState.Idle);
    }

    [Fact]
    public void Reset_FromCompleted_ReturnsToIdle()
    {
        var sut = CreateSut();
        AdvanceTo(sut, WorkflowState.Completed);

        var result = sut.Reset();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(WorkflowState.Idle);
    }

    [Fact]
    public void Reset_FromPatientSelected_Fails()
    {
        var sut = CreateSut();
        AdvanceTo(sut, WorkflowState.PatientSelected);

        var result = sut.Reset();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InvalidStateTransition);
    }

    // ── CanTransitionTo ───────────────────────────────────────────────────────

    [Fact]
    public void CanTransitionTo_ValidTarget_ReturnsTrue()
    {
        var sut = CreateSut();

        var result = sut.CanTransitionTo(WorkflowState.PatientSelected);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanTransitionTo_InvalidTarget_ReturnsFalse()
    {
        var sut = CreateSut();

        var result = sut.CanTransitionTo(WorkflowState.ImageProcessing);

        result.Should().BeFalse();
    }

    // ── GetAllowedTransitions ─────────────────────────────────────────────────

    [Fact]
    public void GetAllowedTransitions_IdleState_ContainsPatientSelectedAndError()
    {
        var sut = CreateSut();

        var transitions = sut.GetAllowedTransitions();

        transitions.Should().Contain(WorkflowState.PatientSelected);
        transitions.Should().Contain(WorkflowState.Error);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static readonly WorkflowState[] ForwardPath =
    [
        WorkflowState.PatientSelected,
        WorkflowState.ProtocolLoaded,
        WorkflowState.ReadyToExpose,
        WorkflowState.Exposing,
        WorkflowState.ImageAcquiring,
        WorkflowState.ImageProcessing,
        WorkflowState.ImageReview,
        WorkflowState.Completed,
    ];

    private static void AdvanceTo(WorkflowStateMachine sut, WorkflowState target)
    {
        if (target == WorkflowState.Idle)
            return;

        foreach (var state in ForwardPath)
        {
            sut.TryTransition(state);
            if (state == target)
                return;
        }
    }
}
