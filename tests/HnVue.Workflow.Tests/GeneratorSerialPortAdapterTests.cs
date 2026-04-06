using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Workflow;
using Xunit;

namespace HnVue.Workflow.Tests;

/// <summary>
/// Unit tests for <see cref="GeneratorSerialPort"/> using <see cref="FakeSerialPortAdapter"/>
/// to exercise the full state machine without physical hardware.
///
/// Coverage targets: PrepareAsync, ConnectAsync, TriggerExposureAsync, AbortAsync.
/// IEC 62304 §5.3.6: SWR-WF-020, SWR-WF-021, SWR-WF-022.
/// </summary>
[Trait("SWR", "SWR-WF-020")]
public sealed class GeneratorSerialPortAdapterTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static GeneratorConfig MakeCpiConfig(int timeoutMs = 2000) =>
        new(PortName: "COM_FAKE", BaudRate: 9600, Protocol: GeneratorProtocol.Cpi, TimeoutMs: timeoutMs);

    private static GeneratorConfig MakeSedecalConfig(int timeoutMs = 2000) =>
        new(PortName: "COM_FAKE", BaudRate: 9600, Protocol: GeneratorProtocol.Sedecal, TimeoutMs: timeoutMs);

    private static ExposureParameters MakeParams() =>
        new(BodyPart: "CHEST", Kvp: 80.0, Mas: 5.0, StudyInstanceUid: "1.2.3");

    /// <summary>
    /// Creates a SUT already in Idle state (simulating a successful ConnectAsync).
    /// Returns both the SUT and the fake adapter for assertion.
    /// </summary>
    private static (GeneratorSerialPort sut, FakeSerialPortAdapter fake) CreateIdleSut(
        GeneratorConfig? config = null)
    {
        var fake = new FakeSerialPortAdapter();
        fake.ForceOpen();

        var sut = new GeneratorSerialPort(config ?? MakeCpiConfig(), fake);

        // Drive the SUT to Idle by simulating a successful ConnectAsync.
        // The adapter is already open, so we enqueue ACK for GET_STATUS.
        fake.EnqueueResponse("ACK\r\n");
        sut.ConnectAsync().GetAwaiter().GetResult();

        return (sut, fake);
    }

    // ── ConnectAsync via fake adapter ─────────────────────────────────────────

    [Fact]
    public async Task ConnectAsync_WithFakeAdapter_AckResponse_TransitionsToIdle()
    {
        var fake = new FakeSerialPortAdapter();
        fake.ForceOpen();
        fake.EnqueueResponse("ACK\r\n");

        using var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);

        var result = await sut.ConnectAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Idle);
    }

    [Fact]
    public async Task ConnectAsync_WithFakeAdapter_ReadyResponse_TransitionsToIdle()
    {
        var fake = new FakeSerialPortAdapter();
        fake.ForceOpen();
        fake.EnqueueResponse("READY\r\n");

        using var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);

        var result = await sut.ConnectAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Idle);
    }

    [Fact]
    public async Task ConnectAsync_PortNotOpen_ReturnsFailure()
    {
        var fake = new FakeSerialPortAdapter();
        // Port is closed — Open() will succeed but ReadExisting returns nothing,
        // so the SUT will time out.
        // Set a very short timeout to keep the test fast.
        using var sut = new GeneratorSerialPort(MakeCpiConfig(timeoutMs: 100), fake);

        var result = await sut.ConnectAsync();

        // Open() is called inside ConnectAsync, but no response is enqueued → timeout.
        result.IsFailure.Should().BeTrue();
    }

    // ── PrepareAsync ──────────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-WF-020")]
    public async Task PrepareAsync_HappyPath_ReturnsSuccessAndStateIsReady()
    {
        var (sut, fake) = CreateIdleSut();

        // Enqueue ACK for SET_KVP, SET_MAS, LOAD_APR, then READY for PREP.
        fake.EnqueueResponse("ACK\r\n"); // SET_KVP
        fake.EnqueueResponse("ACK\r\n"); // SET_MAS
        fake.EnqueueResponse("ACK\r\n"); // LOAD_APR
        fake.EnqueueResponse("READY\r\n"); // PREP

        var result = await sut.PrepareAsync(MakeParams());

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Ready);
    }

    [Fact]
    [Trait("SWR", "SWR-WF-020")]
    public async Task PrepareAsync_ErrorResponseAfterPrep_ReturnsFailureAndStateIsError()
    {
        var (sut, fake) = CreateIdleSut();

        fake.EnqueueResponse("ACK\r\n"); // SET_KVP
        fake.EnqueueResponse("ACK\r\n"); // SET_MAS
        fake.EnqueueResponse("ACK\r\n"); // LOAD_APR
        fake.EnqueueResponse("ERROR overload\r\n"); // PREP response

        var result = await sut.PrepareAsync(MakeParams());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
        sut.CurrentState.Should().Be(GeneratorState.Error);
    }

    [Fact]
    [Trait("SWR", "SWR-WF-020")]
    public async Task PrepareAsync_TimeoutWaitingForKvpAck_ReturnsFailure()
    {
        var config = MakeCpiConfig(timeoutMs: 100); // very short timeout
        var (sut, fake) = CreateIdleSut(config);
        // Do NOT enqueue any response — will time out.

        var result = await sut.PrepareAsync(MakeParams());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
    }

    [Fact]
    [Trait("SWR", "SWR-WF-020")]
    public async Task PrepareAsync_WhenNotIdle_ReturnsFailure()
    {
        var fake = new FakeSerialPortAdapter();
        fake.ForceOpen();
        using var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);
        // SUT remains Disconnected — not Idle.

        var result = await sut.PrepareAsync(MakeParams());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
    }

    [Fact]
    [Trait("SWR", "SWR-WF-020")]
    public async Task PrepareAsync_SendsCorrectCommandSequence()
    {
        var (sut, fake) = CreateIdleSut();

        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("READY\r\n");

        await sut.PrepareAsync(MakeParams());

        // ConnectAsync sent GET_STATUS; PrepareAsync should then send SET_KVP, SET_MAS, LOAD_APR, PREP.
        fake.SentCommands.Should().Contain(c => c.StartsWith("SET_KVP"));
        fake.SentCommands.Should().Contain(c => c.StartsWith("SET_MAS"));
        fake.SentCommands.Should().Contain(c => c.StartsWith("LOAD_APR"));
        fake.SentCommands.Should().Contain(c => c == "PREP");
    }

    [Fact]
    [Trait("SWR", "SWR-WF-020")]
    public async Task PrepareAsync_StateChangedEvent_FiresPreparingThenReady()
    {
        var (sut, fake) = CreateIdleSut();

        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("READY\r\n");

        var states = new List<GeneratorState>();
        sut.StateChanged += (_, e) => states.Add(e.NewState);

        await sut.PrepareAsync(MakeParams());

        states.Should().Contain(GeneratorState.Preparing);
        states.Should().Contain(GeneratorState.Ready);
    }

    // ── TriggerExposureAsync ──────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-WF-021")]
    public async Task TriggerExposureAsync_WhenNotReady_ReturnsFailure()
    {
        var fake = new FakeSerialPortAdapter();
        fake.ForceOpen();
        using var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);
        // State is Disconnected, not Ready.

        var result = await sut.TriggerExposureAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
    }

    [Fact]
    [Trait("SWR", "SWR-WF-021")]
    public async Task TriggerExposureAsync_HappyPath_ReturnsSuccessAndStateIsIdle()
    {
        var (sut, fake) = CreateIdleSut();

        // Drive to Ready state first.
        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("READY\r\n");
        await sut.PrepareAsync(MakeParams());

        sut.CurrentState.Should().Be(GeneratorState.Ready);

        // Now trigger exposure.
        fake.EnqueueResponse("EXPOSURE_DONE\r\n");
        var result = await sut.TriggerExposureAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Idle);
    }

    [Fact]
    [Trait("SWR", "SWR-WF-021")]
    public async Task TriggerExposureAsync_AecTerminated_ReturnsSuccessAndStateIsIdle()
    {
        var (sut, fake) = CreateIdleSut();

        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("READY\r\n");
        await sut.PrepareAsync(MakeParams());

        fake.EnqueueResponse("AEC_TERMINATED\r\n");
        var result = await sut.TriggerExposureAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Idle);
    }

    // ── AbortAsync ────────────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-WF-022")]
    public async Task AbortAsync_WhenIdle_ReturnsSuccessAndStateIsError()
    {
        var (sut, _) = CreateIdleSut();

        var result = await sut.AbortAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Error);
    }

    [Fact]
    [Trait("SWR", "SWR-WF-022")]
    public async Task AbortAsync_WhenDisconnected_ReturnsSuccessAndStateIsError()
    {
        var fake = new FakeSerialPortAdapter();
        using var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);

        var result = await sut.AbortAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Error);
    }

    // ── GetStatusAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStatusAsync_WhenIdle_ReturnsIdleStatus()
    {
        var (sut, fake) = CreateIdleSut();

        // Enqueue HEAT_UNITS response for GET_HEAT_UNITS.
        fake.EnqueueResponse("HEAT_UNITS 12.5\r\n");

        var result = await sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(GeneratorState.Idle);
        result.Value.HeatUnitPercentage.Should().BeApproximately(12.5, 0.001);
        result.Value.IsReadyToExpose.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatusAsync_WhenReady_IsReadyToExposeIsTrue()
    {
        var (sut, fake) = CreateIdleSut();

        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("READY\r\n");
        await sut.PrepareAsync(MakeParams());

        fake.EnqueueResponse("HEAT_UNITS 0.0\r\n");
        var result = await sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.IsReadyToExpose.Should().BeTrue();
    }

    // ── DisconnectAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task DisconnectAsync_WhenIdle_TransitionsToDisconnected()
    {
        var (sut, _) = CreateIdleSut();

        var result = await sut.DisconnectAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Disconnected);
    }
}
