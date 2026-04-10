using HnVue.Common.Enums;

namespace HnVue.Common.Models;

// @MX:NOTE DetectorStatus record - FPD health report with temperature, serial number, firmware version
/// <summary>Represents the current status reported by the flat-panel detector.</summary>
public sealed record DetectorStatus
{
    /// <summary>Gets the current detector state.</summary>
    public DetectorState State { get; init; }

    /// <summary>Gets whether the detector is ready to be armed for acquisition.</summary>
    public bool IsReadyToArm { get; init; }

    /// <summary>Gets the detector panel temperature in degrees Celsius.</summary>
    public double TemperatureCelsius { get; init; }

    /// <summary>Gets the detector serial number, or <see langword="null"/> if not available.</summary>
    public string? SerialNumber { get; init; }

    /// <summary>Gets the detector firmware version string, or <see langword="null"/> if not available.</summary>
    public string? FirmwareVersion { get; init; }

    /// <summary>Gets the timestamp of this status reading.</summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
