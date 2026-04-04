using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Workflow;

/// <summary>
/// Simulates X-ray generator hardware for development, integration testing, and demo environments.
/// Implements <see cref="IGeneratorInterface"/> with deterministic state transitions and configurable delays.
/// </summary>
/// <remarks>
/// State transitions mirror real generator behaviour:
///   Disconnected → Idle (Connect)
///   Idle → Preparing (Prepare) → Ready (auto after PrepareDelayMs)
///   Ready → Exposing (TriggerExposure) → Done (auto after ExposureDelayMs) → Idle
///   Any → Error (failure injection)
///   Any → Disconnected (Disconnect)
/// </remarks>
public sealed class GeneratorSimulator : IGeneratorInterface
{
    /// <summary>Simulated preparation time in milliseconds (default: 500 ms).</summary>
    public int PrepareDelayMs { get; set; } = 500;

    /// <summary>Simulated exposure duration in milliseconds (default: 200 ms).</summary>
    public int ExposureDelayMs { get; set; } = 200;

    /// <summary>When set, the next Connect call will fail with this message.</summary>
    public string? FailNextConnectWith { get; set; }

    /// <summary>When set, the next TriggerExposure call will fail with this message.</summary>
    public string? FailNextExposureWith { get; set; }

    private GeneratorState _currentState = GeneratorState.Disconnected;
    private ExposureParameters? _preparedParameters;
    private readonly object _stateLock = new();
    private double _heatUnitPercentage;

    /// <inheritdoc/>
    public GeneratorState CurrentState
    {
        get { lock (_stateLock) return _currentState; }
    }

    /// <inheritdoc/>
    public event EventHandler<GeneratorStateChangedEventArgs>? StateChanged;

    /// <inheritdoc/>
    public async Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (FailNextConnectWith is not null)
        {
            var msg = FailNextConnectWith;
            FailNextConnectWith = null;
            return Result.Failure(ErrorCode.GeneratorNotReady, msg);
        }

        TransitionState(GeneratorState.Idle);
        _heatUnitPercentage = 0;

        return await Task.FromResult(Result.Success()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        TransitionState(GeneratorState.Disconnected);
        _preparedParameters = null;

        return await Task.FromResult(Result.Success()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result> PrepareAsync(ExposureParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        lock (_stateLock)
        {
            if (_currentState != GeneratorState.Idle)
                return Result.Failure(ErrorCode.GeneratorNotReady,
                    $"Generator must be Idle to prepare; current state: '{_currentState}'.");
        }

        TransitionState(GeneratorState.Preparing);
        _preparedParameters = parameters;

        // Simulate preparation delay, then auto-transition to Ready
        await Task.Delay(PrepareDelayMs, cancellationToken).ConfigureAwait(false);
        TransitionState(GeneratorState.Ready);

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> TriggerExposureAsync(CancellationToken cancellationToken = default)
    {
        if (FailNextExposureWith is not null)
        {
            var msg = FailNextExposureWith;
            FailNextExposureWith = null;
            TransitionState(GeneratorState.Error, msg);
            return Result.Failure(ErrorCode.ExposureAborted, msg);
        }

        lock (_stateLock)
        {
            if (_currentState != GeneratorState.Ready)
                return Result.Failure(ErrorCode.GeneratorNotReady,
                    $"Generator must be Ready to expose; current state: '{_currentState}'.");
        }

        TransitionState(GeneratorState.Exposing);

        // Simulate exposure duration
        await Task.Delay(ExposureDelayMs, cancellationToken).ConfigureAwait(false);

        // Accumulate simulated heat units (each exposure adds 5%)
        _heatUnitPercentage = Math.Min(100.0, _heatUnitPercentage + 5.0);

        TransitionState(GeneratorState.Done);

        // Auto-transition back to Idle after done
        TransitionState(GeneratorState.Idle);

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> AbortAsync(CancellationToken cancellationToken = default)
    {
        TransitionState(GeneratorState.Error, "Abort requested");
        return await Task.FromResult(Result.Success()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<GeneratorStatus>> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        GeneratorState state;
        lock (_stateLock) state = _currentState;

        var status = new GeneratorStatus
        {
            State = state,
            HeatUnitPercentage = _heatUnitPercentage,
            IsReadyToExpose = state == GeneratorState.Ready,
            Timestamp = DateTimeOffset.UtcNow,
        };

        return await Task.FromResult(Result.Success(status)).ConfigureAwait(false);
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private void TransitionState(GeneratorState newState, string? reason = null)
    {
        GeneratorState previous;
        lock (_stateLock)
        {
            previous = _currentState;
            _currentState = newState;
        }

        if (previous != newState)
            StateChanged?.Invoke(this, new GeneratorStateChangedEventArgs(previous, newState, reason));
    }
}
