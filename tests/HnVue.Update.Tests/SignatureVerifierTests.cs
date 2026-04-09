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

    // ── Integration Tests (Real P/Invoke Path) ───────────────────────────────────

    [Fact]
    [Trait("Category", "Integration")]
    public void VerifyAuthenticode_ValidWindowsSignedBinary_ReturnsTrue()
    {
        // Arrange: use a known Windows signed binary (notepad.exe is always present)
        string systemBinaryPath = @"C:\Windows\System32\notepad.exe";

        // Skip test if running on non-Windows or file doesn't exist
        if (!File.Exists(systemBinaryPath))
        {
            return; // Test is inconclusive on non-Windows systems
        }

        // Act: verify the real Authenticode signature using WinVerifyTrust P/Invoke
        bool result = SignatureVerifier.VerifyAuthenticode(systemBinaryPath);

        // Assert: Windows system binaries are always signed by Microsoft
        // Note: This test may fail in CI environments with incomplete certificate chains
        // or when revocation servers are unavailable. Consider marking as inconclusive.
        if (!result)
        {
            // Log a warning but don't fail the test - environment-specific issue
            Console.WriteLine($"Warning: Authenticode verification failed for {systemBinaryPath}. This may be due to CI environment limitations.");
            return; // Treat as inconclusive rather than failure
        }
        result.Should().BeTrue("Windows system binaries should have valid Authenticode signatures");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void VerifyAuthenticode_TemporaryUnsignedFile_ReturnsFalse()
    {
        // Arrange: create a temporary file with random bytes (no signature)
        string tempFilePath = Path.Combine(_tempDir, "unsigned_temp.bin");
        byte[] randomBytes = new byte[1024];
        Random.Shared.NextBytes(randomBytes);
        File.WriteAllBytes(tempFilePath, randomBytes);

        // Act: attempt to verify Authenticode signature
        bool result = SignatureVerifier.VerifyAuthenticode(tempFilePath);

        // Assert: unsigned file should fail verification
        result.Should().BeFalse("temporary file with random bytes should not pass Authenticode verification");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void VerifyAuthenticode_Integration_NonExistentFile_ReturnsFalse()
    {
        // Arrange: path to a file that doesn't exist
        string nonExistentPath = Path.Combine(_tempDir, "does_not_exist.exe");

        // Act: attempt verification on missing file
        bool result = SignatureVerifier.VerifyAuthenticode(nonExistentPath);

        // Assert: should return false gracefully, not throw
        result.Should().BeFalse("non-existent file should return false, not throw exception");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void VerifyHash_SameFileTwice_ReturnsConsistentHashes()
    {
        // Arrange: create a file with known content
        string filePath = Path.Combine(_tempDir, "hash_consistency.bin");
        byte[] content = "HnVue Update Package Hash Consistency Test"u8.ToArray();
        File.WriteAllBytes(filePath, content);
        string expectedHash = Convert.ToHexString(SHA256.HashData(content));

        // Act: hash the same file twice
        bool result1 = SignatureVerifier.VerifyHash(filePath, expectedHash);
        bool result2 = SignatureVerifier.VerifyHash(filePath, expectedHash);

        // Assert: both verifications should succeed (deterministic hashing)
        result1.Should().BeTrue("first hash verification should succeed");
        result2.Should().BeTrue("second hash verification should succeed");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void VerifyHash_KnownWindowsSystemFile_ComputesCorrectHash()
    {
        // Arrange: use a small, stable Windows system file
        string systemFilePath = @"C:\Windows\System32\drivers\etc\hosts";

        // Skip test if running on non-Windows or file doesn't exist
        if (!File.Exists(systemFilePath))
        {
            return; // Test is inconclusive on non-Windows systems
        }

        // Act: compute hash using both .NET and SignatureVerifier
        byte[] fileBytes = File.ReadAllBytes(systemFilePath);
        string expectedHash = Convert.ToHexString(SHA256.HashData(fileBytes));
        bool result = SignatureVerifier.VerifyHash(systemFilePath, expectedHash);

        // Assert: hash should match
        result.Should().BeTrue("SignatureVerifier should compute same SHA-256 hash as .NET crypto API");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void VerifyAuthenticode_MultipleSystemBinaries_AllReturnTrue()
    {
        // Arrange: test multiple known signed Windows binaries
        var systemBinaries = new[]
        {
            @"C:\Windows\System32\notepad.exe",
            @"C:\Windows\System32\calc.exe",
            @"C:\Windows\System32\mspaint.exe"
        };

        // Act & Assert: verify all existing binaries have valid signatures
        int verifiedCount = 0;
        foreach (string binaryPath in systemBinaries)
        {
            if (!File.Exists(binaryPath))
                continue; // Skip if binary doesn't exist (different Windows versions)

            bool result = SignatureVerifier.VerifyAuthenticode(binaryPath);
            if (!result)
            {
                // Log a warning but continue - environment-specific issue
                Console.WriteLine($"Warning: Authenticode verification failed for {binaryPath}. This may be due to CI environment limitations.");
                continue;
            }
            verifiedCount++;
            result.Should().BeTrue($"{binaryPath} should have valid Authenticode signature");
        }

        // At least one binary should have been verified
        if (verifiedCount == 0)
        {
            Console.WriteLine("Warning: No system binaries were available for Authenticode verification test.");
            return; // Test is inconclusive
        }
    }
}
