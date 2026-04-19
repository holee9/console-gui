using HnVue.Common.Enums;

namespace HnVue.Common.Models;

/// <summary>
/// Represents a single DICOM file awaiting or undergoing C-STORE transmission.
/// Used by the async pipeline for per-item tracking and status queries.
/// </summary>
/// <param name="FilePath">Absolute path to the DICOM file on disk.</param>
/// <param name="SopInstanceUid">DICOM SOP Instance UID identifying this object uniquely.</param>
/// <param name="Status">Current lifecycle status of the store operation.</param>
/// <param name="Attempts">Number of transmission attempts made so far.</param>
/// <param name="LastAttemptAt">Timestamp of the most recent attempt; null if never attempted.</param>
public sealed record StoreItem(
    string FilePath,
    string SopInstanceUid,
    StoreStatus Status = StoreStatus.Pending,
    int Attempts = 0,
    DateTimeOffset? LastAttemptAt = null);
