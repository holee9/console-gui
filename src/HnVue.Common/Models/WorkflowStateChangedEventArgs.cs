using HnVue.Common.Enums;

namespace HnVue.Common.Models;

/// <summary>
/// Provides data for the <c>IWorkflowEngine.StateChanged</c> event.
/// </summary>
public sealed class WorkflowStateChangedEventArgs : EventArgs
{
    /// <summary>Initialises a new instance of <see cref="WorkflowStateChangedEventArgs"/>.</summary>
    /// <param name="previousState">The workflow state before the transition.</param>
    /// <param name="newState">The workflow state after the transition.</param>
    /// <param name="reason">Optional human-readable reason for the transition (e.g., an abort message).</param>
    public WorkflowStateChangedEventArgs(
        WorkflowState previousState,
        WorkflowState newState,
        string? reason = null)
    {
        PreviousState = previousState;
        NewState = newState;
        Reason = reason;
    }

    /// <summary>Gets the workflow state before the transition.</summary>
    public WorkflowState PreviousState { get; }

    /// <summary>Gets the workflow state after the transition.</summary>
    public WorkflowState NewState { get; }

    /// <summary>Gets an optional human-readable reason for the transition.</summary>
    public string? Reason { get; }
}
