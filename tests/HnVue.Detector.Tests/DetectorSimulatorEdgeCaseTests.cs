using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Detector;
using Xunit;

namespace HnVue.Detector.Tests;

/// <summary>
/// Edge-case and state-transition coverage tests for <see cref="DetectorSimulator"/>.
/// Targets branch coverage gaps identified in coverage analysis.
/// </summary>
[Trait("SWR", "SWR-WF-030")]
public sealed class DetectorSimulatorEdgeCaseTests
{
    private readonly DetectorSimulator _sut = new()
    {
        ArmDelayMs = 0,
        ReadoutDelayMs = 0,
    };

    // ── GetStatusAsync — when Disconnected ───────────────────────────────────

    [Fact]
    public async Task GetStatusAsync_WhenDisconnected_ReturnsNotReady()
    {
        var result = await _sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(DetectorState.Disconnected);
        result.Value.IsReadyToArm.Should().BeFalse();
        result.Value.TemperatureCelsius.Should().Be(28.0);
        result.Value.FirmwareVersion.Should().Be("1.0.0-sim");
    }

    [Fact]
    public async Task GetStatusAsync_AfterError_ReturnsErrorState()
    {
        await _sut.ConnectAsync();
        await _sut.AbortAsync();

        var result = await _sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(DetectorState.Error);
        result.Value.IsReadyToArm.Should().BeFalse();
    }

    // ── StateChanged — no event when same state ──────────────────────────────

    [Fact]
    public async Task DisconnectAsync_WhenAlreadyDisconnected_NoStateChangedEvent()
    {
        var events = new List<DetectorStateChangedEventArgs>();
        _sut.StateChanged += (_, e) => events.Add(e);

        // Already in Disconnected state
        await _sut.DisconnectAsync();

        events.Should().BeEmpty("state did not change from Disconnected to Disconnected");
    }

    // ── ArmAsync — state change sequence ─────────────────────────────────────

    [Fact]
    public async Task ArmAsync_FromIdle_ProducesCorrectStateTransitionSequence()
    {
        await _sut.ConnectAsync();

        var states = new List<DetectorState>();
        _sut.StateChanged += (_, e) => states.Add(e.NewState);

        await _sut.ArmAsync();

        // Expected sequence: Idle -> Armed -> Acquiring -> ImageReady -> Idle
        states.Should().Equal(
            DetectorState.Armed,
            DetectorState.Acquiring,
            DetectorState.ImageReady,
            DetectorState.Idle);
    }

    // ── ConnectAsync — clears FailNextConnectWith after failure ───────────────

    [Fact]
    public async Task ConnectAsync_WithInjectedFailure_ClearsFailureAfterUse()
    {
        _sut.FailNextConnectWith = "First failure";

        // First connect fails
        var result1 = await _sut.ConnectAsync();
        result1.IsFailure.Should().BeTrue();

        // Second connect succeeds (failure was cleared)
        var result2 = await _sut.ConnectAsync();
        result2.IsSuccess.Should().BeTrue();
    }

    // ── ArmAsync — clears FailNextArmWith after failure ──────────────────────

    [Fact]
    public async Task ArmAsync_WithInjectedFailure_ClearsFailureAfterUse()
    {
        await _sut.ConnectAsync();
        _sut.FailNextArmWith = "First failure";

        var result1 = await _sut.ArmAsync();
        result1.IsFailure.Should().BeTrue();

        // Need to reconnect because we're in Error state
        await _sut.ConnectAsync();

        // Second arm succeeds (failure was cleared)
        var result2 = await _sut.ArmAsync();
        result2.IsSuccess.Should().BeTrue();
    }

    // ── ArmAsync — failure transitions state to Error with reason ────────────

    [Fact]
    public async Task ArmAsync_WithInjectedFailure_TransitionEventContainsReason()
    {
        await _sut.ConnectAsync();
        _sut.FailNextArmWith = "Hardware malfunction";

        var events = new List<DetectorStateChangedEventArgs>();
        _sut.StateChanged += (_, e) => events.Add(e);

        await _sut.ArmAsync();

        events.Should().ContainSingle();
        events[0].NewState.Should().Be(DetectorState.Error);
        events[0].Reason.Should().Be("Hardware malfunction");
    }

    // ── AbortAsync — reason in state change event ────────────────────────────

    [Fact]
    public async Task AbortAsync_RaisesStateChangedWithAbortReason()
    {
        await _sut.ConnectAsync();
        var events = new List<DetectorStateChangedEventArgs>();
        _sut.StateChanged += (_, e) => events.Add(e);

        await _sut.AbortAsync();

        events.Should().ContainSingle();
        events[0].Reason.Should().Be("Abort requested");
    }

