namespace HnVue.Common.Enums;

/// <summary>
/// Represents the state of a single acquisition workflow session.
/// Transitions are enforced by <c>IWorkflowEngine</c>.
/// </summary>
public enum WorkflowState
{
    /// <summary>No active session; the system is awaiting patient selection.</summary>
    Idle,

    /// <summary>A patient has been selected and the session has been opened.</summary>
    PatientSelected,

    /// <summary>An acquisition protocol has been loaded for the selected patient.</summary>
    ProtocolLoaded,

    /// <summary>Generator and detector are armed; exposure may be triggered.</summary>
    ReadyToExpose,

    /// <summary>X-ray exposure is in progress.</summary>
    Exposing,

    /// <summary>Detector is reading out and transferring image data.</summary>
    ImageAcquiring,

    /// <summary>Raw image is being processed (windowing, noise reduction, etc.).</summary>
    ImageProcessing,

    /// <summary>Processed image is presented to the user for review and approval.</summary>
    ImageReview,

    /// <summary>Workflow session has been completed successfully.</summary>
    Completed,

    /// <summary>An unrecoverable error has occurred; manual intervention required.</summary>
    Error,
}
