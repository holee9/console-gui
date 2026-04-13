// ─────────────────────────────────────────────────────────────────────────────
// HmeDetectorAdapter.cs — HME (Human Imaging) detector SDK 어댑터
//
// Native SDK: libxd2.dll (P/Invoke via HmeNativeMethods)
// Supported models: S4335(WA/WF), S4343(WA)
//
// 현재 상태: SDK 통합 대기 (VendorAdapterTemplate 패턴 기반 스텁)
// 실제 SDK 연동 시 TODO 항목을 libxd2.dll P/Invoke 호출로 교체하세요.
// ─────────────────────────────────────────────────────────────────────────────

#pragma warning disable CS0067  // Event never used — template pattern; ImageAcquired is raised in HandleImageReady once implemented

using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Detector.ThirdParty.Hme;

/// <summary>
/// Adapter for HME (Human Imaging) wireless FPD detectors.
/// Uses libxd2.dll native SDK via P/Invoke (<see cref="HmeNativeMethods"/>).
/// </summary>
/// <remarks>
/// Supported detector models: S4335-WA, S4335-WF, S4343-WA.
///
/// Current status: SDK integration pending. Methods use template patterns
/// with state transitions. Replace TODO items with HmeNativeMethods calls
/// when ready for production.
///
/// Key HME SDK patterns:
///   - Connection: SD_CheckConnection() + TCP/IP
///   - Acquisition: SDAcq_SetStatusHandler(callback) + SDAcq_ResetReady()
///   - Sleep/Wake: SD_Sleep() / SD_WakeUp()
///   - Diagnostics: GetDiagData() / GetAEDConfig()
/// </remarks>
public sealed class HmeDetectorAdapter : IDetectorInterface, IDisposable
{
    private readonly HmeDetectorConfig _config;
    private DetectorState _currentState = DetectorState.Disconnected;
    private readonly object _stateLock = new();
    private bool _disposed;

    // TODO: Store native callback delegate to prevent GC
    // private HmeNativeMethods.StatusCallbackDelegate? _statusCallback;

    /// <summary>Initialises a new adapter with the supplied configuration.</summary>
    public HmeDetectorAdapter(HmeDetectorConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <inheritdoc/>
    public DetectorState CurrentState
    {
        get { lock (_stateLock) return _currentState; }
    }

    /// <inheritdoc/>
    public event EventHandler<DetectorStateChangedEventArgs>? StateChanged;

    /// <inheritdoc/>
    public event EventHandler<ImageAcquiredEventArgs>? ImageAcquired;

    // ── IDetectorInterface ────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Replace with HME SDK connection:
        //   int rc = HmeNativeMethods.SD_CheckConnection();
        //   if (!HmeNativeMethods.IsSuccess(rc))
        //       return Result.Failure(ErrorCode.DetectorNotReady, HmeNativeMethods.DescribeError(rc));
        //
        //   Load param file if configured:
        //   if (_config.ParamFilePath is not null)
        //       LoadParamFile(_config.ParamFilePath);

        TransitionState(DetectorState.Idle);
        return await Task.FromResult(Result.Success()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Replace with HME SDK disconnect:
        //   int rc = HmeNativeMethods.SD_Sleep();
        //   if (!HmeNativeMethods.IsSuccess(rc))
        //       return Result.Failure(ErrorCode.DetectorNotReady, HmeNativeMethods.DescribeError(rc));

        TransitionState(DetectorState.Disconnected);
        return await Task.FromResult(Result.Success()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result> ArmAsync(
        DetectorTriggerMode triggerMode = DetectorTriggerMode.Sync,
        CancellationToken cancellationToken = default)
    {
        // TODO: Replace with HME SDK arm sequence:
        //   int rc = HmeNativeMethods.SDAcq_ResetReady();
        //   if (!HmeNativeMethods.IsSuccess(rc))
        //       return Result.Failure(ErrorCode.DetectorNotReady, "Failed to reset ready state.");
        //
        //   Register status callback for image-ready notification:
        //   _statusCallback = (status, param) => OnStatusChanged(status, param);
        //   HmeNativeMethods.SDAcq_SetStatusHandler(_statusCallback);
        //
        //   Wake up detector if sleeping:
        //   HmeNativeMethods.WakeUpDetector();

        TransitionState(DetectorState.Armed);
        return await Task.FromResult(Result.Success()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result> AbortAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Replace with HME SDK abort:
        //   int rc = HmeNativeMethods.SDAcq_Abort();
        //   if (!HmeNativeMethods.IsSuccess(rc))
        //       return Result.Failure(ErrorCode.DetectorNotReady, HmeNativeMethods.DescribeError(rc));

        TransitionState(DetectorState.Idle, "Abort requested");
        return await Task.FromResult(Result.Success()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<DetectorStatus>> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Replace with HME SDK status query:
        //   int rc = HmeNativeMethods.SD_GetStatus();
        //   var diagData = default(HmeDiagData);
        //   HmeNativeMethods.GetDiagData(out diagData);

        DetectorState state;
        lock (_stateLock) state = _currentState;

        var status = new DetectorStatus
        {
            State = state,
            IsReadyToArm = state == DetectorState.Idle,
            TemperatureCelsius = 0.0,
            SerialNumber = _config.Model,
            FirmwareVersion = "HME-libxd2",
            Timestamp = DateTimeOffset.UtcNow,
        };

        return await Task.FromResult(Result.Success(status)).ConfigureAwait(false);
    }

    // ── IDisposable ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // TODO: Release HME SDK resources:
        //   HmeNativeMethods.SD_Sleep();
        //   _statusCallback = null;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

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

    // TODO: Implement when SDK callback is integrated
    // private void OnStatusChanged(int status, int param)
    // {
    //     // Map HME status codes to state transitions and image acquisition events
    //     // When image is ready, convert pixel data and fire ImageAcquired event
    // }
}
