using HnVue.Common.Enums;

namespace HnVue.Common.Models;

/// <summary>Provides data for the <see cref="IGeneratorInterface.StateChanged"/> event.</summary>
public sealed class GeneratorStateChangedEventArgs : EventArgs
{
    /// <summary>Gets the previous generator state.</summary>
    public GeneratorState PreviousState { get; }

    /// <summary>Gets the new generator state.</summary>
    public GeneratorState NewState { get; }

    /// <summary>Gets an optional reason for the state change.</summary>
    public string? Reason { get; }

    /// <summary>Initializes a new instance.</summary>
    public GeneratorStateChangedEventArgs(GeneratorState previousState, GeneratorState newState, string? reason = null)
    {
        PreviousState = previousState;
        NewState = newState;
        Reason = reason;
    }
}
