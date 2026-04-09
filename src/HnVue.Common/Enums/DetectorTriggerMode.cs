namespace HnVue.Common.Enums;

// @MX:NOTE DetectorTriggerMode enum - Sync (clinical DR) vs FreeRun (dev/test), dose minimization
/// <summary>
/// Specifies how the detector acquisition is triggered.
/// </summary>
public enum DetectorTriggerMode
{
    /// <summary>
    /// Hardware sync trigger: detector waits for the X-ray exposure signal before reading out.
    /// Standard mode for clinical DR acquisitions. Minimises patient dose.
    /// </summary>
    Sync,

    /// <summary>
    /// Software free-run trigger: detector acquires continuously without waiting for a hardware signal.
    /// Used for development, integration testing, and demo environments.
    /// </summary>
    FreeRun,
}
