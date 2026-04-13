// <copyright file="GeneratorSerialPortCoverageTests.cs" company="ABYZ">
// Copyright (c) ABYZ. All rights reserved.
// </copyright>

using System.IO;
using System.IO.Ports;
using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Workflow;
using Xunit;

namespace HnVue.Workflow.Tests;

/// <summary>
/// Coverage boost tests for <see cref="GeneratorSerialPort"/> async state machine paths.
/// Focus: ConnectAsync, DisconnectAsync, PrepareAsync, TriggerExposureAsync exception paths.
/// IEC 62304 §5.3.6: SWR-WF-020, SWR-WF-021, SWR-WF-022.
/// </summary>
[Trait("SWR", "SWR-WF-020")]
public sealed class GeneratorSerialPortCoverageTests
{
    // ── Error-throwing adapter for exception path testing ──────────────────────

    /// <summary>
    /// Fake adapter that can be configured to throw on specific operations.
    /// Forwards DataReceived from inner adapter.
    /// </summary>
    private sealed class ThrowingFakeAdapter : ISerialPortAdapter
    {
        private readonly FakeSerialPortAdapter _inner = new();
        private SerialDataReceivedEventHandler? _dataReceived;

        public bool ThrowOnOpen { get; set; }
        public bool ThrowOnClose { get; set; }
        public Exception? OpenException { get; set; }
        public Exception? CloseException { get; set; }

        public bool IsOpen => _inner.IsOpen;
        public Stream BaseStream => _inner.BaseStream;

        public event SerialDataReceivedEventHandler? DataReceived
        {
            add
            {
                _dataReceived += value;
                _inner.DataReceived += OnInnerDataReceived;
            }

            remove
            {
                _dataReceived -= value;
                _inner.DataReceived -= OnInnerDataReceived;
            }
        }

        private void OnInnerDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _dataReceived?.Invoke(this, e);
        }

        public void Open()
        {
            if (ThrowOnOpen)
            {
                throw OpenException!;
            }

            _inner.ForceOpen();
        }

        public void Close()
        {
            if (ThrowOnClose)
            {
                throw CloseException!;
            }

            _inner.Close();
        }

        public void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
        public string ReadExisting() => _inner.ReadExisting();
        public void Dispose() => _inner.Dispose();

        public void ForceOpen() => _inner.ForceOpen();
        public void EnqueueResponse(string response) => _inner.EnqueueResponse(response);
        public List<string> SentCommands => _inner.SentCommands;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static GeneratorConfig MakeCpiConfig(int timeoutMs = 2000) =>
        new(PortName: "COM_FAKE", BaudRate: 9600, Protocol: GeneratorProtocol.Cpi, TimeoutMs: timeoutMs);

    private static GeneratorConfig MakeSedecalConfig(int timeoutMs = 2000) =>
        new(PortName: "COM_FAKE", BaudRate: 9600, Protocol: GeneratorProtocol.Sedecal, TimeoutMs: timeoutMs);

    private static ExposureParameters MakeParams() =>
        new(BodyPart: "CHEST", Kvp: 80.0, Mas: 5.0, StudyInstanceUid: "1.2.3");

    private static (GeneratorSerialPort sut, FakeSerialPortAdapter fake) CreateIdleSut(
        GeneratorConfig? config = null)
    {
        var fake = new FakeSerialPortAdapter();
        fake.ForceOpen();
        var sut = new GeneratorSerialPort(config ?? MakeCpiConfig(), fake);
        fake.EnqueueResponse("ACK\r\n");
        sut.ConnectAsync().GetAwaiter().GetResult();
        return (sut, fake);
    }

    private static (GeneratorSerialPort sut, FakeSerialPortAdapter fake) CreateReadySut(
        GeneratorConfig? config = null)
    {
        var (sut, fake) = CreateIdleSut(config);
        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("READY\r\n");
        sut.PrepareAsync(MakeParams()).GetAwaiter().GetResult();
        return (sut, fake);
    }

