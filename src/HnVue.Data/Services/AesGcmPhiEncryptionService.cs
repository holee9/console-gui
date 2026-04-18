using System.Security.Cryptography;
using System.Text;
using HnVue.Common.Abstractions;

namespace HnVue.Data.Services;

// @MX:ANCHOR AesGcmPhiEncryptionService - @MX:REASON: Safety-critical PHI encryption service; implements SWR-CS-080 AES-256-GCM with HKDF key derivation
/// <summary>
/// AES-256-GCM authenticated encryption service for PHI column-level encryption.
/// Implements SWR-CS-080 requirements per SPEC-INFRA-002.
///
/// Output format: base64( Nonce[12] || Ciphertext[N] || Tag[16] )
/// </summary>
// @MX:WARN AesGcmPhiEncryptionService - @MX:REASON: Cryptographic key must be 32 bytes (AES-256); key never stored in plaintext
public sealed class AesGcmPhiEncryptionService : IPhiEncryptionService
{
    private const int NonceSize = 12;  // 96-bit nonce for AES-GCM
    private const int TagSize = 16;    // 128-bit authentication tag

    private static readonly byte[] HkdfSalt = Encoding.UTF8.GetBytes("HnVue-PHI-Encryption-v1");
    private static readonly byte[] HkdfInfo = Encoding.UTF8.GetBytes("HnVue-PHI-AES256GCM-v1");

    private readonly byte[] _key;

    /// <summary>
    /// Initializes a new instance with a pre-derived 32-byte AES-256 key.
    /// </summary>
    /// <param name="key">The AES-256 encryption key (must be exactly 32 bytes).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> length is not 32 bytes.</exception>
    public AesGcmPhiEncryptionService(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length != 32)
            throw new ArgumentException("AES-256-GCM key must be exactly 32 bytes.", nameof(key));
        _key = key;
    }

    /// <summary>
    /// Derives a 32-byte AES-256 key from the SQLCipher master key using HKDF-SHA256.
    /// </summary>
    /// <param name="sqlCipherKeyMaterial">The SQLCipher connection password (IKM for HKDF).</param>
    /// <returns>A new <see cref="AesGcmPhiEncryptionService"/> initialized with the derived key.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sqlCipherKeyMaterial"/> is null or whitespace.</exception>
    public static AesGcmPhiEncryptionService FromSqlCipherKey(string sqlCipherKeyMaterial)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlCipherKeyMaterial);

        var ikm = Encoding.UTF8.GetBytes(sqlCipherKeyMaterial);
        var derivedKey = HKDF.DeriveKey(HashAlgorithmName.SHA256, ikm, 32, HkdfSalt, HkdfInfo);
        return new AesGcmPhiEncryptionService(derivedKey);
    }

    /// <summary>
    /// Derives a 32-byte AES-256 key from raw key material using HKDF-SHA256.
    /// </summary>
    /// <param name="keyMaterial">Raw key material bytes (IKM for HKDF).</param>
    /// <returns>Derived 32-byte key.</returns>
    public static byte[] DeriveKey(byte[] keyMaterial)
    {
        ArgumentNullException.ThrowIfNull(keyMaterial);
        if (keyMaterial.Length == 0)
            throw new ArgumentException("Key material cannot be empty.", nameof(keyMaterial));

        return HKDF.DeriveKey(HashAlgorithmName.SHA256, keyMaterial, 32, HkdfSalt, HkdfInfo);
    }

    /// <summary>
    /// Derives a 32-byte AES-256 key from a string using HKDF-SHA256.
    /// Deterministic: same input always produces the same key.
    /// </summary>
    /// <param name="keyMaterial">String key material.</param>
    /// <returns>Derived 32-byte key.</returns>
    public static byte[] DeriveKey(string keyMaterial)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyMaterial);
        return HKDF.DeriveKey(HashAlgorithmName.SHA256, Encoding.UTF8.GetBytes(keyMaterial), 32, HkdfSalt, HkdfInfo);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Null or empty input is returned as-is without encryption (preserves EF Core null semantics).
    /// Output format: base64( Nonce[12] || Ciphertext[N] || Tag[16] )
    /// </remarks>
    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return plaintext;

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        // Format: Nonce(12) || Ciphertext(N) || Tag(16)
        var combined = new byte[NonceSize + ciphertext.Length + TagSize];
        Buffer.BlockCopy(nonce, 0, combined, 0, NonceSize);
        Buffer.BlockCopy(ciphertext, 0, combined, NonceSize, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, combined, NonceSize + ciphertext.Length, TagSize);

        return Convert.ToBase64String(combined);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Null or empty input is returned as-is.
    /// Throws <see cref="CryptographicException"/> on authentication tag mismatch (tampered data).
    /// </remarks>
    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
            return ciphertext;

        var data = Convert.FromBase64String(ciphertext);

        if (data.Length < NonceSize + TagSize)
            throw new FormatException($"Ciphertext too short: expected at least {NonceSize + TagSize} bytes after base64 decode.");

        var nonce = data[..NonceSize];
        var encryptedContent = data[NonceSize..(data.Length - TagSize)];
        var tag = data[(data.Length - TagSize)..];

        var decryptedBytes = new byte[encryptedContent.Length];

        using var aes = new AesGcm(_key, TagSize);

        // Throws CryptographicException if tag is invalid (REQ-PHI-001)
        aes.Decrypt(nonce, encryptedContent, tag, decryptedBytes);

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
