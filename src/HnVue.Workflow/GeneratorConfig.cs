using System.IO.Ports;

namespace HnVue.Workflow;

/// <summary>
/// Selects the wire-protocol variant for the connected X-ray generator.
/// </summary>
public enum GeneratorProtocol
{
    /// <summary>Sedecal binary frame protocol (STX / ETX + XOR checksum).</summary>
    Sedecal,

    /// <summary>CPI ASCII text line protocol (CRLF-terminated).</summary>
    Cpi,
}

/// <summary>
/// Immutable configuration for <see cref="GeneratorSerialPort"/>.
/// Describes the RS-232 port and the protocol variant of the connected generator.
/// </summary>
/// <param name="PortName">Windows COM port name, e.g. <c>"COM3"</c>.</param>
/// <param name="BaudRate">Serial baud rate. Common values: 9600, 19200, 57600. Default: 9600.</param>
/// <param name="DataBits">Number of data bits per character. Default: 8.</param>
/// <param name="Parity">Parity mode. Default: <see cref="System.IO.Ports.Parity.None"/>.</param>
/// <param name="StopBits">Stop-bit configuration. Default: <see cref="System.IO.Ports.StopBits.One"/>.</param>
/// <param name="TimeoutMs">
/// Maximum milliseconds to wait for a generator response.
/// Applied to READY wait (PrepareAsync) and EXPOSURE_DONE wait (TriggerExposureAsync).
/// Default: 5000 ms.
/// </param>
/// <param name="Protocol">
/// Selects Sedecal binary-frame or CPI ASCII-line protocol. Default: <see cref="GeneratorProtocol.Sedecal"/>.
/// </param>
/// <remarks>
/// IEC 62304 §5.3.6 — configuration record for the WF-2xx generator communication module.
/// Do not store port credentials in application settings files.
/// </remarks>
public sealed record GeneratorConfig(
    string PortName,
    int BaudRate = 9600,
    int DataBits = 8,
    Parity Parity = Parity.None,
    StopBits StopBits = StopBits.One,
    int TimeoutMs = 5000,
    GeneratorProtocol Protocol = GeneratorProtocol.Sedecal);
