using System.IO;
using System.IO.Ports;

namespace HnVue.Workflow;

/// <summary>
/// Production wrapper around <see cref="System.IO.Ports.SerialPort"/> that implements
/// <see cref="ISerialPortAdapter"/> for dependency injection and testability.
/// </summary>
internal sealed class SerialPortAdapter : ISerialPortAdapter
{
    private readonly SerialPort _port;

    /// <summary>
    /// Initialises a new <see cref="SerialPortAdapter"/> wrapping the supplied <paramref name="port"/>.
    /// </summary>
    /// <param name="port">The underlying <see cref="SerialPort"/> instance to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="port"/> is null.</exception>
    public SerialPortAdapter(SerialPort port)
    {
        ArgumentNullException.ThrowIfNull(port);
        _port = port;
    }

    /// <inheritdoc/>
    public bool IsOpen => _port.IsOpen;

    /// <inheritdoc/>
    public Stream BaseStream => _port.BaseStream;

    /// <inheritdoc/>
    public void Open() => _port.Open();

    /// <inheritdoc/>
    public void Close() => _port.Close();

    /// <inheritdoc/>
    public void Write(byte[] buffer, int offset, int count) => _port.Write(buffer, offset, count);

    /// <inheritdoc/>
    public string ReadExisting() => _port.ReadExisting();

    /// <inheritdoc/>
    public event SerialDataReceivedEventHandler DataReceived
    {
        add => _port.DataReceived += value;
        remove => _port.DataReceived -= value;
    }

    /// <inheritdoc/>
    public void Dispose() => _port.Dispose();
}
