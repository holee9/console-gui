using System.IO;
using System.IO.Ports;

namespace HnVue.Workflow;

/// <summary>
/// Abstraction over <see cref="System.IO.Ports.SerialPort"/> to enable unit testing
/// of <see cref="GeneratorSerialPort"/> without physical hardware.
/// </summary>
internal interface ISerialPortAdapter : IDisposable
{
    /// <summary>Gets a value indicating whether the serial port is open.</summary>
    bool IsOpen { get; }

    /// <summary>Gets the underlying stream for low-latency direct writes (e.g. ABORT).</summary>
    Stream BaseStream { get; }

    /// <summary>Opens the serial port connection.</summary>
    void Open();

    /// <summary>Closes the serial port connection.</summary>
    void Close();

    /// <summary>Writes a binary frame to the port.</summary>
    void Write(byte[] buffer, int offset, int count);

    /// <summary>Reads all immediately available bytes as a string.</summary>
    string ReadExisting();

    /// <summary>Raised when data is received on the serial port.</summary>
    event SerialDataReceivedEventHandler DataReceived;
}
