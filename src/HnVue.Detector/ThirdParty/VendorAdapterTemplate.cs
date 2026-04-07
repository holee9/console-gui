// ─────────────────────────────────────────────────────────────────────────────
// VendorAdapterTemplate.cs — 타사 detector SDK 어댑터 구현 패턴 가이드
//
// 새 타사 detector를 연동할 때 이 파일을 복사하여 {VendorName}DetectorAdapter.cs로
// 이름을 바꾸고 TODO 항목을 실제 SDK 호출로 교체하세요.
//
// 파일 배치 예시:
//   src/HnVue.Detector/ThirdParty/VendorA/VendorADetectorAdapter.cs
//   src/HnVue.Detector/ThirdParty/VendorA/VendorADetectorConfig.cs
//   sdk/third-party/vendor-a/net8.0-windows/VendorASdk.dll
//
// HnVue.Detector.csproj에 SDK Reference를 추가하세요 (csproj 파일 내 주석 참고).
// ─────────────────────────────────────────────────────────────────────────────

#pragma warning disable CS0067  // Event never used — template pattern; ImageAcquired is raised in HandleImageReady once implemented

using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Detector.ThirdParty;

/// <summary>
/// Template for third-party (타사) detector SDK adapters.
/// Copy this file, rename the class, and replace TODO items with the vendor SDK calls.
///
/// Known patterns across common DR detector SDKs:
///   - Callback-based: SDK fires a callback/event when image is ready (most common).
///   - Polling-based:  Caller polls SDK IsImageReady() or GetStatus() in a loop.
///   - Async Task:     SDK exposes a Task-based API (rare; modern SDKs only).
///
/// If the vendor SDK is COM-based, wrap it with a RCW and call it from a dedicated STA thread.
/// </summary>
public sealed class VendorAdapterTemplate : IDetectorInterface, IDisposable
{
    // TODO: Replace DetectorConfig with a vendor-specific config record.
    private readonly DetectorConfig _config;
    private DetectorState _currentState = DetectorState.Disconnected;
    private readonly object _stateLock = new();
    private bool _disposed;

    // TODO: Add vendor SDK session/handle field here.
    // Example: private VendorSdk.Session? _session;

    /// <summary>Initialises a new adapter with the supplied configuration.</summary>
    public VendorAdapterTemplate(DetectorConfig config)
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
        // TODO: Open SDK session, load calibration maps.
        // Example:
        //   _session = new VendorSdk.Session();
        //   int rc = _session.Open(_config.Host, _config.Port);
        //   if (rc != 0) return Result.Failure(ErrorCode.DetectorNotReady, $"SDK error: {rc}");
        //   _session.OnImageReady += HandleImageReady;  // subscribe to SDK callback

        TransitionState(DetectorState.Idle);
        return await Task.FromResult(Result.Success()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Unsubscribe callbacks, close session.
        // Example:
        //   _session!.OnImageReady -= HandleImageReady;
        //   _session.Close();
        //   _session = null;

        TransitionState(DetectorState.Disconnected);
        return await Task.FromResult(Result.Success()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result> ArmAsync(
        DetectorTriggerMode triggerMode = DetectorTriggerMode.Sync,
        CancellationToken cancellationToken = default)
    {
        // TODO: Map DetectorTriggerMode to the vendor SDK trigger constant.
        // Example (callback pattern):
        //   int sdkMode = triggerMode == DetectorTriggerMode.Sync ? VendorSdk.TRIG_HW : VendorSdk.TRIG_SW;
        //   int rc = _session!.Arm(sdkMode);
        //   if (rc != 0) return Result.Failure(ErrorCode.DetectorNotReady, $"Arm failed: {rc}");
        //   // Image will arrive via HandleImageReady callback — do not block here.

        TransitionState(DetectorState.Armed);
        return await Task.FromResult(Result.Success()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result> AbortAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Send abort/cancel command to the SDK.
        // Example:
        //   _session?.Abort();

        TransitionState(DetectorState.Idle, "Abort requested");
        return await Task.FromResult(Result.Success()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<DetectorStatus>> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Query vendor SDK for status.
        // Example:
        //   var sdkStatus = _session!.GetStatus();
        //   return Result.Success(new DetectorStatus { ... map fields ... });

        var status = new DetectorStatus
        {
            State = CurrentState,
            IsReadyToArm = CurrentState == DetectorState.Idle,
            TemperatureCelsius = 0.0,
            SerialNumber = null,
            FirmwareVersion = null,
        };

        return await Task.FromResult(Result.Success(status)).ConfigureAwait(false);
    }

    // ── IDisposable ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // TODO: Release SDK resources.
        // Example: _session?.Dispose();
    }

    // ── SDK callback handler ──────────────────────────────────────────────────

    /// <summary>
    /// Invoked by the vendor SDK callback when a new image frame is ready.
    /// Maps vendor-specific image data to <see cref="RawDetectorImage"/> and raises
    /// <see cref="ImageAcquired"/>.
    /// </summary>
    /// <remarks>
    /// TODO: Adjust method signature to match the vendor SDK callback delegate type.
    /// Example: void HandleImageReady(object sender, VendorSdk.ImageEventArgs e)
    /// </remarks>
    private static void HandleImageReady(/* VendorSdk.ImageEventArgs e */)
    {
        // TODO: Extract pixel data from the vendor image object.
        // Example:
        //   var image = new RawDetectorImage(
        //       Width: e.Width,
        //       Height: e.Height,
        //       BitsPerPixel: e.BitsPerPixel,
        //       PixelData: e.GetPixelBytes(),
        //       SerialNumber: _config.Host,
        //       TemperatureCelsius: 0.0,
        //       Timestamp: DateTimeOffset.UtcNow);
        //   TransitionState(DetectorState.ImageReady);
        //   ImageAcquired?.Invoke(this, new ImageAcquiredEventArgs(image));
        //   TransitionState(DetectorState.Idle);
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
}
