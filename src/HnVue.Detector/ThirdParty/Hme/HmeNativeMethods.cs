// ─────────────────────────────────────────────────────────────────────────────
// HmeNativeMethods.cs — HME (Human Imaging) detector SDK P/Invoke 선언부
//
// Native DLL: sdk/third-party/hme-licence/dll/libxd2.dll
// Supported models: S4335(WA/WF), S4343(WA)
// Param files: sdk/third-party/hme-licence/HME/2G_SDK/XAS_W_2G_SampleCode/Debug/param/
//
// PE exports from libxd2.dll (analysis):
//   SD_CheckConnection, SD_GetStatus, SD_SetSleepMethod,
//   SD_Sleep, SD_WakeUp, SDAcq_Abort, SDAcq_ResetReady,
//   SDAcq_SetStatusHandler, WakeUpDetector, SleepDetector,
//   GetDiagData, GetAEDConfig, ResetByFPGAReset, ResetByReboot
// ─────────────────────────────────────────────────────────────────────────────

using System.Runtime.InteropServices;

namespace HnVue.Detector.ThirdParty.Hme;

/// <summary>
/// P/Invoke declarations for the HME native detector SDK (libxd2.dll).
/// </summary>
internal static class HmeNativeMethods
{
    private const string DllName = "libxd2";

    // ── Connection ────────────────────────────────────────────────────────────

    /// <summary>Checks if the detector is connected.</summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SD_CheckConnection();

    /// <summary>Gets the current detector status register.</summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SD_GetStatus();

    // ── Sleep / Power ─────────────────────────────────────────────────────────

    /// <summary>Sets the sleep method configuration.</summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SD_SetSleepMethod(int method);

    /// <summary>Puts the detector into sleep mode.</summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SD_Sleep();

    /// <summary>Wakes the detector from sleep mode.</summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SD_WakeUp();

    /// <summary>Wakes up the detector (high-level wrapper).</summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int WakeUpDetector();

    /// <summary>Puts the detector to sleep (high-level wrapper).</summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SleepDetector();

    // ── Acquisition Control ───────────────────────────────────────────────────

    /// <summary>Aborts the current acquisition.</summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SDAcq_Abort();

    /// <summary>Resets the ready state for next acquisition.</summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SDAcq_ResetReady();

    /// <summary>Registers a status change callback handler.</summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SDAcq_SetStatusHandler(StatusCallbackDelegate handler);

    // ── Diagnostics ───────────────────────────────────────────────────────────

    /// <summary>Retrieves diagnostic data from the detector.</summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetDiagData(out HmeDiagData diagData);

    /// <summary>Gets the AED (Automatic Exposure Detection) configuration.</summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetAEDConfig(out HmeAedConfig aedConfig);

    // ── Reset ─────────────────────────────────────────────────────────────────

    /// <summary>Resets the detector via FPGA reset.</summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int ResetByFPGAReset();

    /// <summary>Resets the detector via full reboot.</summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int ResetByReboot();

    // ── Error helpers ─────────────────────────────────────────────────────────

    /// <summary>Returns true when the native return code indicates success.</summary>
    internal static bool IsSuccess(int returnCode) => returnCode == 0;

    /// <summary>Converts a native return code to a human-readable message.</summary>
    internal static string DescribeError(int returnCode) =>
        $"HME SDK error code: 0x{returnCode:X8}";

    // ── Delegates ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Callback delegate for detector status changes.
    /// Must be stored as a field to prevent garbage collection of the delegate.
    /// </summary>
    internal delegate void StatusCallbackDelegate(int status, int param);
}

// ── Native structs ────────────────────────────────────────────────────────────

/// <summary>
/// Diagnostic data structure returned by GetDiagData.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct HmeDiagData
{
    public int ConnectionStatus;
    public float TemperatureCelsius;
    public int BatteryLevel;
    public int ErrorCode;
}

/// <summary>
/// AED (Automatic Exposure Detection) configuration structure.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct HmeAedConfig
{
    public int Enabled;
    public float Sensitivity;
    public int Mode;
}
