using System.IO;
using System.Security.Cryptography;
using FluentAssertions;
using HnVue.Update;
using Xunit;

namespace HnVue.Update.Tests;

/// <summary>
/// Tests for <see cref="SignatureVerifier"/> hash verification logic.
/// Authenticode tests are limited to non-existent / unsigned files because
/// a valid code-signing certificate is not available in the test environment.
/// </summary>
public sealed class SignatureVerifierTests : IDisposable
{
    private readonly string _tempDir;

    public SignatureVerifierTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"HnVueUpdateTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // ── VerifyHash ─────────────────────────────────────────────────────────────

    [Fact]
    public void VerifyHash_ValidFile_ReturnsTrue()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "package.zip");
        byte[] content = "hello world update package"u8.ToArray();
        File.WriteAllBytes(filePath, content);
        string expectedHash = Convert.ToHexString(SHA256.HashData(content));

        // Act
        bool result = SignatureVerifier.VerifyHash(filePath, expectedHash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyHash_ValidFile_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "package_ci.zip");
        byte[] content = "case insensitive hash test"u8.ToArray();
        File.WriteAllBytes(filePath, content);
        string expectedHash = Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();

        // Act
        bool result = SignatureVerifier.VerifyHash(filePath, expectedHash);

        // Assert
        result.Should().BeTrue("hash comparison should be case-insensitive");
    }

    [Fact]
    public void VerifyHash_TamperedFile_ReturnsFalse()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "tampered.zip");
        byte[] originalContent = "original content"u8.ToArray();
        byte[] tamperedContent = "tampered content!"u8.ToArray();
        File.WriteAllBytes(filePath, tamperedContent);
        string originalHash = Convert.ToHexString(SHA256.HashData(originalContent));

        // Act
        bool result = SignatureVerifier.VerifyHash(filePath, originalHash);

        // Assert
        result.Should().BeFalse("tampered file should not match the original hash");
    }

    [Fact]
    public void VerifyHash_NonExistentFile_ReturnsFalse()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "does_not_exist.zip");
        string anyHash = new string('A', 64);

        // Act
        bool result = SignatureVerifier.VerifyHash(filePath, anyHash);

        // Assert
        result.Should().BeFalse("non-existent file should return false, not throw");
    }

    [Fact]
    public void VerifyHash_EmptyExpectedHash_ReturnsFalse()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "file.zip");
        File.WriteAllBytes(filePath, "data"u8.ToArray());

        // Act
        bool result = SignatureVerifier.VerifyHash(filePath, string.Empty);

        // Assert
        result.Should().BeFalse("empty expected hash should return false");
    }

    [Fact]
    public void VerifyHash_EmptyFile_ComputesHashCorrectly()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "empty.zip");
        File.WriteAllBytes(filePath, []);
        string expectedHash = Convert.ToHexString(SHA256.HashData([]));

        // Act
        bool result = SignatureVerifier.VerifyHash(filePath, expectedHash);

        // Assert
        result.Should().BeTrue("empty file hash should match the SHA-256 of empty bytes");
    }

    [Fact]
    public void VerifyHash_WrongHash_ReturnsFalse()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "wrong_hash.zip");
        File.WriteAllBytes(filePath, "some content"u8.ToArray());
        string wrongHash = new string('0', 64); // All-zeros SHA-256 (highly unlikely to be correct)

        // Act
        bool result = SignatureVerifier.VerifyHash(filePath, wrongHash);

        // Assert
        result.Should().BeFalse();
    }

    // ── VerifyAuthenticode ─────────────────────────────────────────────────────

    [Fact]
    public void VerifyAuthenticode_NonExistentFile_ReturnsFalse()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "nonexistent.exe");

        // Act
        bool result = SignatureVerifier.VerifyAuthenticode(filePath);

        // Assert
        result.Should().BeFalse("non-existent file should return false, not throw");
    }

    [Fact]
    public void VerifyAuthenticode_UnsignedFile_ReturnsFalse()
    {
        // Arrange: write a plain text file — it will fail Authenticode verification
        string filePath = Path.Combine(_tempDir, "unsigned.exe");
        File.WriteAllText(filePath, "this is not a signed PE binary");

        // Act
        bool result = SignatureVerifier.VerifyAuthenticode(filePath);

        // Assert
        result.Should().BeFalse("a plain text file should not pass Authenticode verification");
    }
}
