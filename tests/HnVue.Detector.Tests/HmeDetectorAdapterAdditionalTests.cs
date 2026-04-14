using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Detector.ThirdParty.Hme;
using Xunit;

namespace HnVue.Detector.Tests;

/// <summary>
/// Additional coverage tests for HmeDetectorAdapter edge cases and state transitions.
/// </summary>
[Trait("SWR", "SWR-DT-063")]
public sealed class HmeDetectorAdapterAdditionalTests
{
    private static readonly HmeDetectorConfig DefaultConfig = new("192.168.1.100");

    // ── GetStatusAsync with various states ──────────────────────────────────────

    [Fact]
    public async Task GetStatusAsync_AfterConnectAndArm_ReturnsArmedState()
    {
        using var adapter = new HmeDetectorAdapter(DefaultConfig);
        await adapter.ConnectAsync();
        await adapter.ArmAsync();

        var result = await adapter.GetStatusAsync();

        result.Value.State.Should().Be(DetectorState.Armed);
        result.Value.IsReadyToArm.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatusAsync_AfterAbort_ReturnsIdleState()
    {
        using var adapter = new HmeDetectorAdapter(DefaultConfig);
        await adapter.ConnectAsync();
        await adapter.ArmAsync();
        await adapter.AbortAsync();

        var result = await adapter.GetStatusAsync();

        result.Value.State.Should().Be(DetectorState.Idle);
        result.Value.IsReadyToArm.Should().BeTrue();
    }

    // ── Lifecycle edge cases ────────────────────────────────────────────────────

    [Fact]
    public async Task Arm_ThenDisconnect_ReturnsDisconnected()
    {
        using var adapter = new HmeDetectorAdapter(DefaultConfig);
        await adapter.ConnectAsync();
        await adapter.ArmAsync();

        var result = await adapter.DisconnectAsync();

        result.IsSuccess.Should().BeTrue();
        adapter.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    [Fact]
    public async Task Connect_Abort_Disconnect_FullSequence()
    {
        using var adapter = new HmeDetectorAdapter(DefaultConfig);

        await adapter.ConnectAsync();
        adapter.CurrentState.Should().Be(DetectorState.Idle);

        await adapter.AbortAsync();
        adapter.CurrentState.Should().Be(DetectorState.Idle);

        await adapter.DisconnectAsync();
        adapter.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    // ── Dispose edge cases ──────────────────────────────────────────────────────

    [Fact]
    public async Task Dispose_AfterConnect_PreventsReuse()
    {
        var adapter = new HmeDetectorAdapter(DefaultConfig);
        await adapter.ConnectAsync();

        adapter.Dispose();

        // State should still be readable
        adapter.CurrentState.Should().Be(DetectorState.Idle);
    }

    [Fact]
    public void Dispose_InUsingBlock_CleansUp()
    {
        var config = new HmeDetectorConfig("10.0.0.1", Port: 9999, Model: "S4343-WA");
        using (var adapter = new HmeDetectorAdapter(config))
        {
            adapter.CurrentState.Should().Be(DetectorState.Disconnected);
        }
        // Adapter disposed — no exception
    }

    // ── Config variations ───────────────────────────────────────────────────────

    [Theory]
    [InlineData("S4335-WA")]
    [InlineData("S4335-WF")]
    [InlineData("S4343-WA")]
    public async Task GetStatusAsync_DifferentModels_ReturnsCorrectSerialNumber(string model)
    {
        var config = new HmeDetectorConfig("192.168.1.100", Model: model);
        using var adapter = new HmeDetectorAdapter(config);

        var result = await adapter.GetStatusAsync();

        result.Value.SerialNumber.Should().Be(model);
    }

    [Fact]
    public async Task GetStatusAsync_CustomPort_StillReturnsStatus()
    {
        var config = new HmeDetectorConfig("10.0.0.1", Port: 9999);
        using var adapter = new HmeDetectorAdapter(config);

        var result = await adapter.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
    }

    // ── StateChanged event with reason ──────────────────────────────────────────

    [Fact]
    public async Task AbortAsync_RaisesEventWithReason()
    {
        using var adapter = new HmeDetectorAdapter(DefaultConfig);
        await adapter.ConnectAsync();
        await adapter.ArmAsync();

        DetectorStateChangedEventArgs? capturedEvent = null;
        adapter.StateChanged += (_, e) => capturedEvent = e;

        await adapter.AbortAsync();

        capturedEvent.Should().NotBeNull();
        capturedEvent!.Reason.Should().Be("Abort requested");
        capturedEvent.PreviousState.Should().Be(DetectorState.Armed);
        capturedEvent.NewState.Should().Be(DetectorState.Idle);
    }

    // ── Multiple state transitions tracking ─────────────────────────────────────

    [Fact]
    public async Task FullLifecycle_TracksAllStateChanges()
    {
        using var adapter = new HmeDetectorAdapter(DefaultConfig);
        var transitions = new List<DetectorStateChangedEventArgs>();
        adapter.StateChanged += (_, e) => transitions.Add(e);

        await adapter.ConnectAsync();
        await adapter.ArmAsync();
        await adapter.AbortAsync();
        await adapter.ArmAsync();
        await adapter.DisconnectAsync();

        transitions.Should().HaveCount(5);
        transitions[0].NewState.Should().Be(DetectorState.Idle);       // Connect
        transitions[1].NewState.Should().Be(DetectorState.Armed);      // Arm
        transitions[2].NewState.Should().Be(DetectorState.Idle);       // Abort
        transitions[3].NewState.Should().Be(DetectorState.Armed);      // Arm again
        transitions[4].NewState.Should().Be(DetectorState.Disconnected); // Disconnect
    }
}
