using HnVue.Common.Abstractions;

namespace HnVue.Data.Security;

/// <summary>
/// Null implementation of <see cref="IPhiEncryptionService"/> that performs no encryption.
/// Used when column-level PHI encryption is not configured (SWR-CS-080 optional).
/// </summary>
internal sealed class NullPhiEncryptionService : IPhiEncryptionService
{
    /// <inheritdoc/>
    public string Encrypt(string plaintext) => plaintext;

    /// <inheritdoc/>
    public string Decrypt(string ciphertext) => ciphertext;
}
