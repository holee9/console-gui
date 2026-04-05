using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.App.Stubs;

/// <summary>
/// Stub implementation of <see cref="IWorkflowEngine"/> used until the HnVue.Workflow module
/// is integrated in Wave 3.
/// All mutating operations return a failure result with a descriptive message.
/// </summary>
internal sealed class StubWorkflowEngine : IWorkflowEngine
{
    private const string NotImplementedMessage =
        "WorkflowEngine not implemented in Wave 2. Available from Wave 3.";

    /// <inheritdoc/>
    public WorkflowState CurrentState => WorkflowState.Idle;

    /// <inheritdoc/>
    public SafeState CurrentSafeState => SafeState.Idle;

    /// <inheritdoc/>
#pragma warning disable CS0067 // Event never used — stub implementation; event required by IWorkflowEngine interface
    public event EventHandler<WorkflowStateChangedEventArgs>? StateChanged;
#pragma warning restore CS0067

    /// <inheritdoc/>
    public Task<Result> StartAsync(
        string patientId,
        string studyInstanceUid,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure(ErrorCode.Unknown, NotImplementedMessage));

    /// <inheritdoc/>
    public Task<Result> TransitionAsync(
        WorkflowState targetState,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure(ErrorCode.Unknown, NotImplementedMessage));

    /// <inheritdoc/>
    public Task<Result> AbortAsync(
        string reason,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure(ErrorCode.Unknown, NotImplementedMessage));
}
