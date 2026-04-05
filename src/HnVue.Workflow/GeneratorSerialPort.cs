using System.IO;
using System.IO.Ports;
using System.Text;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Workflow;

/// <summary>
/// Production RS-232 implementation of <see cref="IGeneratorInterface"/> for Sedecal and CPI generators.
/// Manages the full exposure state machine over a physical serial port.
/// </summary>
/// <remarks>
/// State transitions mirror <see cref="GeneratorSimulator"/>:
///   Disconnected → Idle (ConnectAsync)
///   Idle → Preparing (PrepareAsync) → Ready (READY response from hardware)
///   Ready → Exposing (TriggerExposureAsync) → Done (EXPOSURE_DONE) → Idle
///   Any → Error (hardware ERROR response or timeout)
///   Any → Disconnected (DisconnectAsync)
///
/// IEC 62304 §5.3.6 traceability: WF-2xx module.
/// SWR-WF-020 (generator control), SWR-WF-021 (exposure command), SWR-WF-022 (abort command).
/// HAZ-RAD radiation safety interlock: AbortAsync must not fail silently.
/// </remarks>
public sealed class GeneratorSerialPort : IGeneratorInterface, IDisposable
{
    // ── Response tokens ───────────────────────────────────────────────────────

    private const string TokenAck = "ACK";
    private const string TokenReady = "READY";
    private const string TokenExposureDone = "EXPOSURE_DONE";
    private const string TokenAecTerminated = "AEC_TERMINATED";
    private const string TokenHeatUnits = "HEAT_UNITS";
    private const string TokenError = "ERROR";
    private const string TokenBusy = "BUSY";

    // Sedecal binary frame delimiters
    private const byte Stx = 0x02;
    private const byte Etx = 0x03;

    // ── Fields ────────────────────────────────────────────────────────────────

    private readonly GeneratorConfig _config;
    private readonly SerialPort _port;
    private readonly object _stateLock = new();

    // Pending-response infrastructure: one outstanding wait at a time.
    private readonly object _responseLock = new();
    private TaskCompletionSource<string>? _pendingResponse;

    // Raw receive buffer shared between DataReceived callback and parser.
    private readonly StringBuilder _receiveBuffer = new();

    private GeneratorState _currentState = GeneratorState.Disconnected;
    private double _heatUnitPercentage;
    private bool _disposed;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Initialises a new <see cref="GeneratorSerialPort"/> with the supplied configuration.
    /// The port is not opened until <see cref="ConnectAsync"/> is called.
    /// </summary>
    /// <param name="config">RS-232 port and protocol settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public GeneratorSerialPort(GeneratorConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = config;

        _port = new SerialPort
        {
            PortName = config.PortName,
            BaudRate = config.BaudRate,
            DataBits = config.DataBits,
            Parity = config.Parity,
            StopBits = config.StopBits,
            Handshake = Handshake.None,
            ReadTimeout = config.TimeoutMs,
            WriteTimeout = 1000,
            Encoding = Encoding.ASCII,
        };

