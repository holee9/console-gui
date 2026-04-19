using HnVue.Common.Enums;

namespace HnVue.Common.Models;

// @MX:NOTE DoseInterlockEventArgs - Event payload for dose interlock state transitions, published to UI for safety notification
/// <summary>
/// Event arguments raised when a dose interlock level transition occurs.
/// Carries the interlock level, associated study, and timestamp for audit trail.
/// </summary>
/// <param name="Level">The interlock level that was triggered.</param>
/// <param name="StudyInstanceUid">DICOM Study Instance UID associated with the interlock event.</param>
/// <param name="Timestamp">UTC timestamp of the interlock event.</param>
/// <param name="Reason">Human-readable reason for the interlock activation.</param>
/// <param name="RequiresPhysicalReset">
/// True when the interlock requires physical hardware reset (Emergency level only).
/// System-wide safety flag is set and cannot be cleared through software.
/// </param>
public sealed class DoseInterlockEventArgs(
    DoseValidationLevel level,
    string studyInstanceUid,
    DateTimeOffset timestamp,
    string reason,
    bool requiresPhysicalReset) : EventArgs
{
    /// <summary>The interlock level that was triggered.</summary>
    public DoseValidationLevel Level { get; } = level;

    /// <summary>DICOM Study Instance UID associated with the interlock event.</summary>
    public string StudyInstanceUid { get; } = studyInstanceUid;

    /// <summary>UTC timestamp of the interlock event.</summary>
    public DateTimeOffset Timestamp { get; } = timestamp;

    /// <summary>Human-readable reason for the interlock activation.</summary>
    public string Reason { get; } = reason;

    /// <summary>
    /// True when the interlock requires physical hardware reset (Emergency level only).
    /// System-wide safety flag is set and cannot be cleared through software.
    /// </summary>
    public bool RequiresPhysicalReset { get; } = requiresPhysicalReset;
}
