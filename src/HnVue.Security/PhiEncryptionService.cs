using System.Security.Cryptography;
using System.Text;
using HnVue.Common.Abstractions;

namespace HnVue.Security;

/// <summary>
/// AES-256-GCM encryption service for PHI column-level encryption (SWR-CS-080).
/// Key must be 32 bytes (256 bits). Each encryption generates a random 12-byte nonce.
/// Output format: base64(nonce + tag + ciphertext)
/// </summary>
public sealed class PhiEncryptionService : IPhiEncryptionService
{
    private readonly byte[] _key;

    /// <summary>Initializes a new instance with a 32-byte AES-256 key.</summary>
    /// <param name="key">The encryption key (must be exactly 32 bytes).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> length is not 32 bytes.</exception>
    public PhiEncryptionService(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length != 32)
            throw new ArgumentException("AES-256-GCM key must be 32 bytes.", nameof(key));
        _key = key;
    }

    /// <inheritdoc/>
    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return plaintext;

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize]; // 16 bytes

        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        // Concatenate: nonce (12) + tag (16) + ciphertext (N)
        var result = new byte[nonce.Length + tag.Length + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(ciphertext, 0, result, nonce.Length + tag.Length, ciphertext.Length);

        return Convert.ToBase64String(result);
    }

    /// <inheritdoc/>
    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
            return ciphertext;

        var data = Convert.FromBase64String(ciphertext);
        var nonceSize = AesGcm.NonceByteSizes.MaxSize; // 12
        var tagSize = AesGcm.TagByteSizes.MaxSize; // 16

        if (data.Length < nonceSize + tagSize)
            throw new FormatException("Invalid ciphertext format.");

        var nonce = data[..nonceSize];
        var tag = data[nonceSize..(nonceSize + tagSize)];
        var encryptedBytes = data[(nonceSize + tagSize)..];

        var decryptedBytes = new byte[encryptedBytes.Length];
        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aes.Decrypt(nonce, encryptedBytes, tag, decryptedBytes);

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
