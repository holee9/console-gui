using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Detector;
using Xunit;

namespace HnVue.Detector.Tests;

[Trait("SWR", "SWR-WF-030")]
public sealed class DetectorSimulatorTests
{
    private readonly DetectorSimulator _sut = new()
    {
        ArmDelayMs = 0,
        ReadoutDelayMs = 0,
    };

    // ── Constructor / initial state ───────────────────────────────────────────

    [Fact]
    public void InitialState_IsDisconnected()
    {
        _sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    // ── ConnectAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ConnectAsync_FromDisconnected_TransitionsToIdle()
    {
        var result = await _sut.ConnectAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(DetectorState.Idle);
    }

    [Fact]
    public async Task ConnectAsync_WithInjectedFailure_ReturnsFailure()
    {
        _sut.FailNextConnectWith = "Simulated connect failure";

        var result = await _sut.ConnectAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DetectorNotReady);
    }

    // ── DisconnectAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task DisconnectAsync_FromIdle_TransitionsToDisconnected()
    {
        await _sut.ConnectAsync();

        var result = await _sut.DisconnectAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    // ── ArmAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task ArmAsync_FromIdle_AcquiresImageAndReturnsToIdle()
    {
        await _sut.ConnectAsync();
        RawDetectorImage? receivedImage = null;
        _sut.ImageAcquired += (_, e) => receivedImage = e.Image;

        var result = await _sut.ArmAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(DetectorState.Idle);
        receivedImage.Should().NotBeNull();
        receivedImage!.Width.Should().Be(_sut.SimulatedImageWidth);
        receivedImage.Height.Should().Be(_sut.SimulatedImageHeight);
        receivedImage.BitsPerPixel.Should().Be(12);
        receivedImage.PixelData.Should().HaveCount(_sut.SimulatedImageWidth * _sut.SimulatedImageHeight * 2);
    }

    [Fact]
    public async Task ArmAsync_WhenNotIdle_ReturnsFailure()
    {
        // Detector is Disconnected — not Idle
        var result = await _sut.ArmAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DetectorNotReady);
    }

    [Fact]
    public async Task ArmAsync_WithInjectedFailure_ReturnsFailureAndTransitionsToError()
    {
        await _sut.ConnectAsync();
        _sut.FailNextArmWith = "Simulated arm failure";

        var result = await _sut.ArmAsync();

        result.IsFailure.Should().BeTrue();
        _sut.CurrentState.Should().Be(DetectorState.Error);
    }

    // ── AbortAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AbortAsync_FromAnyState_TransitionsToError()
    {
        await _sut.ConnectAsync();

        var result = await _sut.AbortAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.CurrentState.Should().Be(DetectorState.Error);
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
        result.Value.SerialNumber.Should().Be("SIM-001");
    }

    // ── StateChanged event ────────────────────────────────────────────────────

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

    // ── FreeRun trigger mode ──────────────────────────────────────────────────

    [Fact]
    public async Task ArmAsync_FreeRunMode_AlsoAcquiresImage()
    {
        await _sut.ConnectAsync();
        var imageReceived = false;
        _sut.ImageAcquired += (_, _) => imageReceived = true;

        var result = await _sut.ArmAsync(DetectorTriggerMode.FreeRun);

        result.IsSuccess.Should().BeTrue();
        imageReceived.Should().BeTrue();
    }
}
