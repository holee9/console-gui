namespace HnVue.Common.Enums;

// @MX:NOTE DoseWarnLevel enum - Segmentation of Warn interlock level per IEC 60601-2-54, WarnLow (auto-proceed) vs WarnHigh (acknowledgment required)
/// <summary>
/// Specifies the sub-level within the Warn tier of the dose interlock system.
/// Used to differentiate between informational warnings and those requiring
/// explicit operator acknowledgment.
/// </summary>
public enum DoseWarnLevel
{
    /// <summary>No warning sub-level applies (dose is Allow, Block, or Emergency).</summary>
    None,

    /// <summary>
    /// Low warning: DAP between 1x and 1.5x DRL. Informational only; auto-proceed permitted.
    /// </summary>
    Low,

    /// <summary>
    /// High warning: DAP between 1.5x and 2x DRL. Explicit operator acknowledgment required.
    /// </summary>
    High,
}