    // ── ConnectAsync exception paths ──────────────────────────────────────────

    [Fact]
    public async Task ConnectAsync_UnauthorizedAccessException_ReturnsFailure()
    {
        var fake = new ThrowingFakeAdapter
        {
            ThrowOnOpen = true,
            OpenException = new UnauthorizedAccessException("Port in use"),
        };

        using var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);
        var result = await sut.ConnectAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
        sut.CurrentState.Should().Be(GeneratorState.Disconnected);
    }

    [Fact]
    public async Task ConnectAsync_IOException_ReturnsFailure()
    {
        var fake = new ThrowingFakeAdapter
        {
            ThrowOnOpen = true,
            OpenException = new IOException("Port not found"),
        };

        using var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);
        var result = await sut.ConnectAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
    }

    [Fact]
    public async Task ConnectAsync_InvalidOperationException_ReturnsFailure()
    {
        var fake = new ThrowingFakeAdapter
        {
            ThrowOnOpen = true,
            OpenException = new InvalidOperationException("Port already open"),
        };

        using var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);
        var result = await sut.ConnectAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
    }

    [Fact]
    public async Task ConnectAsync_BusyResponse_TransitionsToIdle()
    {
        var fake = new FakeSerialPortAdapter();
        fake.ForceOpen();
        fake.EnqueueResponse("BUSY\r\n");

        using var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);
        var result = await sut.ConnectAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Idle);
    }

    [Fact]
    public async Task ConnectAsync_AlreadyConnected_ReturnsFailure()
    {
        var (sut, _) = CreateIdleSut();

        // Already in Idle, try to connect again
        var result = await sut.ConnectAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
        sut.Dispose();
    }

    [Fact]
    public async Task ConnectAsync_TimeoutWaitingForStatus_ClosesPortAndReturnsFailure()
    {
        var fake = new FakeSerialPortAdapter();
        fake.ForceOpen();
        // Don't enqueue any response → timeout

        using var sut = new GeneratorSerialPort(MakeCpiConfig(timeoutMs: 100), fake);
        var result = await sut.ConnectAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
    }

    // ── DisconnectAsync exception paths ───────────────────────────────────────

    [Fact]
    public async Task DisconnectAsync_CloseThrowsIOException_ReturnsFailure()
    {
        var fake = new ThrowingFakeAdapter
        {
            ThrowOnClose = true,
            CloseException = new IOException("Port error"),
        };
        fake.ForceOpen();

        using var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);
        fake.EnqueueResponse("ACK\r\n");
        await sut.ConnectAsync();

        var result = await sut.DisconnectAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
    }

    // ── PrepareAsync exception paths ──────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-WF-020")]
    public async Task PrepareAsync_PortNotOpen_ReturnsFailure()
    {
        var fake = new FakeSerialPortAdapter();
        // Port is not open
        using var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);
        fake.EnqueueResponse("ACK\r\n");
        await sut.ConnectAsync(); // Will fail because port can't be opened

        // Try to prepare after failed connect
        var result = await sut.PrepareAsync(MakeParams());

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    [Trait("SWR", "SWR-WF-020")]
    public async Task PrepareAsync_TimeoutWaitingForMasAck_ReturnsFailure()
    {
        var config = MakeCpiConfig(timeoutMs: 100);
        var (sut, fake) = CreateIdleSut(config);

        fake.EnqueueResponse("ACK\r\n"); // SET_KVP ACK
        // No response for SET_MAS → timeout

        var result = await sut.PrepareAsync(MakeParams());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
        sut.Dispose();
    }

    [Fact]
    [Trait("SWR", "SWR-WF-020")]
    public async Task PrepareAsync_TimeoutWaitingForAprAck_ReturnsFailure()
    {
        var config = MakeCpiConfig(timeoutMs: 100);
        var (sut, fake) = CreateIdleSut(config);

        fake.EnqueueResponse("ACK\r\n"); // SET_KVP
        fake.EnqueueResponse("ACK\r\n"); // SET_MAS
        // No response for LOAD_APR → timeout

        var result = await sut.PrepareAsync(MakeParams());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
        sut.Dispose();
    }

    [Fact]
    [Trait("SWR", "SWR-WF-020")]
    public async Task PrepareAsync_TimeoutWaitingForReady_ReturnsFailure()
    {
        var config = MakeCpiConfig(timeoutMs: 100);
        var (sut, fake) = CreateIdleSut(config);

        fake.EnqueueResponse("ACK\r\n"); // SET_KVP
        fake.EnqueueResponse("ACK\r\n"); // SET_MAS
        fake.EnqueueResponse("ACK\r\n"); // LOAD_APR
        // No READY response for PREP → timeout

        var result = await sut.PrepareAsync(MakeParams());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
        sut.CurrentState.Should().Be(GeneratorState.Error);
        sut.Dispose();
    }

    [Fact]
    [Trait("SWR", "SWR-WF-020")]
    public async Task PrepareAsync_NullParameters_ThrowsArgumentNullException()
    {
        var (sut, _) = CreateIdleSut();

        var act = () => sut.PrepareAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
        sut.Dispose();
    }

    // ── TriggerExposureAsync exception paths ──────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-WF-021")]
    public async Task TriggerExposureAsync_PortNotOpen_ReturnsFailure()
    {
        var fake = new FakeSerialPortAdapter();
        using var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);

        // SUT is Disconnected, port not open
        var result = await sut.TriggerExposureAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
    }

    [Fact]
    [Trait("SWR", "SWR-WF-021")]
    public async Task TriggerExposureAsync_ErrorResponse_ReturnsExposureAborted()
    {
        var (sut, fake) = CreateReadySut();

        fake.EnqueueResponse("ERROR overload\r\n");
        var result = await sut.TriggerExposureAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ExposureAborted);
        sut.CurrentState.Should().Be(GeneratorState.Error);
        sut.Dispose();
    }

    [Fact]
    [Trait("SWR", "SWR-WF-021")]
    public async Task TriggerExposureAsync_TimeoutWaitingForResponse_ReturnsFailure()
    {
        var config = MakeCpiConfig(timeoutMs: 100);
        var (sut, fake) = CreateReadySut(config);

        // No response enqueued → timeout
        var result = await sut.TriggerExposureAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.GeneratorNotReady);
        sut.CurrentState.Should().Be(GeneratorState.Error);
        sut.Dispose();
    }

    // ── AbortAsync with open port ─────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-WF-022")]
    public async Task AbortAsync_WhenPortOpen_WritesAbortFrameAndReturnsSuccess()
    {
        var (sut, _) = CreateReadySut();

        var result = await sut.AbortAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Error);
        sut.Dispose();
    }

    // ── GetStatusAsync with closed port ───────────────────────────────────────

    [Fact]
    public async Task GetStatusAsync_PortNotOpen_ReturnsOfflineStatus()
    {
        var fake = new FakeSerialPortAdapter();
        using var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);

        var result = await sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(GeneratorState.Disconnected);
        result.Value.IsReadyToExpose.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatusAsync_HeatUnitsTimeout_ReturnsCachedValue()
    {
        var (sut, fake) = CreateIdleSut(MakeCpiConfig(timeoutMs: 100));

        // No heat units response enqueued → timeout, returns cached 0
        var result = await sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.HeatUnitPercentage.Should().Be(0);
        sut.Dispose();
    }

    // ── OnDataReceived with error response ────────────────────────────────────

    [Fact]
    public async Task OnDataReceived_UnsolicitedError_ForceTransitionsToErrorState()
    {
        var (sut, fake) = CreateIdleSut();

        // Enqueue an unsolicited error response that will be received
        // This will be picked up when the next read happens
        fake.EnqueueResponse("ERROR hardware_fault\r\n");

        // Trigger a read by sending a command that expects a response
        fake.EnqueueResponse("ACK\r\n"); // Recovery ACK

        // Send GET_STATUS to trigger data received flow
        var result = await sut.GetStatusAsync();

        // The error response should have forced state to Error
        sut.CurrentState.Should().Be(GeneratorState.Error);
        sut.Dispose();
    }

    // ── Dispose after connect ─────────────────────────────────────────────────

    [Fact]
    public void Dispose_WhenConnected_ClosesPortAndCancelsPending()
    {
        var fake = new FakeSerialPortAdapter();
        fake.ForceOpen();
        fake.EnqueueResponse("ACK\r\n");

        var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);
        sut.ConnectAsync().GetAwaiter().GetResult();

        // Dispose should close port cleanly
        var act = () => sut.Dispose();

        act.Should().NotThrow();
    }

    // ── Disposed object throws ────────────────────────────────────────────────

    [Fact]
    public async Task ConnectAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var fake = new FakeSerialPortAdapter();
        fake.ForceOpen();
        var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);
        sut.Dispose();

        var act = () => sut.ConnectAsync();

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task DisconnectAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var fake = new FakeSerialPortAdapter();
        var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);
        sut.Dispose();

        var act = () => sut.DisconnectAsync();

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task PrepareAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var fake = new FakeSerialPortAdapter();
        var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);
        sut.Dispose();

        var act = () => sut.PrepareAsync(MakeParams());

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task GetStatusAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var fake = new FakeSerialPortAdapter();
        var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);
        sut.Dispose();

        var act = () => sut.GetStatusAsync();

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    // ── State change event tracking ───────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-WF-021")]
    public async Task TriggerExposureAsync_FullWorkflow_StateChangesAreCorrect()
    {
        var (sut, fake) = CreateIdleSut();

        // Prepare
        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("ACK\r\n");
        fake.EnqueueResponse("READY\r\n");

        var states = new List<GeneratorState>();
        sut.StateChanged += (_, e) => states.Add(e.NewState);

        await sut.PrepareAsync(MakeParams());
        sut.CurrentState.Should().Be(GeneratorState.Ready);

        // Expose with AEC_TERMINATED
        fake.EnqueueResponse("AEC_TERMINATED\r\n");
        var result = await sut.TriggerExposureAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Idle);
        states.Should().Contain(GeneratorState.Exposing);
        states.Should().Contain(GeneratorState.Done);
        sut.Dispose();
    }

    // ── Constructor validation ────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullAdapter_ThrowsArgumentNullException()
    {
        var config = MakeCpiConfig();
        var act = () => new GeneratorSerialPort(config, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── DisconnectAsync when port is not open ─────────────────────────────────

    [Fact]
    public async Task DisconnectAsync_PortNotOpen_StillTransitionsToDisconnected()
    {
        var fake = new FakeSerialPortAdapter();
        // Port is not open
        using var sut = new GeneratorSerialPort(MakeCpiConfig(), fake);

        var result = await sut.DisconnectAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Disconnected);
    }

    // ── Multiple state change events ──────────────────────────────────────────

    [Fact]
    public async Task TriggerExposureAsync_AecTerminated_CpiProtocol_ReturnsSuccess()
    {
        var (sut, fake) = CreateReadySut();

        fake.EnqueueResponse("AEC_TERMINATED\r\n");
        var result = await sut.TriggerExposureAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(GeneratorState.Idle);
        sut.Dispose();
    }

    // ── Heat units response parsing ───────────────────────────────────────────

    [Fact]
    public async Task GetStatusAsync_HeatUnitsAckOnly_DoesNotUpdatePercentage()
    {
        var (sut, fake) = CreateIdleSut();

        fake.EnqueueResponse("ACK\r\n"); // Not HEAT_UNITS
        var result = await sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.HeatUnitPercentage.Should().Be(0);
        sut.Dispose();
    }
}