    // ── ConnectAsync — reason is null for normal transition ───────────────────

    [Fact]
    public async Task ConnectAsync_RaisesStateChangedWithNullReason()
    {
        var events = new List<DetectorStateChangedEventArgs>();
        _sut.StateChanged += (_, e) => events.Add(e);

        await _sut.ConnectAsync();

        events.Should().ContainSingle();
        events[0].Reason.Should().BeNull();
    }

    // ── ConnectAsync — failure message ────────────────────────────────────────

    [Fact]
    public async Task ConnectAsync_WithInjectedFailure_ReturnsFailureMessage()
    {
        _sut.FailNextConnectWith = "Cable disconnected";

        var result = await _sut.ConnectAsync();

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Cable disconnected");
    }

    // ── ArmAsync — failure error message ─────────────────────────────────────

    [Fact]
    public async Task ArmAsync_WhenNotIdle_ContainsCurrentStateInMessage()
    {
        // Detector is Disconnected
        var result = await _sut.ArmAsync();

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Contain("Disconnected");
    }

    // ── Simulated image properties ───────────────────────────────────────────

    [Fact]
    public async Task ArmAsync_CustomImageSize_ProducesImageWithCorrectDimensions()
    {
        _sut.SimulatedImageWidth = 128;
        _sut.SimulatedImageHeight = 256;
        await _sut.ConnectAsync();

        RawDetectorImage? image = null;
        _sut.ImageAcquired += (_, e) => image = e.Image;

        await _sut.ArmAsync();

        image.Should().NotBeNull();
        image!.Width.Should().Be(128);
        image.Height.Should().Be(256);
        image.PixelData.Should().HaveCount(128 * 256 * 2); // 16-bit per pixel
        image.SerialNumber.Should().Be("SIM-001");
        image.TemperatureCelsius.Should().Be(28.0);
        image.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    // ── Multiple consecutive operations ──────────────────────────────────────

    [Fact]
    public async Task MultipleArmCycles_EachProducesImage()
    {
        await _sut.ConnectAsync();
        var imageCount = 0;
        _sut.ImageAcquired += (_, _) => imageCount++;

        await _sut.ArmAsync();
        await _sut.ArmAsync();
        await _sut.ArmAsync();

        imageCount.Should().Be(3);
    }

    // ── Disconnect after abort ───────────────────────────────────────────────

    [Fact]
    public async Task DisconnectAsync_AfterAbort_TransitionsFromErrorToDisconnected()
    {
        await _sut.ConnectAsync();
        await _sut.AbortAsync();
        _sut.CurrentState.Should().Be(DetectorState.Error);

        var events = new List<DetectorStateChangedEventArgs>();
        _sut.StateChanged += (_, e) => events.Add(e);

        var result = await _sut.DisconnectAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(DetectorState.Disconnected);
        events.Should().ContainSingle();
        events[0].PreviousState.Should().Be(DetectorState.Error);
    }

    // ── Default property values ──────────────────────────────────────────────

    [Fact]
    public void DefaultProperties_HaveSensibleValues()
    {
        var sim = new DetectorSimulator();

        sim.ArmDelayMs.Should().Be(200);
        sim.ReadoutDelayMs.Should().Be(1000);
        sim.SimulatedImageWidth.Should().Be(64);
        sim.SimulatedImageHeight.Should().Be(64);
        sim.FailNextConnectWith.Should().BeNull();
        sim.FailNextArmWith.Should().BeNull();
    }

    // ── CancellationToken — cancellation during arm delay ────────────────────

    [Fact]
    public async Task ArmAsync_CancelledDuringArmDelay_ThrowsOperationCancelledException()
    {
        var sim = new DetectorSimulator
        {
            ArmDelayMs = 5000, // long enough to cancel
            ReadoutDelayMs = 0,
        };
        await sim.ConnectAsync();
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(50);

        var act = async () => await sim.ArmAsync(cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── No ImageAcquired subscribers ─────────────────────────────────────────

    [Fact]
    public async Task ArmAsync_NoImageAcquiredSubscribers_StillSucceeds()
    {
        await _sut.ConnectAsync();
        // No subscribers attached to ImageAcquired

        var result = await _sut.ArmAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(DetectorState.Idle);
    }

    // ── No StateChanged subscribers ──────────────────────────────────────────

    [Fact]
    public async Task ConnectAsync_NoStateChangedSubscribers_StillSucceeds()
    {
        // No subscribers attached to StateChanged

        var result = await _sut.ConnectAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(DetectorState.Idle);
    }
}
