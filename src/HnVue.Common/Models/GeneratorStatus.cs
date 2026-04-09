using HnVue.Common.Enums;

namespace HnVue.Common.Models;

// @MX:NOTE GeneratorStatus record - X-ray generator health with heat unit percentage, ready state
/// <summary>Represents the current status reported by the X-ray generator.</summary>
public sealed record GeneratorStatus
{
    /// <summary>Gets the current generator state.</summary>
    public GeneratorState State { get; init; }

    /// <summary>Gets the current heat unit loading percentage (0-100).</summary>
    public double HeatUnitPercentage { get; init; }

    /// <summary>Gets whether the generator is ready to accept an exposure command.</summary>
    public bool IsReadyToExpose { get; init; }

    /// <summary>Gets the timestamp of this status reading.</summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
