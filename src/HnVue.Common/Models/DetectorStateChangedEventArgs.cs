using HnVue.Common.Enums;

namespace HnVue.Common.Models;

/// <summary>Provides data for the <see cref="HnVue.Common.Abstractions.IDetectorInterface.StateChanged"/> event.</summary>
public sealed class DetectorStateChangedEventArgs : EventArgs
{
    /// <summary>Gets the previous detector state.</summary>
    public DetectorState PreviousState { get; }

    /// <summary>Gets the new detector state.</summary>
    public DetectorState NewState { get; }

    /// <summary>Gets an optional reason for the state change.</summary>
    public string? Reason { get; }

    /// <summary>Initializes a new instance.</summary>
    public DetectorStateChangedEventArgs(DetectorState previousState, DetectorState newState, string? reason = null)
    {
        PreviousState = previousState;
        NewState = newState;
        Reason = reason;
    }
}
