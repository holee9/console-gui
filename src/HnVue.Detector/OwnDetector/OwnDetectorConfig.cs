namespace HnVue.Detector.OwnDetector;

/// <summary>
/// Immutable configuration for <see cref="OwnDetectorAdapter"/>.
/// Extends the base <see cref="DetectorConfig"/> with 자사-specific parameters.
/// </summary>
/// <param name="Host">Detector IP address or hostname.</param>
/// <param name="Port">Detector command port (default: 8888).</param>
/// <param name="ReadoutTimeoutMs">Maximum readout wait time in ms (default: 5000).</param>
/// <param name="ArmTimeoutMs">Maximum arm confirmation wait time in ms (default: 2000).</param>
/// <param name="CalibrationPath">
/// Path to the detector calibration folder containing gain/offset correction maps.
/// Typically: C:\HnVue\Calibration\{SerialNumber}\
/// </param>
/// <param name="BitsPerPixel">Detector pixel depth (default: 14 for 자사 CsI panel).</param>
public sealed record OwnDetectorConfig(
    string Host,
    int Port = 8888,
    int ReadoutTimeoutMs = 5000,
    int ArmTimeoutMs = 2000,
    string? CalibrationPath = null,
    int BitsPerPixel = 14)
    : DetectorConfig(Host, Port, ReadoutTimeoutMs, ArmTimeoutMs);
