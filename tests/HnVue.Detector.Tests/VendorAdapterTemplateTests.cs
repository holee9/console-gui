using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Detector;
using HnVue.Detector.ThirdParty;
using Xunit;

namespace HnVue.Detector.Tests;

/// <summary>
/// Tests for VendorAdapterTemplate — the third-party detector SDK adapter template.
/// Validates lifecycle transitions, interface compliance, and disposal behaviour.
/// SWR-WF-033: VendorAdapterTemplate must implement IDetectorInterface contract.
/// </summary>
[Trait("SWR", "SWR-WF-033")]
public sealed class VendorAdapterTemplateTests : IDisposable
{
    private readonly DetectorConfig _config = new("192.168.1.200");
    private readonly VendorAdapterTemplate _sut;

    public VendorAdapterTemplateTests()
    {
        _sut = new VendorAdapterTemplate(_config);
    }

    public void Dispose() => _sut.Dispose();

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithValidConfig_InitialStateIsDisconnected()
    {
        _sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    [Fact]
    public void Constructor_NullConfig_ThrowsArgumentNullException()
    {
        var act = () => new VendorAdapterTemplate(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    // ── ConnectAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ConnectAsync_FromDisconnected_ReturnsSuccess()
    {
        var result = await _sut.ConnectAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ConnectAsync_FromDisconnected_TransitionsToIdle()
    {
        await _sut.ConnectAsync();

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
    public async Task ConnectAsync_WithCancellationToken_ReturnsSuccess()
    {
        using var cts = new CancellationTokenSource();

        var result = await _sut.ConnectAsync(cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    // ── DisconnectAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task DisconnectAsync_AfterConnect_ReturnsSuccess()
    {
        await _sut.ConnectAsync();

        var result = await _sut.DisconnectAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DisconnectAsync_AfterConnect_TransitionsToDisconnected()
    {
        await _sut.ConnectAsync();

        await _sut.DisconnectAsync();

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
        events[0].NewState.Should().Be(DetectorState.Disconnected);
    }

    // ── ArmAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task ArmAsync_AfterConnect_ReturnsSuccess()
    {
        await _sut.ConnectAsync();

        var result = await _sut.ArmAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ArmAsync_AfterConnect_TransitionsToArmed()
    {
        await _sut.ConnectAsync();

        await _sut.ArmAsync();

        _sut.CurrentState.Should().Be(DetectorState.Armed);
    }

    [Fact]
    public async Task ArmAsync_FreeRunMode_TransitionsToArmed()
    {
        await _sut.ConnectAsync();

        var result = await _sut.ArmAsync(DetectorTriggerMode.FreeRun);

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(DetectorState.Armed);
    }

    [Fact]
    public async Task ArmAsync_SyncMode_TransitionsToArmed()
    {
        await _sut.ConnectAsync();

        var result = await _sut.ArmAsync(DetectorTriggerMode.Sync);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ArmAsync_RaisesStateChangedEvent()
    {
        await _sut.ConnectAsync();
        var events = new List<DetectorStateChangedEventArgs>();
        _sut.StateChanged += (_, e) => events.Add(e);

        await _sut.ArmAsync();

        events.Should().ContainSingle();
        events[0].NewState.Should().Be(DetectorState.Armed);
    }

    // ── AbortAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AbortAsync_FromArmed_ReturnsSuccess()
    {
        await _sut.ConnectAsync();
        await _sut.ArmAsync();

        var result = await _sut.AbortAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AbortAsync_FromArmed_TransitionsToIdle()
    {
        await _sut.ConnectAsync();
        await _sut.ArmAsync();

        await _sut.AbortAsync();

        _sut.CurrentState.Should().Be(DetectorState.Idle);
    }

    [Fact]
    public async Task AbortAsync_RaisesStateChangedEvent()
    {
        await _sut.ConnectAsync();
        await _sut.ArmAsync();
        var events = new List<DetectorStateChangedEventArgs>();
        _sut.StateChanged += (_, e) => events.Add(e);

        await _sut.AbortAsync();

        events.Should().ContainSingle();
        events[0].NewState.Should().Be(DetectorState.Idle);
    }

    // ── GetStatusAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStatusAsync_WhenIdle_ReturnsReadyStatus()
    {
        await _sut.ConnectAsync();

        var result = await _sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(DetectorState.Idle);
        result.Value.IsReadyToArm.Should().BeTrue();
    }

    [Fact]
    public async Task GetStatusAsync_WhenDisconnected_ReturnsNotReady()
    {
        var result = await _sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.IsReadyToArm.Should().BeFalse();
        result.Value.State.Should().Be(DetectorState.Disconnected);
    }

    [Fact]
    public async Task GetStatusAsync_WhenArmed_ReturnsArmedState()
    {
        await _sut.ConnectAsync();
        await _sut.ArmAsync();

        var result = await _sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(DetectorState.Armed);
    }

    // ── IDisposable ───────────────────────────────────────────────────────────

    [Fact]
    public void Dispose_CalledOnce_DoesNotThrow()
    {
        using var adapter = new VendorAdapterTemplate(_config);

        var act = () => adapter.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_IsIdempotent()
    {
        var adapter = new VendorAdapterTemplate(_config);

        adapter.Dispose();
        var act = () => adapter.Dispose();

        act.Should().NotThrow();
    }

    // ── Complete lifecycle ─────────────────────────────────────────────────────

    [Fact]
    public async Task FullLifecycle_ConnectArmAbortDisconnect_CompletesSuccessfully()
    {
        using var adapter = new VendorAdapterTemplate(new DetectorConfig("192.168.1.201"));

        var connectResult = await adapter.ConnectAsync();
        var armResult = await adapter.ArmAsync();
        var abortResult = await adapter.AbortAsync();
        var disconnectResult = await adapter.DisconnectAsync();

        connectResult.IsSuccess.Should().BeTrue();
        armResult.IsSuccess.Should().BeTrue();
        abortResult.IsSuccess.Should().BeTrue();
        disconnectResult.IsSuccess.Should().BeTrue();
        adapter.CurrentState.Should().Be(DetectorState.Disconnected);
    }
}
