namespace HnVue.Common.Enums;

// @MX:ANCHOR SafeState enum - @MX:REASON: 5-level system safety state, IEC 62304 interlock mapping (Idle/Warning/Degraded/Blocked/Emergency)
/// <summary>
/// Represents the system-wide safety state of the console application.
/// Used by the workflow engine to enforce safety interlocks per IEC 62304.
/// </summary>
public enum SafeState
{
    /// <summary>SS-IDLE: Normal operating condition; all subsystems functional. Maps to ALLOW interlock level.</summary>
    Idle,

    /// <summary>
    /// SS-WARNING: Dose reference level exceeded but within operator-override range.
    /// Maps to WARN interlock level (SWR-WF-023). Exposure is permitted after acknowledgement.
    /// Issue #21.
    /// </summary>
    Warning,

    /// <summary>SS-DEGRADED: Reduced functionality; a non-critical subsystem has failed.</summary>
    Degraded,

    /// <summary>SS-BLOCKED: Operations are blocked; a safety interlock is active. Maps to BLOCK interlock level.</summary>
    Blocked,

    /// <summary>SS-EMERGENCY: Immediate stop required; a critical failure has occurred. Maps to EMERGENCY interlock level.</summary>
    Emergency,
}
