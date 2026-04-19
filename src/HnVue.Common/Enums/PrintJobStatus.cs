namespace HnVue.Common.Enums;

/// <summary>
/// Represents the status of a DICOM Print Job as reported by the printer SCP via N-GET.
/// Maps to the Print Job SOP Class status attributes (DICOM PS3.4, Print Management).
/// </summary>
public enum PrintJobStatus
{
    /// <summary>Print job has been created but has not yet started.</summary>
    Pending,

    /// <summary>Print job is actively being processed by the printer.</summary>
    Printing,

    /// <summary>Print job completed successfully.</summary>
    Done,

    /// <summary>Print job failed due to an error condition.</summary>
    Failure,
}
