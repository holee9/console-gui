using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Detector;

/// <summary>
/// Simulates FPD detector hardware for development, integration testing, and demo environments.
/// Implements <see cref="IDetectorInterface"/> with deterministic state transitions and configurable delays.
/// </summary>
/// <remarks>
/// State transitions mirror real detector behaviour:
///   Disconnected → Idle   (ConnectAsync)
///   Idle         → Armed  (ArmAsync) → Acquiring (auto after ArmDelayMs)
///   Acquiring    → ImageReady (auto after ReadoutDelayMs) → fires ImageAcquired → Idle
///   Any          → Error  (failure injection)
///   Any          → Disconnected (DisconnectAsync)
/// </remarks>
public sealed class DetectorSimulator : IDetectorInterface
{
    /// <summary>Simulated arm time in milliseconds (default: 200 ms).</summary>
    public int ArmDelayMs { get; set; } = 200;

    /// <summary>Simulated readout time in milliseconds (default: 1000 ms).</summary>
    public int ReadoutDelayMs { get; set; } = 1000;

    /// <summary>Width of the synthetic test image in pixels (default: 64).</summary>
    public int SimulatedImageWidth { get; set; } = 64;

    /// <summary>Height of the synthetic test image in pixels (default: 64).</summary>
    public int SimulatedImageHeight { get; set; } = 64;

    /// <summary>When set, the next ConnectAsync call will fail with this message.</summary>
    public string? FailNextConnectWith { get; set; }

    /// <summary>When set, the next ArmAsync call will fail with this message.</summary>
    public string? FailNextArmWith { get; set; }

    private DetectorState _currentState = DetectorState.Disconnected;
    private readonly object _stateLock = new();

    /// <inheritdoc/>
    public DetectorState CurrentState
    {
        get { lock (_stateLock) return _currentState; }
    }

    /// <inheritdoc/>
    public event EventHandler<DetectorStateChangedEventArgs>? StateChanged;

    /// <inheritdoc/>
    public event EventHandler<ImageAcquiredEventArgs>? ImageAcquired;

    /// <inheritdoc/>
    public async Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (FailNextConnectWith is not null)
        {
            var msg = FailNextConnectWith;
            FailNextConnectWith = null;
            return Result.Failure(ErrorCode.DetectorNotReady, msg);
        }

        TransitionState(DetectorState.Idle);
        return await Task.FromResult(Result.Success()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        TransitionState(DetectorState.Disconnected);
        return await Task.FromResult(Result.Success()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result> ArmAsync(
        DetectorTriggerMode triggerMode = DetectorTriggerMode.Sync,
        CancellationToken cancellationToken = default)
    {
        if (FailNextArmWith is not null)
        {
            var msg = FailNextArmWith;
            FailNextArmWith = null;
            TransitionState(DetectorState.Error, msg);
            return Result.Failure(ErrorCode.DetectorNotReady, msg);
        }

        lock (_stateLock)
        {
            if (_currentState != DetectorState.Idle)
                return Result.Failure(ErrorCode.DetectorNotReady,
                    $"Detector must be Idle to arm; current state: '{_currentState}'.");
        }

        TransitionState(DetectorState.Armed);

        // Simulate arm confirmation delay
        await Task.Delay(ArmDelayMs, cancellationToken).ConfigureAwait(false);

        // In FreeRun mode the detector acquires immediately without waiting for X-ray signal.
        // In Sync mode the "X-ray fires" event is simulated by automatically proceeding after arm.
        // In real hardware: the physical X-ray trigger would initiate the transition below.
        TransitionState(DetectorState.Acquiring);

        await Task.Delay(ReadoutDelayMs, cancellationToken).ConfigureAwait(false);

        // Readout complete — synthesise test image and fire event.
        var image = CreateSyntheticImage();
        TransitionState(DetectorState.ImageReady);

        // Raise ImageAcquired before returning to Idle so subscribers see ImageReady state.
        ImageAcquired?.Invoke(this, new ImageAcquiredEventArgs(image));

        TransitionState(DetectorState.Idle);

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> AbortAsync(CancellationToken cancellationToken = default)
    {
        TransitionState(DetectorState.Error, "Abort requested");
        return await Task.FromResult(Result.Success()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<DetectorStatus>> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        DetectorState state;
        lock (_stateLock) state = _currentState;

        var status = new DetectorStatus
        {
            State = state,
            IsReadyToArm = state == DetectorState.Idle,
            TemperatureCelsius = 28.0,
            SerialNumber = "SIM-001",
            FirmwareVersion = "1.0.0-sim",
            Timestamp = DateTimeOffset.UtcNow,
        };

        return await Task.FromResult(Result.Success(status)).ConfigureAwait(false);
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private void TransitionState(DetectorState newState, string? reason = null)
    {
        DetectorState previous;
        lock (_stateLock)
        {
            previous = _currentState;
            _currentState = newState;
        }

        if (previous != newState)
            StateChanged?.Invoke(this, new DetectorStateChangedEventArgs(previous, newState, reason));
    }

    /// <summary>
    /// Generates a synthetic 16-bit noise image for simulation and testing.
    /// Real detectors return actual pixel data from the panel ASIC.
    /// </summary>
    private RawDetectorImage CreateSyntheticImage()
    {
        int w = SimulatedImageWidth;
        int h = SimulatedImageHeight;
        int pixelCount = w * h;
        var pixels = new byte[pixelCount * 2]; // 16-bit LE per pixel

        var rng = new Random();
        for (int i = 0; i < pixelCount; i++)
        {
            // Simulate ~50% grey with noise (12-bit range: ~2048 ± 400)
            ushort value = (ushort)(2048 + rng.Next(-400, 400));
            pixels[i * 2] = (byte)(value & 0xFF);
            pixels[i * 2 + 1] = (byte)(value >> 8);
        }

        return new RawDetectorImage(
            Width: w,
            Height: h,
            BitsPerPixel: 12,
            PixelData: pixels,
            SerialNumber: "SIM-001",
            TemperatureCelsius: 28.0,
            Timestamp: DateTimeOffset.UtcNow);
    }
}
