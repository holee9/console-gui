using System.Security.Cryptography;
using System.Text;
using HnVue.Data.Services;

namespace HnVue.Data.Tests.Services;

/// <summary>
/// Unit tests for <see cref="AesGcmPhiEncryptionService"/> (SPEC-INFRA-002, REQ-PHI-001, REQ-PHI-002).
/// Verifies AES-256-GCM encryption correctness, nonce randomness, tamper detection,
/// and HKDF key derivation per SWR-CS-080.
/// </summary>
[Trait("SWR", "SWR-CS-080")]
[Trait("SPEC", "SPEC-INFRA-002")]
public sealed class AesGcmPhiEncryptionServiceTests
{
    private static byte[] NewKey() => RandomNumberGenerator.GetBytes(32);

    // ── Round-trip tests (REQ-PHI-001) ─────────────────────────────────────

    [Fact]
    [Trait("Category", "RoundTrip")]
    public void EncryptDecrypt_ShortInput_RoundTripSucceeds()
    {
        // Arrange
        var service = new AesGcmPhiEncryptionService(NewKey());
        const string plaintext = "홍길동"; // Short Korean name (PHI test)

        // Act
        var ciphertext = service.Encrypt(plaintext);
        var decrypted = service.Decrypt(ciphertext);

        // Assert
        decrypted.Should().Be(plaintext);
        ciphertext.Should().NotBe(plaintext, "ciphertext must differ from plaintext");
    }

    [Fact]
    [Trait("Category", "RoundTrip")]
    public void EncryptDecrypt_MediumInput_RoundTripSucceeds()
    {
        // Arrange
        var service = new AesGcmPhiEncryptionService(NewKey());
        var plaintext = string.Concat(Enumerable.Repeat("PatientNameABCD", 10)); // ~150 chars

        // Act
        var ciphertext = service.Encrypt(plaintext);
        var decrypted = service.Decrypt(ciphertext);

        // Assert
        decrypted.Should().Be(plaintext);
    }

    [Fact]
    [Trait("Category", "RoundTrip")]
    public void EncryptDecrypt_LongInput_RoundTripSucceeds()
    {
        // Arrange
        var service = new AesGcmPhiEncryptionService(NewKey());
        var plaintext = string.Concat(Enumerable.Repeat("X", 1024)); // 1 KB of data

        // Act
        var ciphertext = service.Encrypt(plaintext);
        var decrypted = service.Decrypt(ciphertext);

        // Assert
        decrypted.Should().Be(plaintext);
    }

    // ── Tampered tag test (REQ-PHI-001) ────────────────────────────────────

    [Fact]
    [Trait("Category", "Security")]
    public void Decrypt_TamperedTag_ThrowsCryptographicException()
    {
        // Arrange
        var service = new AesGcmPhiEncryptionService(NewKey());
        var ciphertext = service.Encrypt("Sensitive Patient Data");

        // Tamper with the last 16 bytes (the authentication tag)
        var data = Convert.FromBase64String(ciphertext);
        data[^1] ^= 0xFF; // Flip last byte of the tag
        var tampered = Convert.ToBase64String(data);

        // Act
        var act = () => service.Decrypt(tampered);

        // Assert - AES-GCM authentication failure
        act.Should().Throw<CryptographicException>("tampered tag must fail authentication");
    }

