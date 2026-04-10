using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Detector.ThirdParty;
using Xunit;

namespace HnVue.Detector.Tests;

/// <summary>
/// Edge-case and comprehensive tests for <see cref="VendorAdapterTemplate"/>
/// to improve branch coverage, especially around Dispose and state transitions.
/// </summary>
[Trait("SWR", "SWR-DET-010")]
public sealed class VendorAdapterTemplateEdgeCaseTests
{
    private static DetectorConfig CreateConfig() => new("192.168.1.200");

    // ── Connect then connect again — no duplicate event ──────────────────────

    [Fact]
    public async Task ConnectAsync_WhenAlreadyIdle_FiresNoAdditionalEvent()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        await sut.ConnectAsync();

        var events = new List<DetectorStateChangedEventArgs>();
        sut.StateChanged += (_, e) => events.Add(e);

        // Already Idle, transitioning to Idle → no event
        await sut.ConnectAsync();

        events.Should().BeEmpty("state did not change from Idle to Idle");
    }

    // ── Disconnect twice — no duplicate event ────────────────────────────────

    [Fact]
    public async Task DisconnectAsync_Twice_SecondCallFiresNoEvent()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        await sut.ConnectAsync();
        await sut.DisconnectAsync();

        var events = new List<DetectorStateChangedEventArgs>();
        sut.StateChanged += (_, e) => events.Add(e);

        await sut.DisconnectAsync();

        events.Should().BeEmpty("already Disconnected, same state transition is suppressed");
    }

    // ── AbortAsync from Disconnected ─────────────────────────────────────────

    [Fact]
    public async Task AbortAsync_FromDisconnected_TransitionsToIdle()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());

        var result = await sut.AbortAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(DetectorState.Idle);
    }

    // ── AbortAsync from Idle ─────────────────────────────────────────────────

    [Fact]
    public async Task AbortAsync_FromIdle_StaysIdle()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        await sut.ConnectAsync();

        var events = new List<DetectorStateChangedEventArgs>();
        sut.StateChanged += (_, e) => events.Add(e);

        await sut.AbortAsync();

        // Already Idle, AbortAsync transitions to Idle with reason — no actual state change
        events.Should().BeEmpty("state did not change from Idle to Idle");
    }

    // ── GetStatusAsync after Abort ───────────────────────────────────────────

    [Fact]
    public async Task GetStatusAsync_AfterAbort_ReturnsIdleWithReady()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        await sut.ConnectAsync();
        await sut.ArmAsync();
        await sut.AbortAsync();

        var result = await sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(DetectorState.Idle);
        result.Value.IsReadyToArm.Should().BeTrue();
    }

    // ── ArmAsync from Disconnected — still succeeds (template pattern) ───────

    [Fact]
    public async Task ArmAsync_FromDisconnected_TransitionsToArmed()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        // Not connected, but template doesn't enforce state guards

        var result = await sut.ArmAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(DetectorState.Armed);
    }

    // ── Dispose then continue — state still accessible ───────────────────────

    [Fact]
    public async Task Dispose_AfterConnect_StateStillAccessible()
    {
        var sut = new VendorAdapterTemplate(CreateConfig());
        await sut.ConnectAsync();
        sut.Dispose();

        // State is still accessible after dispose (read-only)
        sut.CurrentState.Should().Be(DetectorState.Idle);
    }

    // ── CancellationToken support ────────────────────────────────────────────

    [Fact]
    public async Task ConnectAsync_WithCancellationToken_Succeeds()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        using var cts = new CancellationTokenSource();

        var result = await sut.ConnectAsync(cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DisconnectAsync_WithCancellationToken_Succeeds()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        await sut.ConnectAsync();
        using var cts = new CancellationTokenSource();

        var result = await sut.DisconnectAsync(cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ArmAsync_WithCancellationToken_Succeeds()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        await sut.ConnectAsync();
        using var cts = new CancellationTokenSource();

        var result = await sut.ArmAsync(cancellationToken: cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AbortAsync_WithCancellationToken_Succeeds()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        using var cts = new CancellationTokenSource();

        var result = await sut.AbortAsync(cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetStatusAsync_WithCancellationToken_Succeeds()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        using var cts = new CancellationTokenSource();

        var result = await sut.GetStatusAsync(cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    // ── Multiple state transitions in quick succession ───────────────────────

    [Fact]
    public async Task RapidStateTransitions_AllEventsRecorded()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        var events = new List<DetectorStateChangedEventArgs>();
        sut.StateChanged += (_, e) => events.Add(e);

        await sut.ConnectAsync();
        await sut.ArmAsync();
        await sut.AbortAsync();
        await sut.ArmAsync();
        await sut.DisconnectAsync();

        events.Should().HaveCount(5);
        events[0].NewState.Should().Be(DetectorState.Idle);
        events[1].NewState.Should().Be(DetectorState.Armed);
        events[2].NewState.Should().Be(DetectorState.Idle);
        events[3].NewState.Should().Be(DetectorState.Armed);
        events[4].NewState.Should().Be(DetectorState.Disconnected);
    }

    // ── ImageAcquired event — never fires from template ──────────────────────

    [Fact]
    public async Task FullWorkflow_ImageAcquiredNeverFires()
    {
        using var sut = new VendorAdapterTemplate(CreateConfig());
        var fired = false;
        sut.ImageAcquired += (_, _) => fired = true;

        await sut.ConnectAsync();
        await sut.ArmAsync();
        await sut.AbortAsync();
        await sut.DisconnectAsync();

        fired.Should().BeFalse("template does not implement image acquisition");
    }
}
