using HnVue.Data.Services;

namespace HnVue.Data.Security;

/// <summary>
/// Derives deterministic AES-256-GCM keys for PHI encryption from stable secret material.
/// Uses the same HKDF salt/info tuple as <see cref="AesGcmPhiEncryptionService"/>.
/// </summary>
internal static class PhiKeyDerivation
{
    /// <summary>
    /// Derives a 32-byte key from the supplied secret material using HKDF-SHA256.
    /// </summary>
    internal static byte[] DeriveKey(string keyMaterial) =>
        AesGcmPhiEncryptionService.DeriveKey(keyMaterial);
}
