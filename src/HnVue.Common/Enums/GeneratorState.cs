namespace HnVue.Common.Enums;

// @MX:ANCHOR GeneratorState enum - @MX:REASON: 7-state generator hardware FSM, exposure sequencing, heat unit tracking
/// <summary>
/// Represents the operational state of the X-ray generator hardware.
/// Reported by the generator driver and consumed by the workflow engine.
/// </summary>
public enum GeneratorState
{
    /// <summary>No communication link with the generator hardware.</summary>
    Disconnected,

    /// <summary>Generator is connected and standing by; no preparation in progress.</summary>
    Idle,

    /// <summary>Generator is charging or warming up for the upcoming exposure.</summary>
    Preparing,

    /// <summary>Generator is fully prepared and waiting for the exposure trigger.</summary>
    Ready,

    /// <summary>X-ray exposure is currently active.</summary>
    Exposing,

    /// <summary>Exposure has completed; generator is cooling down and logging dose.</summary>
    Done,

    /// <summary>Generator has reported a hardware or communication error.</summary>
    Error,
}
