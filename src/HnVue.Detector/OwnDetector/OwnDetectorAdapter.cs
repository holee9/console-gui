// ─────────────────────────────────────────────────────────────────────────────
// OwnDetectorAdapter.cs — 자사 FPD detector SDK 어댑터
//
// SDK 준비 완료 후 작업 절차:
//   1. sdk/own-detector/ 폴더에 SDK DLL을 배치합니다.
//   2. OwnDetectorNativeMethods.cs 또는 managed wrapper 호출로
//      TODO 마커를 실제 SDK 호출로 교체합니다.
//   3. #if OWN_DETECTOR_NATIVE_SDK 조건부 컴파일 플래그를 제거합니다.
//   4. HnVue.App/App.xaml.cs 등록을 GeneratorSimulator → OwnDetectorAdapter로 변경합니다.
// ─────────────────────────────────────────────────────────────────────────────

#pragma warning disable CS1998  // Async method lacks 'await' — intentional SDK skeleton; methods will await SDK calls when implemented
#pragma warning disable CS0414  // Field assigned but never used — SDK handle placeholder; used after SDK is integrated

using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Detector.OwnDetector;

/// <summary>
/// Production adapter for the 자사 CsI flat-panel detector.
/// Wraps the proprietary SDK and maps it to <see cref="IDetectorInterface"/>.
/// </summary>
/// <remarks>
/// SDK DLL 위치: sdk/own-detector/net8.0-windows/OwnDetectorSdk.dll
/// 또는 native: sdk/own-detector/x64/OwnDetectorNative.dll (P/Invoke via OwnDetectorNativeMethods)
///
/// SDK가 도착하면 아래 TODO 항목들을 실제 SDK 호출로 교체하세요.
/// </remarks>
public sealed class OwnDetectorAdapter : IDetectorInterface, IDisposable
{
    private readonly OwnDetectorConfig _config;
    private DetectorState _currentState = DetectorState.Disconnected;
    private readonly object _stateLock = new();
    private bool _disposed;

    // SDK 연결 핸들 (managed SDK handle 또는 native int handle)
    // TODO: SDK 타입에 맞게 변경하세요.
    //   managed: private OwnDetectorSdk.Session? _session;
    //   native:  private int _handle = -1;
    private int _nativeHandle = -1;

    /// <summary>Initialises a new adapter with the supplied configuration.</summary>
    public OwnDetectorAdapter(OwnDetectorConfig config)
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
        ObjectDisposedException.ThrowIf(_disposed, this);

        // TODO: SDK 연결 호출로 교체하세요.
        //
        // [managed SDK 예시]
        //   _session = new OwnDetectorSdk.Session(_config.Host, _config.Port);
        //   var result = await _session.ConnectAsync(cancellationToken);
        //   if (!result.Success) return Result.Failure(ErrorCode.DetectorNotReady, result.ErrorMessage);
        //
        // [native SDK P/Invoke 예시]
        //   int rc = OwnDetectorNativeMethods.DET_Open(_config.Host, _config.Port, out _nativeHandle);
        //   if (!OwnDetectorNativeMethods.IsSuccess(rc))
        //       return Result.Failure(ErrorCode.DetectorNotReady, OwnDetectorNativeMethods.DescribeError(rc));
        //
        // [캘리브레이션 로드 예시]
        //   if (_config.CalibrationPath is not null)
        //       await _session.LoadCalibrationAsync(_config.CalibrationPath, cancellationToken);

        throw new NotImplementedException(
            "OwnDetectorAdapter.ConnectAsync: SDK 연결 코드를 구현해야 합니다. " +
            "OwnDetectorNativeMethods.cs의 P/Invoke 선언을 참고하세요.");

        // SDK 구현 완료 후 아래 주석을 해제하세요:
        // TransitionState(DetectorState.Idle);
        // return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        // TODO: SDK 연결 해제로 교체하세요.
        //
        // [managed SDK 예시]
        //   await _session!.DisconnectAsync(cancellationToken);
        //   _session = null;
        //
        // [native SDK P/Invoke 예시]
        //   if (_nativeHandle >= 0)
        //   {
        //       OwnDetectorNativeMethods.DET_Close(_nativeHandle);
        //       _nativeHandle = -1;
        //   }

        throw new NotImplementedException(
            "OwnDetectorAdapter.DisconnectAsync: SDK 해제 코드를 구현해야 합니다.");

