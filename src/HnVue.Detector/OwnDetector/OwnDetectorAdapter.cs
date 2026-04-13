// ─────────────────────────────────────────────────────────────────────────────
// OwnDetectorAdapter.cs — 자사 FPD detector SDK 어댑터 (AbyzSdk 연동)
//
// AbyzSdk BlueSdkFacade를 사용하여 자사 CsI+FPD 디텍터를 제어합니다.
// BlueSdkFacade는 TCP/IP 기반 연결, 이미지 획득, 연결 해제를 제공합니다.
//
// SDK DLL 위치: sdk/own-detector/bluesdk/AbyzSdk.dll + AbyzSdk.Imaging.dll
// ─────────────────────────────────────────────────────────────────────────────

using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;

#if ABYZSDK_AVAILABLE
using AbyzSdk;
using AbyzSdk.Application.DTOs;
#endif

namespace HnVue.Detector.OwnDetector;

/// <summary>
/// Production adapter for the 자사 CsI flat-panel detector.
/// Uses AbyzSdk BlueSdkFacade for TCP/IP communication with the detector.
/// </summary>
/// <remarks>
/// When AbyzSdk.dll is not available (e.g., test environments without SDK),
/// falls back to NotImplementedException for all operations.
///
/// Compile with ABYZSDK_AVAILABLE defined (automatic when SDK DLL exists)
/// to enable real SDK integration.
/// </remarks>
public sealed class OwnDetectorAdapter : IDetectorInterface, IDisposable
{
    private readonly OwnDetectorConfig _config;
    private DetectorState _currentState = DetectorState.Disconnected;
    private readonly object _stateLock = new();
    private bool _disposed;

#if ABYZSDK_AVAILABLE
    private BlueSdkFacade? _facade;
#else
    // SDK handle placeholder — used when SDK is not compiled in
    private int _nativeHandle = -1;
#endif

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

#if ABYZSDK_AVAILABLE
        try
        {
            _facade = new BlueSdkFacade();
            var connected = await _facade.ConnectAsync(_config.Host, DetectorType.Blue)
                .ConfigureAwait(false);

            if (!connected)
            {
                _facade.Dispose();
                _facade = null;
                return Result.Failure(ErrorCode.DetectorNotReady,
                    $"Failed to connect to detector at {_config.Host}.");
            }

            TransitionState(DetectorState.Idle);
            return Result.Success();
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure(ErrorCode.DetectorNotReady,
                $"Connection error: {ex.Message}");
        }
#else
        await Task.CompletedTask;
        throw new NotImplementedException(
            "OwnDetectorAdapter.ConnectAsync: AbyzSdk not available. " +
            "Ensure AbyzSdk.dll is in sdk/own-detector/bluesdk/ and ABYZSDK_AVAILABLE is defined.");
#endif
    }

    /// <inheritdoc/>
    public async Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
    {
#if ABYZSDK_AVAILABLE
        try
        {
            if (_facade is not null)
            {
                await _facade.DisconnectAsync(_config.Host).ConfigureAwait(false);
                _facade.Dispose();
                _facade = null;
            }

            TransitionState(DetectorState.Disconnected);
            return Result.Success();
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            TransitionState(DetectorState.Disconnected);
            return Result.Failure(ErrorCode.DetectorNotReady,
                $"Disconnect error: {ex.Message}");
        }
#else
        await Task.CompletedTask;
        throw new NotImplementedException(
            "OwnDetectorAdapter.DisconnectAsync: AbyzSdk not available.");
#endif
    }

    /// <inheritdoc/>
    public async Task<Result> ArmAsync(
        DetectorTriggerMode triggerMode = DetectorTriggerMode.Sync,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

#if ABYZSDK_AVAILABLE
        try
        {
            lock (_stateLock)
            {
                if (_currentState != DetectorState.Idle)
                    return Result.Failure(ErrorCode.DetectorNotReady,
                        $"Detector must be Idle to arm; current state: '{_currentState}'.");
            }

            if (_facade is null)
                return Result.Failure(ErrorCode.DetectorNotReady, "Detector not connected.");

            TransitionState(DetectorState.Armed);
            TransitionState(DetectorState.Acquiring);

            var imageDto = await _facade.AcquireImageAsync(_config.Host)
                .ConfigureAwait(false);

            if (imageDto is null)
            {
                TransitionState(DetectorState.Error, "Acquisition returned no image data");
                return Result.Failure(ErrorCode.DetectorNotReady,
                    "Acquisition failed: no image data returned from detector.");
            }

            var image = ConvertImage(imageDto);
            TransitionState(DetectorState.ImageReady);
            ImageAcquired?.Invoke(this, new ImageAcquiredEventArgs(image));
            TransitionState(DetectorState.Idle);

            return Result.Success();
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            TransitionState(DetectorState.Error, ex.Message);
            return Result.Failure(ErrorCode.DetectorNotReady,
                $"Arm/acquisition error: {ex.Message}");
        }
#else
        await Task.CompletedTask;
        throw new NotImplementedException(
            "OwnDetectorAdapter.ArmAsync: AbyzSdk not available.");
#endif
    }

    /// <inheritdoc/>
    public async Task<Result> AbortAsync(CancellationToken cancellationToken = default)
    {
#if ABYZSDK_AVAILABLE
        try
        {
            _facade?.Dispose();
            _facade = null;
            TransitionState(DetectorState.Error, "Abort requested");
            return Result.Success();
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure(ErrorCode.DetectorNotReady,
                $"Abort error: {ex.Message}");
        }
#else
        await Task.CompletedTask;
        throw new NotImplementedException(
            "OwnDetectorAdapter.AbortAsync: AbyzSdk not available.");
#endif
    }

    /// <inheritdoc/>
    public async Task<Result<DetectorStatus>> GetStatusAsync(CancellationToken cancellationToken = default)
    {
#if ABYZSDK_AVAILABLE
        DetectorState state;
        lock (_stateLock) state = _currentState;

        var status = new DetectorStatus
        {
            State = state,
            IsReadyToArm = state == DetectorState.Idle,
            TemperatureCelsius = 0.0, // Not available through BlueSdkFacade
            SerialNumber = _config.Host, // Use IP as identifier
            FirmwareVersion = "AbyzSdk-0.1.0",
            Timestamp = DateTimeOffset.UtcNow,
        };

        return await Task.FromResult(Result.Success(status)).ConfigureAwait(false);
#else
        await Task.CompletedTask;
        throw new NotImplementedException(
            "OwnDetectorAdapter.GetStatusAsync: AbyzSdk not available.");
#endif
    }

    // ── IDisposable ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

