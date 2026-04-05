using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace HnVue.Update;

/// <summary>
/// Provides Windows Authenticode digital-signature verification and SHA-256 hash checking
/// for update package files.
/// </summary>
/// <remarks>
/// This class performs P/Invoke calls into <c>wintrust.dll</c> and is therefore
/// Windows-only. The project targets <c>net8.0-windows</c>, so this is safe.
/// IEC 62304 §6.2.5: all update packages must be verified before installation.
/// </remarks>
internal static class SignatureVerifier
{
    // WinTrust GUID: WINTRUST_ACTION_GENERIC_VERIFY_V2
    private static readonly Guid WintrustActionGenericVerifyV2 =
        new(0x00AAC56B, 0xCD44, 0x11D0, 0x8C, 0xC2, 0x00, 0xC0, 0x4F, 0xC2, 0x95, 0xEE);

    // WinVerifyTrust return codes
    private const uint ErrorSuccess = 0x00000000;

    /// <summary>
    /// Verifies the Authenticode digital signature of the specified file using <c>WinVerifyTrust</c>.
    /// </summary>
    /// <param name="filePath">Absolute path to the file to verify.</param>
    /// <returns>
    /// <see langword="true"/> when the signature is present and trusted;
    /// <see langword="false"/> when the file does not exist, is unsigned, or the signature is invalid.
    /// </returns>
    internal static bool VerifyAuthenticode(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        var fileInfo = new WinTrustFileInfo
        {
            cbStruct = (uint)Marshal.SizeOf<WinTrustFileInfo>(),
            pcwszFilePath = filePath,
            hFile = IntPtr.Zero,
            pgKnownSubject = IntPtr.Zero
        };

        var fileInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<WinTrustFileInfo>());
        try
        {
            Marshal.StructureToPtr(fileInfo, fileInfoPtr, false);

            var trustData = new WinTrustData
            {
                cbStruct = (uint)Marshal.SizeOf<WinTrustData>(),
                pPolicyCallbackData = IntPtr.Zero,
                pSIPClientData = IntPtr.Zero,
                dwUIChoice = WinTrustDataUiChoice.None,
                fdwRevocationChecks = WinTrustDataRevocationChecks.None,
                dwUnionChoice = WinTrustDataChoice.File,
                pFile = fileInfoPtr,
                dwStateAction = WinTrustDataStateAction.Ignore,
                hWVTStateData = IntPtr.Zero,
                pwszURLReference = null,
                dwProvFlags = WinTrustDataProvFlags.SaferFlag,
                dwUIContext = 0
            };

            uint result = NativeMethods.WinVerifyTrust(
                IntPtr.Zero,
                WintrustActionGenericVerifyV2,
                trustData);

            return result == ErrorSuccess;
        }
        finally
        {
            Marshal.FreeHGlobal(fileInfoPtr);
        }
    }

    /// <summary>
    /// Computes the SHA-256 hash of the specified file and compares it against the expected value.
    /// </summary>
    /// <param name="filePath">Absolute path to the file to hash.</param>
    /// <param name="expectedSha256Hex">
    /// Expected SHA-256 digest as a lowercase or uppercase hexadecimal string (64 characters).
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the computed hash matches <paramref name="expectedSha256Hex"/>;
    /// <see langword="false"/> when the file does not exist or the hashes differ.
    /// </returns>
    internal static bool VerifyHash(string filePath, string expectedSha256Hex)
    {
        if (!File.Exists(filePath))
            return false;

        if (string.IsNullOrWhiteSpace(expectedSha256Hex))
            return false;

        byte[] fileBytes = File.ReadAllBytes(filePath);
        byte[] hashBytes = SHA256.HashData(fileBytes);
        string actualHex = Convert.ToHexString(hashBytes);

        return string.Equals(actualHex, expectedSha256Hex, StringComparison.OrdinalIgnoreCase);
    }

    // ── P/Invoke declarations ─────────────────────────────────────────────────

    private static class NativeMethods
    {
        [DllImport("wintrust.dll", ExactSpelling = true, SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern uint WinVerifyTrust(
            IntPtr hwnd,
            [MarshalAs(UnmanagedType.LPStruct)] Guid pgActionID,
            [In] WinTrustData pWVTData);
    }

    // ── WinTrust interop structures ───────────────────────────────────────────

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WinTrustFileInfo
    {
        public uint cbStruct;
        [MarshalAs(UnmanagedType.LPWStr)] public string pcwszFilePath;
        public IntPtr hFile;
        public IntPtr pgKnownSubject;
    }

    private enum WinTrustDataUiChoice : uint
    {
        All = 1,
        None = 2,
        NoBad = 3,
        NoGood = 4
    }

    private enum WinTrustDataRevocationChecks : uint
    {
        None = 0x00000000,
        WholeChain = 0x00000001
    }

    private enum WinTrustDataChoice : uint
    {
        File = 1,
        Catalog = 2,
        Blob = 3,
        Signer = 4,
        Certificate = 5
    }

    private enum WinTrustDataStateAction : uint
    {
        Ignore = 0x00000000,
        Verify = 0x00000001,
        Close = 0x00000002,
        AutoCache = 0x00000003,
        AutoCacheFlush = 0x00000004
    }

    [Flags]
    private enum WinTrustDataProvFlags : uint
    {
        UseIe4TrustFlag = 0x00000001,
        NoIe4ChainFlag = 0x00000002,
        NoPolicyUsageFlag = 0x00000004,
        RevocationCheckNone = 0x00000010,
        RevocationCheckEndCert = 0x00000020,
        RevocationCheckChain = 0x00000040,
        RevocationCheckChainExcludeRoot = 0x00000080,
        SaferFlag = 0x00000100,
        HashOnlyFlag = 0x00000200,
        UseDefaultOsverCheck = 0x00000400,
        LifetimeSigningFlag = 0x00000800,
        CacheOnlyUrlRetrieval = 0x00001000
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WinTrustData
    {
        public uint cbStruct;
        public IntPtr pPolicyCallbackData;
        public IntPtr pSIPClientData;
        public WinTrustDataUiChoice dwUIChoice;
        public WinTrustDataRevocationChecks fdwRevocationChecks;
        public WinTrustDataChoice dwUnionChoice;
        public IntPtr pFile;
        public WinTrustDataStateAction dwStateAction;
        public IntPtr hWVTStateData;
        [MarshalAs(UnmanagedType.LPWStr)] public string? pwszURLReference;
        public WinTrustDataProvFlags dwProvFlags;
        public uint dwUIContext;
    }
}
