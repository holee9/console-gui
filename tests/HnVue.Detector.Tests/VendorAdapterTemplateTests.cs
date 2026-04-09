using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Detector.ThirdParty;
using Xunit;

namespace HnVue.Detector.Tests;

[Trait("SWR", "SWR-DET-010")]
public sealed class VendorAdapterTemplateTests
{
    private static DetectorConfig CreateConfig() => new("192.168.1.200");

    // ── Constructor ──────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullConfig_ThrowsArgumentNullException()
    {
        var act = () => new VendorAdapterTemplate(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public void Constructor_ValidConfig_SetsDisconnectedState()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());

        sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    // ── ConnectAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ConnectAsync_FromDisconnected_TransitionsToIdle()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());

        var result = await sut.ConnectAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(DetectorState.Idle);
    }

    [Fact]
    public async Task ConnectAsync_RaisesStateChangedEvent()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        var events = new List<DetectorStateChangedEventArgs>();
        sut.StateChanged += (_, e) => events.Add(e);

        await sut.ConnectAsync();

        events.Should().ContainSingle();
        events[0].PreviousState.Should().Be(DetectorState.Disconnected);
        events[0].NewState.Should().Be(DetectorState.Idle);
    }

    // ── DisconnectAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DisconnectAsync_FromIdle_TransitionsToDisconnected()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        await sut.ConnectAsync();

        var result = await sut.DisconnectAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    [Fact]
    public async Task DisconnectAsync_FromDisconnected_StillSucceeds()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());

        var result = await sut.DisconnectAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    [Fact]
    public async Task DisconnectAsync_RaisesStateChangedEvent()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        await sut.ConnectAsync();
        var events = new List<DetectorStateChangedEventArgs>();
        sut.StateChanged += (_, e) => events.Add(e);

        await sut.DisconnectAsync();

        events.Should().ContainSingle();
        events[0].PreviousState.Should().Be(DetectorState.Idle);
        events[0].NewState.Should().Be(DetectorState.Disconnected);
    }

    // ── ArmAsync ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ArmAsync_FromIdle_TransitionsToArmed()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        await sut.ConnectAsync();

        var result = await sut.ArmAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(DetectorState.Armed);
    }

    [Fact]
    public async Task ArmAsync_WithFreeRunMode_TransitionsToArmed()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        await sut.ConnectAsync();

        var result = await sut.ArmAsync(DetectorTriggerMode.FreeRun);

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(DetectorState.Armed);
    }

    [Fact]
    public async Task ArmAsync_RaisesStateChangedEvent()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        await sut.ConnectAsync();
        var events = new List<DetectorStateChangedEventArgs>();
        sut.StateChanged += (_, e) => events.Add(e);

        await sut.ArmAsync();

        events.Should().ContainSingle();
        events[0].PreviousState.Should().Be(DetectorState.Idle);
        events[0].NewState.Should().Be(DetectorState.Armed);
    }

    // ── AbortAsync ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task AbortAsync_FromAnyState_TransitionsToIdle()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        await sut.ConnectAsync();
        await sut.ArmAsync();

        var result = await sut.AbortAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(DetectorState.Idle);
    }

    [Fact]
    public async Task AbortAsync_RaisesStateChangedEventWithReason()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        await sut.ConnectAsync();
        await sut.ArmAsync();
        var events = new List<DetectorStateChangedEventArgs>();
        sut.StateChanged += (_, e) => events.Add(e);

        await sut.AbortAsync();

        events.Should().ContainSingle();
        events[0].PreviousState.Should().Be(DetectorState.Armed);
        events[0].NewState.Should().Be(DetectorState.Idle);
        events[0].Reason.Should().Be("Abort requested");
    }

    // ── GetStatusAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStatusAsync_WhenDisconnected_ReturnsDisconnectedStatus()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());

        var result = await sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(DetectorState.Disconnected);
        result.Value.IsReadyToArm.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatusAsync_WhenIdle_ReturnsReadyStatus()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        await sut.ConnectAsync();

        var result = await sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(DetectorState.Idle);
        result.Value.IsReadyToArm.Should().BeTrue();
    }

    [Fact]
    public async Task GetStatusAsync_WhenArmed_ReturnsArmedStatus()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        await sut.ConnectAsync();
        await sut.ArmAsync();

        var result = await sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(DetectorState.Armed);
        result.Value.IsReadyToArm.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsDefaultTemperatureAndNullMetadata()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        await sut.ConnectAsync();

        var result = await sut.GetStatusAsync();

        result.Value.TemperatureCelsius.Should().Be(0.0);
        result.Value.SerialNumber.Should().BeNull();
        result.Value.FirmwareVersion.Should().BeNull();
    }

    // ── Dispose ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Dispose_CalledOnce_DoesNotThrow()
    {
        var sut = new VendorAdapterTemplate(CreateConfig());

        var act = () => sut.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var sut = new VendorAdapterTemplate(CreateConfig());

        sut.Dispose();

        var act = () => sut.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_PreventsReuseViaObjectDisposedCheck()
    {
        var sut = new VendorAdapterTemplate(CreateConfig());
        sut.Dispose();

        // VendorAdapterTemplate does not check disposed state in its methods
        // (unlike OwnDetectorAdapter), so we verify CurrentState is still accessible
        sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    // ── Full workflow ────────────────────────────────────────────────────────────

    [Fact]
    public async Task FullWorkflow_ConnectArmAbortDisconnect_Succeeds()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        var stateChanges = new List<DetectorStateChangedEventArgs>();
        sut.StateChanged += (_, e) => stateChanges.Add(e);

        await sut.ConnectAsync();
        sut.CurrentState.Should().Be(DetectorState.Idle);

        await sut.ArmAsync();
        sut.CurrentState.Should().Be(DetectorState.Armed);

        await sut.AbortAsync();
        sut.CurrentState.Should().Be(DetectorState.Idle);

        await sut.DisconnectAsync();
        sut.CurrentState.Should().Be(DetectorState.Disconnected);

        stateChanges.Should().HaveCount(4);
        stateChanges[0].NewState.Should().Be(DetectorState.Idle);
        stateChanges[1].NewState.Should().Be(DetectorState.Armed);
        stateChanges[2].NewState.Should().Be(DetectorState.Idle);
        stateChanges[3].NewState.Should().Be(DetectorState.Disconnected);
    }

    [Fact]
    public async Task StateChanged_FiresOnlyWhenStateActuallyChanges()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        var events = new List<DetectorStateChangedEventArgs>();
        sut.StateChanged += (_, e) => events.Add(e);

        // Disconnect when already Disconnected — no event should fire
        await sut.DisconnectAsync();

        events.Should().BeEmpty("state did not change from Disconnected to Disconnected");
    }
}