#if ABYZSDK_AVAILABLE
        _facade?.Dispose();
        _facade = null;
#endif
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

#if ABYZSDK_AVAILABLE
    /// <summary>
    /// Converts AbyzSdk ImageDto to HnVue RawDetectorImage.
    /// </summary>
    private static RawDetectorImage ConvertImage(object imageDto)
    {
        // Use reflection to access ImageDto properties since we may not have compile-time type info
        var type = imageDto.GetType();

        int width = 0, height = 0, bitsPerPixel = _config.BitsPerPixel;
        byte[]? pixelData = null;

        var widthProp = type.GetProperty("Width");
        var heightProp = type.GetProperty("Height");
        var pixelDataProp = type.GetProperty("PixelData");

        if (widthProp is not null) width = (int)(widthProp.GetValue(imageDto) ?? 0);
        if (heightProp is not null) height = (int)(heightProp.GetValue(imageDto) ?? 0);
        if (pixelDataProp is not null) pixelData = pixelDataProp.GetValue(imageDto) as byte[];

        pixelData ??= new byte[width * height * 2]; // Fallback: 16-bit per pixel

        return new RawDetectorImage(
            Width: width,
            Height: height,
            BitsPerPixel: bitsPerPixel,
            PixelData: pixelData,
            SerialNumber: "ABYZ-001",
            TemperatureCelsius: 0.0,
            Timestamp: DateTimeOffset.UtcNow);
    }
#endif
}