    // ── Null/empty input handling (REQ-PHI-001) ────────────────────────────

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Encrypt_EmptyString_ReturnsEmptyString()
    {
        // Arrange
        var service = new AesGcmPhiEncryptionService(NewKey());

        // Act
        var result = service.Encrypt(string.Empty);

        // Assert
        result.Should().BeEmpty("empty input should pass through without encryption");
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Decrypt_EmptyString_ReturnsEmptyString()
    {
        // Arrange
        var service = new AesGcmPhiEncryptionService(NewKey());

        // Act
        var result = service.Decrypt(string.Empty);

        // Assert
        result.Should().BeEmpty("empty input should pass through without decryption");
    }

    // ── Nonce randomness test (REQ-PHI-001) ────────────────────────────────

    [Fact]
    [Trait("Category", "Security")]
    public void Encrypt_SamePlaintext_ProducesDifferentCiphertext()
    {
        // Arrange
        var service = new AesGcmPhiEncryptionService(NewKey());
        const string plaintext = "1990-01-15"; // BirthDate PHI

        // Act
        var ciphertext1 = service.Encrypt(plaintext);
        var ciphertext2 = service.Encrypt(plaintext);

        // Assert - Nonce randomness ensures probabilistic encryption
        ciphertext1.Should().NotBe(ciphertext2,
            "each encryption must use a fresh nonce (semantic security)");
    }

    // ── Key derivation tests (REQ-PHI-002) ────────────────────────────────

    [Fact]
    [Trait("Category", "KeyDerivation")]
    public void DeriveKey_SameInput_ReturnsSameKey()
    {
        // Arrange
        const string keyMaterial = "sqlcipher-master-password-2026";

        // Act
        var key1 = AesGcmPhiEncryptionService.DeriveKey(keyMaterial);
        var key2 = AesGcmPhiEncryptionService.DeriveKey(keyMaterial);

        // Assert - HKDF is deterministic
        key1.Should().Equal(key2, "HKDF must be deterministic for the same input");
        key1.Should().HaveCount(32, "derived key must be 32 bytes for AES-256");
    }

    [Fact]
    [Trait("Category", "KeyDerivation")]
    public void DeriveKey_DifferentInput_ReturnsDifferentKey()
    {
        // Arrange & Act
        var key1 = AesGcmPhiEncryptionService.DeriveKey("password-a");
        var key2 = AesGcmPhiEncryptionService.DeriveKey("password-b");

        // Assert
        key1.Should().NotEqual(key2, "different inputs must produce different derived keys");
    }

    // ── Boundary value test (REQ-PHI-001) ──────────────────────────────────

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void EncryptDecrypt_SingleCharacter_RoundTripSucceeds()
    {
        // Arrange
        var service = new AesGcmPhiEncryptionService(NewKey());
        const string singleChar = "M"; // Sex field boundary value

        // Act
        var ciphertext = service.Encrypt(singleChar);
        var decrypted = service.Decrypt(ciphertext);

        // Assert
        decrypted.Should().Be(singleChar);
        // Verify output format: [Nonce(12)] + [Ciphertext(1)] + [Tag(16)] = 29 bytes -> base64
        var raw = Convert.FromBase64String(ciphertext);
        raw.Should().HaveCount(29, "output must be 12 (nonce) + 1 (ciphertext) + 16 (tag) bytes");
    }

    // ── Constructor validation tests ────────────────────────────────────────

    [Fact]
    [Trait("Category", "Validation")]
    public void Constructor_NullKey_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AesGcmPhiEncryptionService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Constructor_WrongKeyLength_ThrowsArgumentException()
    {
        // Arrange
        var invalidKey = new byte[16]; // AES-128, not AES-256

        // Act
        var act = () => new AesGcmPhiEncryptionService(invalidKey);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*32 bytes*");
    }

    // ── FromSqlCipherKey factory test ────────────────────────────────────────

    [Fact]
    [Trait("Category", "KeyDerivation")]
    public void FromSqlCipherKey_EncryptDecrypt_RoundTripSucceeds()
    {
        // Arrange - simulate key derived from SQLCipher password
        var service = AesGcmPhiEncryptionService.FromSqlCipherKey("HnVue-Test-Password-2026");
        const string plaintext = "P12345678"; // PatientId PHI

        // Act
        var ciphertext = service.Encrypt(plaintext);
        var decrypted = service.Decrypt(ciphertext);

        // Assert
        decrypted.Should().Be(plaintext);
    }

    // ── Output format validation test ────────────────────────────────────────

    [Fact]
    [Trait("Category", "Format")]
    public void Encrypt_OutputFormat_IsNoncePlusCiphertextPlusTag()
    {
        // Arrange
        var key = NewKey();
        var service = new AesGcmPhiEncryptionService(key);
        const string plaintext = "TestPatient";
        var plaintextBytes = Encoding.UTF8.GetByteCount(plaintext);

        // Act
        var ciphertext = service.Encrypt(plaintext);
        var raw = Convert.FromBase64String(ciphertext);

        // Assert format: Nonce(12) + Ciphertext(N) + Tag(16)
        raw.Length.Should().Be(12 + plaintextBytes + 16,
            "output must be exactly nonce(12) + ciphertext(N) + tag(16)");
    }
}
