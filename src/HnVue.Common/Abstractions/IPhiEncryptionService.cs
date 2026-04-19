using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

/// <summary>
/// Provides column-level encryption for PHI (Protected Health Information) fields.
/// Uses AES-256-GCM for authenticated encryption per SWR-CS-080.
/// </summary>
public interface IPhiEncryptionService
{
    /// <summary>Encrypts plaintext to base64-encoded ciphertext.</summary>
    string Encrypt(string plaintext);

    /// <summary>Decrypts base64-encoded ciphertext to plaintext.</summary>
    string Decrypt(string ciphertext);

    /// <summary>
    /// Verifies the GCM authentication tag of a ciphertext without decrypting.
    /// Used for data integrity validation during migration and periodic checks.
    /// </summary>
    /// <param name="ciphertext">Base64-encoded ciphertext to verify.</param>
    /// <returns>Success if the tag is valid; failure with <see cref="ErrorCode.EncryptionFailed"/> if tampered.</returns>
    Result VerifyTag(string ciphertext);

    /// <summary>
    /// Generates a new 32-byte AES-256 key suitable for PHI encryption.
    /// Use during initial setup or key rotation.
    /// </summary>
    /// <returns>Base64-encoded 32-byte key.</returns>
    string GenerateKey();
}
