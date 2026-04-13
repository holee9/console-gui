using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Detector.ThirdParty.Hme;
using Xunit;

namespace HnVue.Detector.Tests;

[Trait("SWR", "SWR-DT-060")]
public sealed class HmeDetectorAdapterTests
{
    private static readonly HmeDetectorConfig DefaultConfig = new("192.168.1.100");

    private readonly HmeDetectorAdapter _sut = new(DefaultConfig);

    // ── Constructor ────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullConfig_ThrowsArgumentNullException()
    {
        var act = () => new HmeDetectorAdapter(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public void Constructor_ValidConfig_InitialStateIsDisconnected()
    {
        using var adapter = new HmeDetectorAdapter(DefaultConfig);

        adapter.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    // ── ConnectAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ConnectAsync_TransitionsToIdle()
    {
        var result = await _sut.ConnectAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(DetectorState.Idle);
    }

    [Fact]
    public async Task ConnectAsync_RaisesStateChangedEvent()
    {
        var events = new List<DetectorStateChangedEventArgs>();
        _sut.StateChanged += (_, e) => events.Add(e);

        await _sut.ConnectAsync();

        events.Should().ContainSingle();
        events[0].PreviousState.Should().Be(DetectorState.Disconnected);
        events[0].NewState.Should().Be(DetectorState.Idle);
    }

    [Fact]
    public async Task ConnectAsync_CalledTwice_DoesNotRaiseEventSecondTime()
    {
        var events = new List<DetectorStateChangedEventArgs>();
        _sut.StateChanged += (_, e) => events.Add(e);

        await _sut.ConnectAsync();
        await _sut.ConnectAsync();

        // Second connect transitions to same state (Idle) — event should not fire
        events.Should().HaveCount(1);
    }

    // ── DisconnectAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task DisconnectAsync_FromIdle_TransitionsToDisconnected()
    {
        await _sut.ConnectAsync();

        var result = await _sut.DisconnectAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    [Fact]
    public async Task DisconnectAsync_RaisesStateChangedEvent()
    {
        await _sut.ConnectAsync();
        var events = new List<DetectorStateChangedEventArgs>();
        _sut.StateChanged += (_, e) => events.Add(e);

        await _sut.DisconnectAsync();

        events.Should().ContainSingle();
        events[0].PreviousState.Should().Be(DetectorState.Idle);
        events[0].NewState.Should().Be(DetectorState.Disconnected);
    }

    // ── ArmAsync ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task ArmAsync_FromIdle_TransitionsToArmed()
    {
        await _sut.ConnectAsync();

        var result = await _sut.ArmAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(DetectorState.Armed);
    }

    [Fact]
    public async Task ArmAsync_RaisesStateChangedEvent()
    {
        await _sut.ConnectAsync();
        var events = new List<DetectorStateChangedEventArgs>();
        _sut.StateChanged += (_, e) => events.Add(e);

        await _sut.ArmAsync();

        events.Should().ContainSingle();
        events[0].PreviousState.Should().Be(DetectorState.Idle);
        events[0].NewState.Should().Be(DetectorState.Armed);
    }

    [Fact]
    public async Task ArmAsync_WithSyncTriggerMode_Succeeds()
    {
        await _sut.ConnectAsync();

        var result = await _sut.ArmAsync(DetectorTriggerMode.Sync);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ArmAsync_WithFreeRunTriggerMode_Succeeds()
    {
        await _sut.ConnectAsync();

        var result = await _sut.ArmAsync(DetectorTriggerMode.FreeRun);

        result.IsSuccess.Should().BeTrue();
    }

    // ── AbortAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task AbortAsync_FromArmed_TransitionsToIdle()
    {
        await _sut.ConnectAsync();
        await _sut.ArmAsync();

        var result = await _sut.AbortAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(DetectorState.Idle);
    }

    [Fact]
    public async Task AbortAsync_RaisesStateChangedEvent_WithReason()
    {
        await _sut.ConnectAsync();
        await _sut.ArmAsync();
        var events = new List<DetectorStateChangedEventArgs>();
        _sut.StateChanged += (_, e) => events.Add(e);

        await _sut.AbortAsync();

        events.Should().ContainSingle();
        events[0].Reason.Should().Be("Abort requested");
    }

    [Fact]
    public async Task AbortAsync_FromDisconnected_TransitionsToIdle()
    {
        var result = await _sut.AbortAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(DetectorState.Idle);
    }

    // ── GetStatusAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStatusAsync_WhenDisconnected_ReturnsDisconnectedState()
    {
        var result = await _sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(DetectorState.Disconnected);
        result.Value.IsReadyToArm.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatusAsync_WhenIdle_ReturnsReadyToArm()
    {
        await _sut.ConnectAsync();

        var result = await _sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(DetectorState.Idle);
        result.Value.IsReadyToArm.Should().BeTrue();
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsConfigModelAsSerialNumber()
    {
        var config = new HmeDetectorConfig("10.0.0.1", Model: "S4343-WA");
        using var adapter = new HmeDetectorAdapter(config);

        var result = await adapter.GetStatusAsync();

        result.Value.SerialNumber.Should().Be("S4343-WA");
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsHmeFirmwareVersion()
    {
        var result = await _sut.GetStatusAsync();

        result.Value.FirmwareVersion.Should().Be("HME-libxd2");
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsValidTimestamp()
    {
        var before = DateTimeOffset.UtcNow;

        var result = await _sut.GetStatusAsync();

        result.Value.Timestamp.Should().BeOnOrAfter(before);
        result.Value.Timestamp.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsZeroTemperature()
    {
        var result = await _sut.GetStatusAsync();

        result.Value.TemperatureCelsius.Should().Be(0.0);
    }

    [Fact]
    public async Task GetStatusAsync_WhenArmed_ReturnsNotReadyToArm()
    {
        await _sut.ConnectAsync();
        await _sut.ArmAsync();

        var result = await _sut.GetStatusAsync();

        result.Value.IsReadyToArm.Should().BeFalse();
    }

    // ── Dispose ────────────────────────────────────────────────────────────────

    [Fact]
    public void Dispose_CalledOnce_DoesNotThrow()
    {
        var act = () => _sut.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        _sut.Dispose();

        var act = () => _sut.Dispose();

        act.Should().NotThrow();
    }

    // ── Full lifecycle ─────────────────────────────────────────────────────────

    [Fact]
    public async Task FullLifecycle_ConnectArmAbortDisconnect()
    {
        var events = new List<DetectorStateChangedEventArgs>();
        _sut.StateChanged += (_, e) => events.Add(e);

        await _sut.ConnectAsync();
        _sut.CurrentState.Should().Be(DetectorState.Idle);

        await _sut.ArmAsync();
        _sut.CurrentState.Should().Be(DetectorState.Armed);

        await _sut.AbortAsync();
        _sut.CurrentState.Should().Be(DetectorState.Idle);

        await _sut.DisconnectAsync();
        _sut.CurrentState.Should().Be(DetectorState.Disconnected);

        events.Should().HaveCount(4);
    }

    [Fact]
    public async Task FullLifecycle_ConnectArmDisconnect()
    {
        await _sut.ConnectAsync();
        await _sut.ArmAsync();

        var result = await _sut.DisconnectAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    // ── StateChanged event ─────────────────────────────────────────────────────

    [Fact]
    public void StateChanged_CanBeSubscribedAndUnsubscribed()
    {
        EventHandler<DetectorStateChangedEventArgs>? handler = (_, _) => { };
        _sut.StateChanged += handler;
        _sut.StateChanged -= handler;

        // No exception means success
    }

    [Fact]
    public void ImageAcquired_CanBeSubscribed()
    {
        EventHandler<ImageAcquiredEventArgs>? handler = (_, _) => { };
        _sut.ImageAcquired += handler;
        _sut.ImageAcquired -= handler;
    }

    // ── Thread safety ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CurrentState_IsThreadSafe_ConcurrentAccess()
    {
        // Access CurrentState from multiple threads concurrently
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => _sut.CurrentState))
            .ToArray();

        var states = await Task.WhenAll(tasks);

        states.Should().OnlyContain(s => s == DetectorState.Disconnected);
    }
}
