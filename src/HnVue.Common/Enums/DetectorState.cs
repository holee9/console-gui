namespace HnVue.Common.Enums;

/// <summary>
/// Represents the operational state of the flat-panel detector (FPD) hardware.
/// Reported by the detector driver and consumed by the workflow engine.
/// </summary>
public enum DetectorState
{
    /// <summary>No communication link with the detector hardware.</summary>
    Disconnected,

    /// <summary>Detector is connected, calibration loaded, standing by.</summary>
    Idle,

    /// <summary>Detector is armed and waiting for the X-ray exposure trigger.</summary>
    Armed,

    /// <summary>Detector is reading out pixel data after exposure.</summary>
    Acquiring,

    /// <summary>Readout complete; image data is available for pickup.</summary>
    ImageReady,

    /// <summary>Detector has reported a hardware or communication error.</summary>
    Error,
}
