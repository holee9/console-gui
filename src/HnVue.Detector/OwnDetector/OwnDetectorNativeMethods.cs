// ─────────────────────────────────────────────────────────────────────────────
// OwnDetectorNativeMethods.cs — 자사 detector SDK P/Invoke 선언부
//
// 자사 SDK 파일 위치:
//   sdk/own-detector/net8.0-windows/OwnDetectorSdk.dll   (managed .NET wrapper, 우선)
//   sdk/own-detector/x64/OwnDetectorNative.dll           (native C DLL, P/Invoke 대상)
//
// 자사 SDK가 managed (.NET) wrapper DLL인 경우:
//   이 파일은 삭제하고 OwnDetectorAdapter.cs에서 직접 managed API를 호출합니다.
//
// 자사 SDK가 native C/C++ DLL인 경우:
//   아래 DllImport 선언에 실제 함수 시그니처를 입력하세요.
//   SDK 헤더 파일(*.h)을 참고하여 파라미터 타입을 맞춰주세요.
// ─────────────────────────────────────────────────────────────────────────────

#if OWN_DETECTOR_NATIVE_SDK   // 실제 SDK 도착 후 이 #if 조건을 제거하거나 수정하세요.

using System.Runtime.InteropServices;

namespace HnVue.Detector.OwnDetector;

/// <summary>
/// P/Invoke declarations for the 자사 native detector SDK (OwnDetectorNative.dll).
/// Replace stub signatures with the actual function signatures from the SDK header files.
/// </summary>
internal static class OwnDetectorNativeMethods
{
    private const string DllName = "OwnDetectorNative";

    // ── Connection ────────────────────────────────────────────────────────────

    /// <summary>
    /// Opens a communication session with the detector.
    /// TODO: Replace with actual SDK function signature from OwnDetectorNative.h
    /// Example: int DET_Open(const char* host, int port, int* handle)
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int DET_Open(
        [MarshalAs(UnmanagedType.LPStr)] string host,
        int port,
        out int handle);

    /// <summary>
    /// Closes the communication session.
    /// TODO: Replace with actual SDK function signature.
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int DET_Close(int handle);

    // ── Acquisition control ───────────────────────────────────────────────────

    /// <summary>
    /// Arms the detector for the next exposure.
    /// TODO: Replace with actual SDK function signature.
    /// Example: int DET_Arm(int handle, int triggerMode)
    ///   triggerMode: 0 = Sync (hardware), 1 = FreeRun (software)
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int DET_Arm(int handle, int triggerMode);

    /// <summary>
    /// Aborts the current acquisition immediately.
    /// TODO: Replace with actual SDK function signature.
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int DET_Abort(int handle);

    // ── Image retrieval ───────────────────────────────────────────────────────

    /// <summary>
    /// Reads the acquired image into the provided buffer.
    /// TODO: Replace with actual SDK function signature.
    /// Example: int DET_GetImage(int handle, ushort* buffer, int bufferSize, int* width, int* height)
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern unsafe int DET_GetImage(
        int handle,
        ushort* buffer,
        int bufferSizePixels,
        out int width,
        out int height);

    // ── Status ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Queries the detector status register.
    /// TODO: Replace with actual SDK function signature.
    /// Example: int DET_GetStatus(int handle, DetStatusStruct* status)
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int DET_GetStatus(int handle, out NativeDetectorStatus status);

    // ── Native status struct ──────────────────────────────────────────────────

    /// <summary>
    /// Maps to the native status structure returned by DET_GetStatus.
    /// TODO: Replace fields with actual struct layout from SDK header.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeDetectorStatus
    {
        public int State;               // 0=Idle, 1=Armed, 2=Acquiring, ...
        public float TemperatureCelsius;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string SerialNumber;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string FirmwareVersion;
    }

    // ── Error code helpers ────────────────────────────────────────────────────

    /// <summary>Returns true when the native return code indicates success.</summary>
    internal static bool IsSuccess(int returnCode) => returnCode == 0;

    /// <summary>Converts a native return code to a human-readable message.</summary>
    internal static string DescribeError(int returnCode) =>
        // TODO: Map SDK-specific error codes to descriptive messages.
        $"SDK error code: 0x{returnCode:X8}";
}

#endif
