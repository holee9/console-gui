using System.IO;
using System.IO.Ports;
using System.Text;
using HnVue.Workflow;

namespace HnVue.Workflow.Tests;

/// <summary>
/// In-memory fake serial port adapter for unit testing <see cref="GeneratorSerialPort"/>
/// without physical hardware.
/// </summary>
/// <remarks>
/// Usage pattern:
/// 1. Construct with the expected protocol.
/// 2. Call <see cref="EnqueueResponse"/> to pre-stage responses.
/// 3. Execute the SUT method under test.
/// 4. Inspect <see cref="SentCommands"/> to verify outgoing commands.
/// </remarks>
internal sealed class FakeSerialPortAdapter : ISerialPortAdapter
{
    private readonly Queue<string> _responseQueue = new();
    private readonly object _lock = new();
    private bool _isOpen;
    private bool _disposed;

    /// <summary>Gets all raw byte sequences that were written via <see cref="Write"/>.</summary>
    public List<byte[]> WrittenFrames { get; } = [];

    /// <summary>Gets ASCII command strings decoded from written frames (convenience helper).</summary>
    public List<string> SentCommands { get; } = [];

    // ── ISerialPortAdapter ────────────────────────────────────────────────────

    /// <inheritdoc/>
    public bool IsOpen => _isOpen;

    /// <inheritdoc/>
    public Stream BaseStream { get; } = new MemoryStream();

    /// <inheritdoc/>
    public event SerialDataReceivedEventHandler? DataReceived;

    /// <inheritdoc/>
    public void Open()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _isOpen = true;
    }

    /// <inheritdoc/>
    public void Close() => _isOpen = false;

    /// <inheritdoc/>
    public void Write(byte[] buffer, int offset, int count)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        byte[] frame = new byte[count];
        Array.Copy(buffer, offset, frame, 0, count);
        WrittenFrames.Add(frame);

        // Decode and record command string for assertion convenience.
        string decoded = Encoding.ASCII.GetString(frame).Trim('\x02', '\x03', '\r', '\n');
        // Strip trailing checksum byte for Sedecal frames that end before ETX.
        if (decoded.Length > 1 && decoded[^1] < 0x20)
            decoded = decoded[..^1];
        SentCommands.Add(decoded.Trim());

        // Fire the enqueued response (if any) on a background thread to simulate
        // the serial port's asynchronous DataReceived event.
        DeliverNextResponse();
    }

    /// <inheritdoc/>
    public string ReadExisting()
    {
        lock (_lock)
        {
            return _responseQueue.TryDequeue(out string? response) ? response : string.Empty;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _disposed = true;
        _isOpen = false;
        BaseStream.Dispose();
    }

    // ── Test helpers ──────────────────────────────────────────────────────────

    /// <summary>
    /// Enqueues a response that will be delivered to the SUT when the next command is sent.
    /// Multiple calls queue multiple responses delivered in FIFO order.
    /// </summary>
    /// <param name="response">The raw response string (e.g. "ACK", "READY", "ERROR bad_param").</param>
    public void EnqueueResponse(string response)
    {
        lock (_lock)
        {
            _responseQueue.Enqueue(response);
        }
    }

    /// <summary>
    /// Opens the port without requiring explicit <see cref="Open"/> call.
    /// Convenience method for tests that need the port pre-opened.
    /// </summary>
    public void ForceOpen() => _isOpen = true;

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Delivers the next queued response by raising <see cref="DataReceived"/> on a thread-pool thread.
    /// This mirrors the asynchronous nature of the real serial port driver.
    /// </summary>
    private void DeliverNextResponse()
    {
        if (DataReceived is null) return;

        // Snapshot the handler reference to avoid race on unsubscribe.
        var handler = DataReceived;

        // Fire asynchronously on thread-pool to avoid deadlock on _responseLock
        // inside GeneratorSerialPort.WaitForAnyResponseAsync.
        _ = Task.Run(() =>
        {
            // Small yield to ensure the SUT has registered its TCS before delivery.
            Thread.Sleep(10);
            handler.Invoke(this, null!);
        });
    }
}
