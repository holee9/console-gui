namespace HnVue.Common.Enums;

/// <summary>
/// Tracks the lifecycle status of an individual DICOM C-STORE item in the async pipeline.
/// </summary>
public enum StoreStatus
{
    /// <summary>Item is waiting in the channel to be processed.</summary>
    Pending,

    /// <summary>Item is currently being sent to the PACS.</summary>
    Sending,

    /// <summary>Item was successfully delivered to the PACS.</summary>
    Sent,

    /// <summary>All retry attempts exhausted; the item could not be delivered.</summary>
    Failed,

    /// <summary>A retry attempt is in progress after a transient failure.</summary>
    Retrying,
}
