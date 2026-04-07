namespace HnVue.Detector;

/// <summary>
/// Immutable network configuration for the detector communication link.
/// Used by <see cref="OwnDetector.OwnDetectorAdapter"/> and third-party adapters
/// that communicate over TCP/IP (GigE Vision or proprietary protocol).
/// </summary>
/// <param name="Host">Detector IP address or hostname (e.g., "192.168.1.100").</param>
/// <param name="Port">Detector command port (default: 8888).</param>
/// <param name="ReadoutTimeoutMs">
/// Maximum time in ms to wait for image readout after exposure trigger (default: 5000 ms).
/// DR detectors typically complete readout in 1–3 seconds.
/// </param>
/// <param name="ArmTimeoutMs">
/// Maximum time in ms to wait for the detector to confirm ARM status (default: 2000 ms).
/// </param>
public record DetectorConfig(
    string Host,
    int Port = 8888,
    int ReadoutTimeoutMs = 5000,
    int ArmTimeoutMs = 2000);
