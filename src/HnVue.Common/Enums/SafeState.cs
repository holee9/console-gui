namespace HnVue.Common.Enums;

/// <summary>
/// Represents the system-wide safety state of the console application.
/// Used by the workflow engine to enforce safety interlocks per IEC 62304.
/// </summary>
public enum SafeState
{
    /// <summary>SS-IDLE: Normal operating condition; all subsystems functional.</summary>
    Idle,

    /// <summary>SS-DEGRADED: Reduced functionality; a non-critical subsystem has failed.</summary>
    Degraded,

    /// <summary>SS-BLOCKED: Operations are blocked; a safety interlock is active.</summary>
    Blocked,

    /// <summary>SS-EMERGENCY: Immediate stop required; a critical failure has occurred.</summary>
    Emergency,
}