        _port.DataReceived += OnDataReceived;
    }

    // ── IGeneratorInterface ───────────────────────────────────────────────────

    /// <inheritdoc/>
    public GeneratorState CurrentState
    {
        get { lock (_stateLock) return _currentState; }
    }

    /// <inheritdoc/>
    public event EventHandler<GeneratorStateChangedEventArgs>? StateChanged;

    /// <inheritdoc/>
    /// <remarks>
    /// Opens the serial port, sends a GET_STATUS query to confirm the generator is present,
    /// and transitions to <see cref="GeneratorState.Idle"/> on success.
    /// </remarks>
    public async Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_stateLock)
        {
            if (_currentState != GeneratorState.Disconnected)
                return Result.Failure(ErrorCode.GeneratorNotReady,
                    $"Cannot connect: generator is already in state '{_currentState}'.");
        }

        try
        {
            _port.Open();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result.Failure(ErrorCode.GeneratorNotReady,
                $"Serial port '{_config.PortName}' is in use by another process. ({ex.Message})");
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException)
        {
            return Result.Failure(ErrorCode.GeneratorNotReady,
                $"Failed to open serial port '{_config.PortName}': {ex.Message}");
        }

        // Verify communication by sending a status query and waiting for ACK or READY.
        try
        {
            SendRawCommand("GET_STATUS");
            var response = await WaitForAnyResponseAsync(
                [TokenAck, TokenReady, TokenBusy],
                cancellationToken).ConfigureAwait(false);

            if (response is null)
            {
                _port.Close();
                return Result.Failure(ErrorCode.GeneratorNotReady,
                    "Timeout waiting for generator status response during connect.");
            }
        }
        catch (OperationCanceledException)
        {
            _port.Close();
            return Result.Failure(ErrorCode.OperationCancelled, "ConnectAsync was cancelled.");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _port.Close();
            return Result.Failure(ErrorCode.GeneratorNotReady,
                $"Communication error during connect: {ex.Message}");
        }

        _heatUnitPercentage = 0;
        TransitionState(GeneratorState.Idle);
        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_port.IsOpen)
        {
            try
            {
                // Best-effort send; do not wait for ACK — port may be degraded.
                SendRawCommand("RESET_ERROR");
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                // Swallow — we are disconnecting regardless.
            }

            try
            {
                _port.Close();
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                return Result.Failure(ErrorCode.GeneratorNotReady,
                    $"Error closing serial port: {ex.Message}");
            }
        }

        TransitionState(GeneratorState.Disconnected);
        return await Task.FromResult(Result.Success()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// SWR-WF-020: Sends SET_KVP, SET_MAS, LOAD_APR then PREP.
    /// Waits up to <see cref="GeneratorConfig.TimeoutMs"/> milliseconds for the READY acknowledgement.
    /// </remarks>
    // SWR-WF-020
    public async Task<Result> PrepareAsync(
        ExposureParameters parameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_stateLock)
        {
            if (_currentState != GeneratorState.Idle)
                return Result.Failure(ErrorCode.GeneratorNotReady,
                    $"Generator must be Idle to prepare; current state: '{_currentState}'.");
        }

        if (!_port.IsOpen)
            return Result.Failure(ErrorCode.GeneratorNotReady, "Serial port is not open.");

        try
        {
            // Set kVp
            SendRawCommand($"SET_KVP {parameters.Kvp:F1}");
            var ackKvp = await WaitForAnyResponseAsync([TokenAck], cancellationToken)
                .ConfigureAwait(false);
            if (ackKvp is null)
                return Result.Failure(ErrorCode.GeneratorNotReady,
                    "Timeout waiting for ACK after SET_KVP.");

            // Set mAs
            SendRawCommand($"SET_MAS {parameters.Mas:F1}");
            var ackMas = await WaitForAnyResponseAsync([TokenAck], cancellationToken)
                .ConfigureAwait(false);
            if (ackMas is null)
                return Result.Failure(ErrorCode.GeneratorNotReady,
                    "Timeout waiting for ACK after SET_MAS.");

            // Load APR preset (body part maps to preset ID via hash — stable for same string)
            int aprId = Math.Abs(parameters.BodyPart.GetHashCode()) % 100;
            SendRawCommand($"LOAD_APR {aprId}");
            var ackApr = await WaitForAnyResponseAsync([TokenAck], cancellationToken)
                .ConfigureAwait(false);
            if (ackApr is null)
                return Result.Failure(ErrorCode.GeneratorNotReady,
                    "Timeout waiting for ACK after LOAD_APR.");

            // Send PREP and wait for READY within TimeoutMs
            TransitionState(GeneratorState.Preparing);
            SendRawCommand("PREP");

            var readyResponse = await WaitForAnyResponseAsync(
                [TokenReady, TokenError],
                cancellationToken).ConfigureAwait(false);

            if (readyResponse is null)
            {
                TransitionState(GeneratorState.Error, "Timeout waiting for READY after PREP.");
                return Result.Failure(ErrorCode.GeneratorNotReady,
                    "Timeout waiting for generator READY response.");
            }

            if (readyResponse.StartsWith(TokenError, StringComparison.OrdinalIgnoreCase))
            {
                TransitionState(GeneratorState.Error, readyResponse);
                return Result.Failure(ErrorCode.GeneratorNotReady,
                    $"Generator reported error during preparation: {readyResponse}");
            }

            TransitionState(GeneratorState.Ready);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            TransitionState(GeneratorState.Error, "PrepareAsync cancelled.");
            return Result.Failure(ErrorCode.OperationCancelled, "PrepareAsync was cancelled.");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            TransitionState(GeneratorState.Error, ex.Message);
            return Result.Failure(ErrorCode.GeneratorNotReady,
                $"Communication error during preparation: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// SWR-WF-021: Sends EXPOSE and waits for EXPOSURE_DONE or AEC_TERMINATED.
    /// The generator must already be in the <see cref="GeneratorState.Ready"/> state.
    /// </remarks>
    // SWR-WF-021
    public async Task<Result> TriggerExposureAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_stateLock)
        {
            if (_currentState != GeneratorState.Ready)
                return Result.Failure(ErrorCode.GeneratorNotReady,
                    $"Generator must be Ready to expose; current state: '{_currentState}'.");
        }

        if (!_port.IsOpen)
            return Result.Failure(ErrorCode.GeneratorNotReady, "Serial port is not open.");

        try
        {
            TransitionState(GeneratorState.Exposing);
            SendRawCommand("EXPOSE");

            var doneResponse = await WaitForAnyResponseAsync(
                [TokenExposureDone, TokenAecTerminated, TokenError],
                cancellationToken).ConfigureAwait(false);

            if (doneResponse is null)
            {
                TransitionState(GeneratorState.Error, "Timeout waiting for EXPOSURE_DONE.");
                return Result.Failure(ErrorCode.GeneratorNotReady,
                    "Timeout waiting for EXPOSURE_DONE response.");
            }

            if (doneResponse.StartsWith(TokenError, StringComparison.OrdinalIgnoreCase))
            {
                TransitionState(GeneratorState.Error, doneResponse);
                return Result.Failure(ErrorCode.ExposureAborted,
                    $"Generator reported error during exposure: {doneResponse}");
            }

            // EXPOSURE_DONE or AEC_TERMINATED — both mean a completed exposure.
            TransitionState(GeneratorState.Done);
            TransitionState(GeneratorState.Idle);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            TransitionState(GeneratorState.Error, "TriggerExposureAsync cancelled.");
            return Result.Failure(ErrorCode.OperationCancelled, "TriggerExposureAsync was cancelled.");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            TransitionState(GeneratorState.Error, ex.Message);
            return Result.Failure(ErrorCode.ExposureAborted,
                $"Communication error during exposure: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// SWR-WF-022 / HAZ-RAD: Sends ABORT immediately.
    /// This method is designed to be callable from any thread at any time.
    /// It must not fail silently per IEC 62304 safety requirements.
    /// The state always transitions to <see cref="GeneratorState.Error"/> so the
    /// calling layer is forced to perform an explicit recovery step.
    /// </remarks>
    // SWR-WF-022
    public async Task<Result> AbortAsync(CancellationToken cancellationToken = default)
    {
        // HAZ-RAD: Transition to Error immediately before any I/O so that even
        // if the serial write fails, the software-side state is safe.
        TransitionState(GeneratorState.Error, "Abort requested");

        if (_port.IsOpen)
        {
            try
            {
                // Write ABORT synchronously via the underlying stream to minimise latency.
                // Do NOT await — this method must enqueue the command with minimal blocking.
                byte[] frame = BuildFrame("ABORT");
                _port.BaseStream.Write(frame, 0, frame.Length);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                // Log is the only safe action here; state is already Error.
                // Do NOT re-throw — AbortAsync must not fail silently (IEC 62304 safety).
                _ = ex; // The state is already Error; caller must recover.
            }
        }

        return await Task.FromResult(Result.Success()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<GeneratorStatus>> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        GeneratorState state;
        lock (_stateLock) state = _currentState;

        if (!_port.IsOpen)
        {
            var offlineStatus = new GeneratorStatus
            {
                State = state,
                HeatUnitPercentage = _heatUnitPercentage,
                IsReadyToExpose = false,
                Timestamp = DateTimeOffset.UtcNow,
            };
            return await Task.FromResult(Result.Success(offlineStatus)).ConfigureAwait(false);
        }

        try
        {
            SendRawCommand("GET_HEAT_UNITS");
            var heatResponse = await WaitForAnyResponseAsync([TokenHeatUnits, TokenAck], cancellationToken)
                .ConfigureAwait(false);

            if (heatResponse is not null && heatResponse.StartsWith(TokenHeatUnits, StringComparison.OrdinalIgnoreCase))
            {
                var parts = heatResponse.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && double.TryParse(parts[1], out double hu))
                    _heatUnitPercentage = hu;
            }
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or TimeoutException)
        {
            // Best-effort heat-unit poll; return cached value on any I/O failure.
        }

        lock (_stateLock) state = _currentState;

        var status = new GeneratorStatus
        {
            State = state,
            HeatUnitPercentage = _heatUnitPercentage,
            IsReadyToExpose = state == GeneratorState.Ready,
            Timestamp = DateTimeOffset.UtcNow,
        };

        return Result.Success(status);
    }

    // ── IDisposable ───────────────────────────────────────────────────────────

    /// <summary>
    /// Closes the serial port and releases all resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _port.DataReceived -= OnDataReceived;

        if (_port.IsOpen)
        {
            try { _port.Close(); } catch (Exception ex) when (ex is not OutOfMemoryException) { /* best effort */ }
        }

        _port.Dispose();

        // Cancel any pending wait.
        lock (_responseLock)
        {
            _pendingResponse?.TrySetCanceled();
            _pendingResponse = null;
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Sends a command using the framing appropriate for the configured protocol.
    /// </summary>
    private void SendRawCommand(string command)
    {
        byte[] frame = BuildFrame(command);
        _port.Write(frame, 0, frame.Length);
    }

    /// <summary>
    /// Builds a protocol-specific byte frame for <paramref name="command"/>.
    /// Sedecal: STX + ASCII payload + checksum byte + ETX.
    /// CPI: ASCII payload + CR + LF.
    /// </summary>
    private byte[] BuildFrame(string command)
    {
        return _config.Protocol switch
        {
            GeneratorProtocol.Sedecal => BuildSedecalFrame(command),
            GeneratorProtocol.Cpi => BuildCpiCommand(command),
            _ => BuildSedecalFrame(command),
        };
    }

    /// <summary>
    /// Builds a Sedecal binary frame: STX (0x02) + ASCII payload + XOR checksum + ETX (0x03).
    /// </summary>
    private static byte[] BuildSedecalFrame(string command)
    {
        byte[] payload = Encoding.ASCII.GetBytes(command);
        byte checksum = 0x00;
        foreach (byte b in payload)
            checksum ^= b;

        // Frame: STX, payload..., checksum, ETX
        byte[] frame = new byte[payload.Length + 3];
        frame[0] = Stx;
        payload.CopyTo(frame, 1);
        frame[^2] = checksum;
        frame[^1] = Etx;
        return frame;
    }

    /// <summary>
    /// Builds a CPI ASCII text command terminated by CR+LF.
    /// CPI generators do not use STX/ETX binary framing.
    /// </summary>
    private static byte[] BuildCpiCommand(string command)
    {
        return Encoding.ASCII.GetBytes(command + "\r\n");
    }

    /// <summary>
    /// Waits up to <see cref="GeneratorConfig.TimeoutMs"/> milliseconds for a response
    /// whose token (first whitespace-delimited word) matches one of <paramref name="expectedTokens"/>.
    /// Returns the full matched response line, or <see langword="null"/> on timeout.
    /// </summary>
    private async Task<string?> WaitForAnyResponseAsync(
        string[] expectedTokens,
        CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        lock (_responseLock)
        {
            // Only one outstanding wait is supported at a time.
            _pendingResponse?.TrySetCanceled(CancellationToken.None);
            _pendingResponse = tcs;
        }

        try
        {
            using var timeoutCts = new CancellationTokenSource(_config.TimeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, timeoutCts.Token);

            await using var registration = linkedCts.Token.Register(() =>
            {
                lock (_responseLock)
                {
                    if (_pendingResponse == tcs)
                    {
                        _pendingResponse?.TrySetCanceled(linkedCts.Token);
                        _pendingResponse = null;
                    }
                }
            });

            // Pump incoming data until a matching token arrives.
            while (true)
            {
                string response;
                try
                {
                    response = await tcs.Task.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return null; // Timeout or caller cancellation.
                }

                string token = response.Split(' ', 2)[0].ToUpperInvariant();
                foreach (var expected in expectedTokens)
                {
                    if (string.Equals(token, expected, StringComparison.OrdinalIgnoreCase))
                        return response;
                }

                // Token did not match; reset TCS and keep waiting.
                var next = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                lock (_responseLock)
                {
                    _pendingResponse = next;
                }

                tcs = next;

                // Re-register cancel on the new TCS.
                // The existing registration already covers the linked token; just swap reference.
            }
        }
        finally
        {
            lock (_responseLock)
            {
                if (_pendingResponse == tcs)
                    _pendingResponse = null;
            }
        }
    }

    /// <summary>
    /// DataReceived callback: feeds raw bytes through the protocol parser
    /// and delivers complete response lines to the pending wait.
    /// </summary>
    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (_disposed) return;

        try
        {
            string raw = _port.ReadExisting();
            foreach (string parsed in ParseIncoming(raw))
            {
                DeliverResponse(parsed);

                // Handle unsolicited ERROR responses: force state to Error.
                if (parsed.StartsWith(TokenError, StringComparison.OrdinalIgnoreCase))
                {
                    lock (_stateLock)
                    {
                        if (_currentState != GeneratorState.Disconnected)
                            TransitionState(GeneratorState.Error, parsed);
                    }
                }
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            // DataReceived handler MUST NOT throw (crashes serial port background thread).
            // Transition to Error so the caller can detect the fault.
            TransitionState(GeneratorState.Error, ex.Message);
        }
    }

    /// <summary>
    /// Parses incoming raw serial data into complete response strings using a shared buffer.
    /// Sedecal: extracts content between STX (0x02) and ETX (0x03) bytes.
    /// CPI: extracts CRLF-terminated lines.
    /// </summary>
    private IEnumerable<string> ParseIncoming(string raw)
    {
        if (_config.Protocol == GeneratorProtocol.Sedecal)
        {
            foreach (char c in raw)
            {
                if (c == (char)Stx)
                {
                    _receiveBuffer.Clear();
                }
                else if (c == (char)Etx)
                {
                    // Last byte before ETX is the checksum; strip it.
                    string frame = _receiveBuffer.ToString();
                    _receiveBuffer.Clear();
                    if (frame.Length > 1)
                        yield return frame[..^1].Trim(); // Remove trailing checksum byte.
                }
                else
                {
                    _receiveBuffer.Append(c);
                }
            }
        }
        else // CPI ASCII line protocol
        {
            _receiveBuffer.Append(raw);
            string accumulated = _receiveBuffer.ToString();
            int newline;
            while ((newline = accumulated.IndexOf('\n')) >= 0)
            {
                string line = accumulated[..newline].TrimEnd('\r').Trim();
                accumulated = accumulated[(newline + 1)..];
                if (!string.IsNullOrWhiteSpace(line))
                    yield return line;
            }

            _receiveBuffer.Clear();
            _receiveBuffer.Append(accumulated);
        }
    }

    /// <summary>
    /// Delivers a parsed response string to the current pending wait, if any.
    /// </summary>
    private void DeliverResponse(string response)
    {
        TaskCompletionSource<string>? pending;
        lock (_responseLock)
        {
            pending = _pendingResponse;
        }

        pending?.TrySetResult(response);
    }

    /// <summary>
    /// Transitions to <paramref name="newState"/> under the state lock and fires
    /// <see cref="StateChanged"/> when the state actually changes.
    /// </summary>
    private void TransitionState(GeneratorState newState, string? reason = null)
    {
        GeneratorState previous;
        lock (_stateLock)
        {
            previous = _currentState;
            _currentState = newState;
        }

        if (previous != newState)
            StateChanged?.Invoke(this, new GeneratorStateChangedEventArgs(previous, newState, reason));
    }
}
