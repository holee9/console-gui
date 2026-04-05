using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Workflow;
using Xunit;

namespace HnVue.Workflow.Tests;

/// <summary>
/// Smoke tests for <see cref="GeneratorSerialPort"/> that do not require physical hardware.
/// These tests verify construction-time behaviour and initial state only.
/// Full integration tests require a real or simulated RS-232 loopback fixture.
/// </summary>
[Trait("SWR", "SWR-WF-020")]
public sealed class GeneratorSerialPortSmokeTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static GeneratorConfig MakeSedecalConfig() =>
        new(PortName: "COM99", BaudRate: 9600, Protocol: GeneratorProtocol.Sedecal);

    private static GeneratorConfig MakeCpiConfig() =>
        new(PortName: "COM99", BaudRate: 9600, Protocol: GeneratorProtocol.Cpi);

    // ── Construction ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithValidConfig_DoesNotThrow()
    {
        var config = MakeSedecalConfig();

        var act = () => new GeneratorSerialPort(config);

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_NullConfig_ThrowsArgumentNullException()
    {
        var act = () => new GeneratorSerialPort(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── Initial state ─────────────────────────────────────────────────────────

    [Fact]
    public void InitialState_IsDisconnected()
    {
        using var sut = new GeneratorSerialPort(MakeSedecalConfig());

        sut.CurrentState.Should().Be(GeneratorState.Disconnected);
    }

    [Fact]
    public void InitialState_WithCpiProtocol_IsDisconnected()
    {
        using var sut = new GeneratorSerialPort(MakeCpiConfig());

        sut.CurrentState.Should().Be(GeneratorState.Disconnected);
    }

    // ── StateChanged event wiring ─────────────────────────────────────────────

    [Fact]
    public void StateChanged_Event_CanBeSubscribed()
    {
        using var sut = new GeneratorSerialPort(MakeSedecalConfig());
        bool eventFired = false;
        sut.StateChanged += (_, _) => eventFired = true;

        // No state change should happen at construction time.
        eventFired.Should().BeFalse();
    }

    // ── Connect without hardware ───────────────────────────────────────────────

    [Fact]
    public async Task ConnectAsync_NonExistentPort_ReturnsFailure()
    {
        using var sut = new GeneratorSerialPort(MakeSedecalConfig());

        var result = await sut.ConnectAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(HnVue.Common.Results.ErrorCode.GeneratorNotReady);
    }

    [Fact]
    public async Task ConnectAsync_NonExistentPort_StateRemainsDisconnected()
    {
        using var sut = new GeneratorSerialPort(MakeSedecalConfig());

        await sut.ConnectAsync();

        sut.CurrentState.Should().Be(GeneratorState.Disconnected);
    }

    // ── Disconnect when already disconnected ──────────────────────────────────

    [Fact]
    public async Task DisconnectAsync_WhenAlreadyDisconnected_Succeeds()
    {
        using var sut = new GeneratorSerialPort(MakeSedecalConfig());

        var result = await sut.DisconnectAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Disconnected);
    }

    // ── Abort from disconnected state ─────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-WF-022")]
    public async Task AbortAsync_WhenDisconnected_ReturnsSuccessAndTransitionsToError()
    {
        using var sut = new GeneratorSerialPort(MakeSedecalConfig());

        // AbortAsync must never fail silently (HAZ-RAD interlock).
        var result = await sut.AbortAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Error);
    }

    // ── GetStatus without open port ───────────────────────────────────────────

    [Fact]
    public async Task GetStatusAsync_WhenDisconnected_ReturnsSuccessWithCurrentState()
    {
        using var sut = new GeneratorSerialPort(MakeSedecalConfig());

        var result = await sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(GeneratorState.Disconnected);
        result.Value.IsReadyToExpose.Should().BeFalse();
    }

    // ── Dispose ───────────────────────────────────────────────────────────────

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var sut = new GeneratorSerialPort(MakeSedecalConfig());

        var act = () =>
        {
            sut.Dispose();
            sut.Dispose();
        };

        act.Should().NotThrow();
    }

    // ── GeneratorConfig defaults ──────────────────────────────────────────────

    [Fact]
    public void GeneratorConfig_Defaults_AreCorrect()
    {
        var config = new GeneratorConfig("COM1");

        config.BaudRate.Should().Be(9600);
        config.DataBits.Should().Be(8);
        config.Parity.Should().Be(System.IO.Ports.Parity.None);
        config.StopBits.Should().Be(System.IO.Ports.StopBits.One);
        config.TimeoutMs.Should().Be(5000);
        config.Protocol.Should().Be(GeneratorProtocol.Sedecal);
    }
}
