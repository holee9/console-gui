namespace HnVue.Detector.ThirdParty.Hme;

/// <summary>
/// Configuration for <see cref="HmeDetectorAdapter"/>.
/// Extends the base <see cref="DetectorConfig"/> with HME-specific parameters.
/// </summary>
/// <param name="Host">Detector IP address or hostname.</param>
/// <param name="Port">Detector command port (default: 8888).</param>
/// <param name="ReadoutTimeoutMs">Maximum readout wait time in ms (default: 5000).</param>
/// <param name="ArmTimeoutMs">Maximum arm confirmation wait time in ms (default: 2000).</param>
/// <param name="ParamFilePath">
/// Path to the detector parameter file (e.g., S4335-WA.par).
/// Located in: sdk/third-party/hme-licence/HME/2G_SDK/XAS_W_2G_SampleCode/Debug/param/
/// </param>
/// <param name="Model">Detector model identifier (default: "S4335-WA").
/// Supported: S4335-WA, S4335-WF, S4343-WA.</param>
public sealed record HmeDetectorConfig(
    string Host,
    int Port = 8888,
    int ReadoutTimeoutMs = 5000,
    int ArmTimeoutMs = 2000,
    string? ParamFilePath = null,
    string Model = "S4335-WA")
    : DetectorConfig(Host, Port, ReadoutTimeoutMs, ArmTimeoutMs);
