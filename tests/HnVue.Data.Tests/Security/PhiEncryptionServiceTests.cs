using System.Security.Cryptography;
using HnVue.Common.Abstractions;
using HnVue.Data.Security;
using Xunit;

namespace HnVue.Data.Tests.Security;

/// <summary>
/// Tests for AES-256-GCM PHI encryption service (REQ-DATA-001).
/// Verifies encryption/decryption roundtrip and format compliance.
/// </summary>
public class PhiEncryptionServiceTests
{
    [Fact]
    public void Encrypt_Decrypt_Roundtrip_Success()
    {
        // Arrange
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        var encryptionService = new PhiEncryptionService(key);
        var plaintext = "John Doe";

        // Act
        var ciphertext = encryptionService.Encrypt(plaintext);
        var decrypted = encryptionService.Decrypt(ciphertext);

        // Assert
        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void Encrypt_SamePlaintext_DifferentCiphertext()
    {
        // Arrange
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        var encryptionService = new PhiEncryptionService(key);
        var plaintext = "Jane Smith";

        // Act
        var ciphertext1 = encryptionService.Encrypt(plaintext);
        var ciphertext2 = encryptionService.Encrypt(plaintext);

        // Assert - Nonce randomization ensures different ciphertext
        Assert.NotEqual(ciphertext1, ciphertext2);
    }

    [Fact]
    public void Decrypt_WrongKey_ThrowsException()
    {
        // Arrange
        var key1 = new byte[32];
        var key2 = new byte[32];
        RandomNumberGenerator.Fill(key1);
        RandomNumberGenerator.Fill(key2);
        var encryptionService1 = new PhiEncryptionService(key1);
        var encryptionService2 = new PhiEncryptionService(key2);
        var plaintext = "Patient PHI";

        // Act
        var ciphertext = encryptionService1.Encrypt(plaintext);
        Action decryptAction = () => encryptionService2.Decrypt(ciphertext);

        // Assert - AES-GCM authentication failure
        Assert.ThrowsAny<CryptographicException>(decryptAction);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Encrypt_NullOrEmpty_ReturnsInput(string? plaintext)
    {
        // Arrange
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        var encryptionService = new PhiEncryptionService(key);

        // Act
        var result = encryptionService.Encrypt(plaintext!);

        // Assert
        Assert.Equal(plaintext, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Decrypt_NullOrEmpty_ReturnsInput(string? ciphertext)
    {
        // Arrange
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        var encryptionService = new PhiEncryptionService(key);

        // Act
        var result = encryptionService.Decrypt(ciphertext!);

        // Assert
        Assert.Equal(ciphertext, result);
    }

    [Fact]
    public void Constructor_NullKey_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PhiEncryptionService(null!));
    }

    [Fact]
    public void Constructor_InvalidKeyLength_ThrowsArgumentException()
    {
        // Arrange
        var invalidKey = new byte[16]; // Wrong size

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PhiEncryptionService(invalidKey));
    }

    [Fact]
    public void Decrypt_InvalidFormat_ThrowsFormatException()
    {
        // Arrange
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        var encryptionService = new PhiEncryptionService(key);
        var invalidCiphertext = "InvalidBase64!";

        // Act
        Action decryptAction = () => encryptionService.Decrypt(invalidCiphertext);

        // Assert
        Assert.ThrowsAny<FormatException>(decryptAction);
    }
}