        // TransitionState(DetectorState.Disconnected);
        // return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> ArmAsync(
        DetectorTriggerMode triggerMode = DetectorTriggerMode.Sync,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // TODO: SDK ARM 명령으로 교체하세요.
        //
        // [managed SDK 예시]
        //   int sdkTrigger = triggerMode == DetectorTriggerMode.Sync ? 0 : 1;
        //   var result = await _session!.ArmAsync(sdkTrigger, cancellationToken);
        //   if (!result.Success) return Result.Failure(ErrorCode.DetectorNotReady, result.ErrorMessage);
        //
        // [native SDK P/Invoke 예시]
        //   int sdkTrigger = triggerMode == DetectorTriggerMode.Sync ? 0 : 1;
        //   int rc = OwnDetectorNativeMethods.DET_Arm(_nativeHandle, sdkTrigger);
        //   if (!OwnDetectorNativeMethods.IsSuccess(rc))
        //       return Result.Failure(ErrorCode.DetectorNotReady, OwnDetectorNativeMethods.DescribeError(rc));
        //
        // ARM 이후 이미지 도착을 기다리는 방법 (둘 중 하나를 선택):
        //   방법 A (이벤트/콜백): SDK가 이미지 준비 완료를 콜백으로 알리면
        //       콜백 핸들러에서 OnImageAcquired()를 호출합니다.
        //   방법 B (폴링): SDK가 폴링 방식이면 Task.Run으로 폴링 루프를 돌립니다.

        throw new NotImplementedException(
            "OwnDetectorAdapter.ArmAsync: SDK ARM 코드를 구현해야 합니다.");

        // TransitionState(DetectorState.Armed);
        // return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> AbortAsync(CancellationToken cancellationToken = default)
    {
        // TODO: SDK ABORT 명령으로 교체하세요.
        //
        // [managed SDK 예시]
        //   await _session!.AbortAsync(cancellationToken);
        //
        // [native SDK P/Invoke 예시]
        //   if (_nativeHandle >= 0)
        //       OwnDetectorNativeMethods.DET_Abort(_nativeHandle);

        throw new NotImplementedException(
            "OwnDetectorAdapter.AbortAsync: SDK ABORT 코드를 구현해야 합니다.");

        // TransitionState(DetectorState.Idle, "Abort requested");
        // return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result<DetectorStatus>> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        // TODO: SDK 상태 조회로 교체하세요.
        //
        // [managed SDK 예시]
        //   var sdkStatus = await _session!.GetStatusAsync(cancellationToken);
        //   return Result.Success(new DetectorStatus {
        //       State = CurrentState,
        //       IsReadyToArm = sdkStatus.IsReady,
        //       TemperatureCelsius = sdkStatus.Temperature,
        //       SerialNumber = sdkStatus.Serial,
        //       FirmwareVersion = sdkStatus.FwVersion });
        //
        // [native SDK P/Invoke 예시]
        //   int rc = OwnDetectorNativeMethods.DET_GetStatus(_nativeHandle, out var ns);
        //   if (!OwnDetectorNativeMethods.IsSuccess(rc))
        //       return Result.Failure<DetectorStatus>(ErrorCode.DetectorNotReady, OwnDetectorNativeMethods.DescribeError(rc));
        //   return Result.Success(new DetectorStatus {
        //       State = CurrentState,
        //       IsReadyToArm = ns.State == 0,
        //       TemperatureCelsius = ns.TemperatureCelsius,
        //       SerialNumber = ns.SerialNumber,
        //       FirmwareVersion = ns.FirmwareVersion });

        throw new NotImplementedException(
            "OwnDetectorAdapter.GetStatusAsync: SDK 상태 조회 코드를 구현해야 합니다.");
    }

    // ── IDisposable ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // TODO: SDK 리소스 해제를 여기에 추가하세요.
        //   managed: _session?.Dispose();
        //   native:  if (_nativeHandle >= 0) OwnDetectorNativeMethods.DET_Close(_nativeHandle);
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

    /// <summary>
    /// Called from the SDK callback or polling loop when a new image is available.
    /// Raises <see cref="ImageAcquired"/> and transitions state to Idle.
    /// </summary>
    private void OnImageAcquired(RawDetectorImage image)
    {
        TransitionState(DetectorState.ImageReady);
        ImageAcquired?.Invoke(this, new ImageAcquiredEventArgs(image));
        TransitionState(DetectorState.Idle);
    }
}
