using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Workflow;
using Xunit;

namespace HnVue.Workflow.Tests;

[Trait("SWR", "SWR-WF-030")]
public sealed class GeneratorSimulatorTests
{
    private static GeneratorSimulator CreateFastSut() => new()
    {
        PrepareDelayMs = 1,   // Fast delays for tests
        ExposureDelayMs = 1,
    };

    private static ExposureParameters MakeParams() =>
        new("CHEST", Kvp: 80, Mas: 10, "1.2.3.4");

    // ── Initial state ─────────────────────────────────────────────────────────

    [Fact]
    public void InitialState_IsDisconnected()
    {
        var sut = CreateFastSut();

        sut.CurrentState.Should().Be(GeneratorState.Disconnected);
    }

    // ── ConnectAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Connect_Succeeds_TransitionsToIdle()
    {
        var sut = CreateFastSut();

        var result = await sut.ConnectAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Idle);
    }

    [Fact]
    public async Task Connect_WithFailureInjected_ReturnsFailure()
    {
        var sut = CreateFastSut();
        sut.FailNextConnectWith = "Simulated connection failure";

        var result = await sut.ConnectAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
    }

    [Fact]
    public async Task Connect_RaisesStateChangedEvent()
    {
        var sut = CreateFastSut();
        GeneratorStateChangedEventArgs? args = null;
        sut.StateChanged += (_, e) => args = e;

        await sut.ConnectAsync();

        args.Should().NotBeNull();
        args!.NewState.Should().Be(GeneratorState.Idle);
    }

    // ── DisconnectAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task Disconnect_FromIdle_TransitionsToDisconnected()
    {
        var sut = CreateFastSut();
        await sut.ConnectAsync();

        var result = await sut.DisconnectAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Disconnected);
    }

    // ── PrepareAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Prepare_FromIdle_TransitionsToReadyAfterDelay()
    {
        var sut = CreateFastSut();
        await sut.ConnectAsync();

        var result = await sut.PrepareAsync(MakeParams());

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Ready);
    }

    [Fact]
    public async Task Prepare_NullParameters_ThrowsArgumentNullException()
    {
        var sut = CreateFastSut();
        await sut.ConnectAsync();

        var act = async () => await sut.PrepareAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Prepare_WhenNotIdle_ReturnsGeneratorNotReady()
    {
        var sut = CreateFastSut();
        // Remain disconnected — not Idle

        var result = await sut.PrepareAsync(MakeParams());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
    }

    [Fact]
    public async Task Prepare_RaisesPreparingThenReadyStateChanges()
    {
        var sut = CreateFastSut();
        await sut.ConnectAsync();
        var states = new List<GeneratorState>();
        sut.StateChanged += (_, e) => states.Add(e.NewState);

        await sut.PrepareAsync(MakeParams());

        states.Should().ContainInOrder(GeneratorState.Preparing, GeneratorState.Ready);
    }

    // ── TriggerExposureAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task TriggerExposure_WhenReady_CompletesAndReturnsToIdle()
    {
        var sut = CreateFastSut();
        await sut.ConnectAsync();
        await sut.PrepareAsync(MakeParams());

        var result = await sut.TriggerExposureAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Idle);
    }

    [Fact]
    public async Task TriggerExposure_WithFailureInjected_ReturnsExposureAborted()
    {
        var sut = CreateFastSut();
        await sut.ConnectAsync();
        await sut.PrepareAsync(MakeParams());
        sut.FailNextExposureWith = "Simulated exposure failure";

        var result = await sut.TriggerExposureAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ExposureAborted);
        sut.CurrentState.Should().Be(GeneratorState.Error);
    }

    [Fact]
    public async Task TriggerExposure_WhenNotReady_ReturnsGeneratorNotReady()
    {
        var sut = CreateFastSut();
        await sut.ConnectAsync(); // Idle, not Ready

        var result = await sut.TriggerExposureAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
    }

    // ── AbortAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Abort_FromAnyState_TransitionsToError()
    {
        var sut = CreateFastSut();
        await sut.ConnectAsync();

        var result = await sut.AbortAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Error);
    }

    // ── GetStatusAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStatus_AfterConnect_ReturnsIdleStatus()
    {
        var sut = CreateFastSut();
        await sut.ConnectAsync();

        var result = await sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(GeneratorState.Idle);
        result.Value.IsReadyToExpose.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatus_AfterPrepare_ReturnsReadyStatus()
    {
        var sut = CreateFastSut();
        await sut.ConnectAsync();
        await sut.PrepareAsync(MakeParams());

        var result = await sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.IsReadyToExpose.Should().BeTrue();
    }

    [Fact]
    public async Task GetStatus_AfterMultipleExposures_AccumulatesHeatUnits()
    {
        var sut = CreateFastSut();
        await sut.ConnectAsync();
        await sut.PrepareAsync(MakeParams());
        await sut.TriggerExposureAsync();
        await sut.PrepareAsync(MakeParams());
        await sut.TriggerExposureAsync();

        var status = await sut.GetStatusAsync();

        status.Value.HeatUnitPercentage.Should().BeGreaterThan(0);
    }
}
