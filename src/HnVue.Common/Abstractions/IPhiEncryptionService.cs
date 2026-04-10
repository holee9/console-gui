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
}
