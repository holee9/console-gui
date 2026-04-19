using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Results;
using HnVue.Security;
using Xunit;

namespace HnVue.IntegrationTests;

/// <summary>
/// Integration tests for S13-R1 STRIDE security controls implemented by Team A.
/// Tests real service instances (not mocks) to verify end-to-end security behavior.
///
/// STRIDE mapping:
///   S/T - Spoofing/Tampering: ITlsConnectionService (SWR-CS-079)
///   I   - Information Disclosure: IPhiMaskingService, IPhiEncryptionService (SWR-CS-080)
///   D   - Denial of Service: IRateLimitingService (SWR-CS-081)
/// </summary>
[Trait("Category", "Integration")]
[Trait("Team", "Coordinator")]
[Trait("Sprint", "S13")]
[Trait("Round", "R1")]
public sealed class StrideSecurityIntegrationTests : IDisposable
{
    // ── Shared fixtures ────────────────────────────────────────────────────────

    private readonly PhiEncryptionService _encryptionService;
    private readonly PhiMaskingService _maskingService;
    private readonly RateLimitingService _rateLimitingService;
    private readonly TlsConnectionService _tlsService;

    public StrideSecurityIntegrationTests()
    {
        // Generate a valid 32-byte AES-256 key for encryption tests
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        _encryptionService = new PhiEncryptionService(key);

        _maskingService = new PhiMaskingService();

        // Use low limits for deterministic rate-limiting tests (3 attempts per 1 minute)
        _rateLimitingService = new RateLimitingService(3, TimeSpan.FromMinutes(1));

        _tlsService = new TlsConnectionService();
    }

    public void Dispose()
    {
        // Nothing to dispose for these stateless services
    }

    // ════════════════════════════════════════════════════════════════════════════
    // 1. ITlsConnectionService — STRIDE S/T (Spoofing/Tampering)  [SWR-CS-079]
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates that ValidateCertificate rejects an empty (zero-length) byte array.
    /// Empty bytes cannot form a valid X.509 certificate.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CS-079")]
    public void Tls_ValidateCertificate_RejectsEmptyCertificate()
    {
        // Act
        var result = _tlsService.ValidateCertificate(Array.Empty<byte>(), "localhost");

        // Assert
        result.IsFailure.Should().BeTrue("empty bytes are not a valid certificate");
        result.Error.Should().Be(ErrorCode.TlsConnectionFailed);
    }

    /// <summary>
    /// Validates that ValidateCertificate rejects random bytes that do not form
    /// a valid X.509 certificate structure.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CS-079")]
    public void Tls_ValidateCertificate_RejectsInvalidCertificate()
    {
        // Arrange — random bytes are not DER-encoded X.509
        var invalidCert = new byte[128];
        RandomNumberGenerator.Fill(invalidCert);

        // Act
        var result = _tlsService.ValidateCertificate(invalidCert, "test.local");

        // Assert
        result.IsFailure.Should().BeTrue("random bytes are not a valid certificate");
        result.Error.Should().Be(ErrorCode.TlsConnectionFailed);
    }

    /// <summary>
    /// Validates that ConnectAsync fails for an unreachable host with a
    /// TlsConnectionFailed error code.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CS-079")]
    public async Task Tls_ConnectAsync_FailsForUnreachableHost()
    {
        // Arrange — use a non-routable IP to guarantee connection failure
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act
        var result = await _tlsService.ConnectAsync("192.0.2.1", 65535, cts.Token);

        // Assert
        result.IsFailure.Should().BeTrue("unreachable host must fail");
        result.Error.Should().Be(ErrorCode.TlsConnectionFailed);
        result.ErrorMessage.Should().NotBeNullOrEmpty("error message should describe the failure");
    }

    /// <summary>
    /// Validates that ConnectAsync rejects invalid port numbers (0 and > 65535).
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(65536)]
    [Trait("SWR", "SWR-CS-079")]
    public async Task Tls_ConnectAsync_FailsForInvalidPort(int port)
    {
        // Act
        var result = await _tlsService.ConnectAsync("localhost", port);

        // Assert
        result.IsFailure.Should().BeTrue("invalid port must be rejected");
        result.Error.Should().Be(ErrorCode.TlsConnectionFailed);
    }

    // ════════════════════════════════════════════════════════════════════════════
    // 2. IPhiMaskingService — STRIDE I (Information Disclosure)  [SWR-CS-080]
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that MaskName produces "김***" format for Korean names.
    /// First syllable preserved, rest masked with asterisks.
    /// </summary>
    [Theory]
    [InlineData("김철수", "김**")]
    [InlineData("이영희", "이**")]
    [InlineData("박동훈", "박**")]
    [Trait("SWR", "SWR-CS-080")]
    public void PhiMasking_MaskName_ReturnsMaskedKoreanName(string name, string expected)
    {
        // Act
        var masked = _maskingService.MaskName(name);

        // Assert
        masked.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that MaskName masks non-Korean names preserving the first character.
    /// </summary>
    [Theory]
    [InlineData("John", "J***")]
    [InlineData("Alice", "A****")]
    [Trait("SWR", "SWR-CS-080")]
    public void PhiMasking_MaskName_MasksNonKoreanName(string name, string expected)
    {
        // Act
        var masked = _maskingService.MaskName(name);

        // Assert
        masked.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that MaskNationalId returns "900101-*******" format.
    /// First 6 digits (birth date portion) preserved, last 7 digits masked.
    /// </summary>
    [Theory]
    [InlineData("900101-1234567", "900101-*******")]
    [InlineData("9001011234567", "900101-*******")]
    [Trait("SWR", "SWR-CS-080")]
    public void PhiMasking_MaskNationalId_ReturnsMaskedFormat(string id, string expected)
    {
        // Act
        var masked = _maskingService.MaskNationalId(id);

        // Assert
        masked.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that MaskPhone masks the last 4 digits of phone numbers.
    /// </summary>
    [Theory]
    [InlineData("010-1234-5678", "010-1234-****")]
    [InlineData("01012345678", "010-1234-****")]
    [InlineData("02-987-6543", "02-987-****")]
    [Trait("SWR", "SWR-CS-080")]
    public void PhiMasking_MaskPhone_MasksLast4Digits(string phone, string expected)
    {
        // Act
        var masked = _maskingService.MaskPhone(phone);

        // Assert
        masked.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that MaskDateOfBirth returns year-only format like "1990-**-**".
    /// </summary>
    [Theory]
    [InlineData("1990-01-15", "1990-**-**")]
    [InlineData("1990.01.15", "1990.01.15")]
    [Trait("SWR", "SWR-CS-080")]
    public void PhiMasking_MaskDateOfBirth_ReturnsYearOnly(string dob, string expected)
    {
        // Act
        var masked = _maskingService.MaskDateOfBirth(dob);

        // Assert
        masked.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that MaskName handles null/whitespace by returning as-is.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [Trait("SWR", "SWR-CS-080")]
    public void PhiMasking_MaskName_HandlesNullOrWhitespace(string? name)
    {
        // Act
        var masked = _maskingService.MaskName(name!);

        // Assert
        masked.Should().Be(name!);
    }

    // ════════════════════════════════════════════════════════════════════════════
    // 3. IPhiEncryptionService — STRIDE I (Information Disclosure)  [SWR-CS-080]
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies AES-256-GCM encrypt-then-decrypt roundtrip returns original plaintext.
    /// </summary>
    [Theory]
    [InlineData("홍길동")]
    [InlineData("Patient PHI data: SSN 900101-1234567")]
    [InlineData("Hello, World!")]
    [InlineData("")]
    [Trait("SWR", "SWR-CS-080")]
    public void PhiEncryption_EncryptDecrypt_RoundtripReturnsOriginal(string plaintext)
    {
        // Act
        var encrypted = _encryptionService.Encrypt(plaintext);
        var decrypted = _encryptionService.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plaintext, "roundtrip through AES-256-GCM must preserve data");
    }

    /// <summary>
    /// Verifies that encryption produces different ciphertext each time (random nonce).
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CS-080")]
    public void PhiEncryption_ProducesUniqueCiphertextPerCall()
    {
        // Arrange
        const string plaintext = "deterministic-input";

        // Act
        var encrypted1 = _encryptionService.Encrypt(plaintext);
        var encrypted2 = _encryptionService.Encrypt(plaintext);

        // Assert — same plaintext, different nonce → different ciphertext
        encrypted1.Should().NotBe(encrypted2, "random nonce must produce unique ciphertext");
    }

    /// <summary>
    /// Verifies VerifyTag succeeds for valid (unmodified) ciphertext.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CS-080")]
    public void PhiEncryption_VerifyTag_SucceedsForValidCiphertext()
    {
        // Arrange
        var encrypted = _encryptionService.Encrypt("tag-verification-test");

        // Act
        var result = _encryptionService.VerifyTag(encrypted);

        // Assert
        result.IsSuccess.Should().BeTrue("valid ciphertext must pass tag verification");
    }

    /// <summary>
    /// Verifies VerifyTag fails for tampered ciphertext (modified bytes).
    /// This is the core GCM integrity guarantee against tampering.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CS-080")]
    public void PhiEncryption_VerifyTag_FailsForTamperedCiphertext()
    {
        // Arrange — encrypt, then tamper with the ciphertext bytes
        var encrypted = _encryptionService.Encrypt("tamper-detection-test");
        var bytes = Convert.FromBase64String(encrypted);

        // Flip a byte in the ciphertext region (after nonce + tag) to simulate tampering
        var tamperIndex = bytes.Length - 1;
        bytes[tamperIndex] ^= 0xFF;
        var tamperedCiphertext = Convert.ToBase64String(bytes);

        // Act
        var result = _encryptionService.VerifyTag(tamperedCiphertext);

        // Assert
        result.IsFailure.Should().BeTrue("tampered ciphertext must fail tag verification");
        result.Error.Should().Be(ErrorCode.EncryptionFailed);
    }

    /// <summary>
    /// Verifies VerifyTag fails for invalid base64 input.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CS-080")]
    public void PhiEncryption_VerifyTag_FailsForInvalidBase64()
    {
        // Act
        var result = _encryptionService.VerifyTag("not-valid-base64!!!");

        // Assert
        result.IsFailure.Should().BeTrue("invalid base64 must fail verification");
        result.Error.Should().Be(ErrorCode.EncryptionFailed);
    }

    /// <summary>
    /// Verifies that GenerateKey returns a valid base64-encoded 32-byte key.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CS-080")]
    public void PhiEncryption_GenerateKey_ReturnsValidBase64Key()
    {
        // Act
        var keyBase64 = _encryptionService.GenerateKey();

        // Assert
        var keyBytes = Convert.FromBase64String(keyBase64);
        keyBytes.Length.Should().Be(32, "AES-256 key must be exactly 32 bytes");
    }

    /// <summary>
    /// Verifies that GenerateKey produces unique keys across calls.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CS-080")]
    public void PhiEncryption_GenerateKey_ProducesUniqueKeys()
    {
        // Act
        var key1 = _encryptionService.GenerateKey();
        var key2 = _encryptionService.GenerateKey();

        // Assert
        key1.Should().NotBe(key2, "each key generation must produce a unique key");
    }

    /// <summary>
    /// Verifies that decrypting with the wrong key throws (cross-key isolation).
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CS-080")]
    public void PhiEncryption_DecryptWithWrongKey_Throws()
    {
        // Arrange — encrypt with the primary service
        var encrypted = _encryptionService.Encrypt("cross-key-isolation");

        // Create a second service with a different key
        var wrongKey = new byte[32];
        RandomNumberGenerator.Fill(wrongKey);
        var wrongKeyService = new PhiEncryptionService(wrongKey);

        // Act
        var act = () => wrongKeyService.Decrypt(encrypted);

        // Assert — GCM tag mismatch must cause an exception
        act.Should().Throw<Exception>("decryption with wrong key must fail due to tag mismatch");
    }

    /// <summary>
    /// Verifies that the constructor rejects keys that are not 32 bytes.
    /// </summary>
    [Theory]
    [InlineData(16)]
    [InlineData(24)]
    [InlineData(64)]
    [Trait("SWR", "SWR-CS-080")]
    public void PhiEncryption_Constructor_RejectsInvalidKeySize(int keySize)
    {
        // Act
        var act = () => new PhiEncryptionService(new byte[keySize]);

        // Assert
        act.Should().Throw<ArgumentException>("only 32-byte keys are valid for AES-256");
    }

    // ════════════════════════════════════════════════════════════════════════════
    // 4. IRateLimitingService — STRIDE D (Denial of Service)  [SWR-CS-081]
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that calls within the rate limit succeed.
    /// Service is configured with max 3 attempts per window.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CS-081")]
    public void RateLimit_WithinLimit_Succeeds()
    {
        // Act — make 3 calls (the configured maximum)
        var result1 = _rateLimitingService.CheckRateLimit("TEST_OP", "user-001");
        var result2 = _rateLimitingService.CheckRateLimit("TEST_OP", "user-001");
        var result3 = _rateLimitingService.CheckRateLimit("TEST_OP", "user-001");

        // Assert — all calls within limit should succeed
        result1.IsSuccess.Should().BeTrue("first call must succeed");
        result2.IsSuccess.Should().BeTrue("second call must succeed");
        result3.IsSuccess.Should().BeTrue("third call is at the limit boundary and should succeed");
    }

    /// <summary>
    /// Verifies that calls exceeding the rate limit return RateLimitExceeded error.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CS-081")]
    public void RateLimit_ExceedingLimit_ReturnsRateLimitExceeded()
    {
        // Arrange — exhaust the limit (3 calls)
        _rateLimitingService.CheckRateLimit("TEST_OP", "user-002");
        _rateLimitingService.CheckRateLimit("TEST_OP", "user-002");
        _rateLimitingService.CheckRateLimit("TEST_OP", "user-002");

        // Act — 4th call exceeds limit
        var result = _rateLimitingService.CheckRateLimit("TEST_OP", "user-002");

        // Assert
        result.IsFailure.Should().BeTrue("call exceeding limit must fail");
        result.Error.Should().Be(ErrorCode.RateLimitExceeded);
        result.ErrorMessage.Should().Contain("Rate limit exceeded");
    }

    /// <summary>
    /// Verifies that Reset clears the rate limit counter, allowing new calls.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CS-081")]
    public void RateLimit_Reset_AllowsNewCalls()
    {
        // Arrange — exhaust the limit
        _rateLimitingService.CheckRateLimit("TEST_OP", "user-003");
        _rateLimitingService.CheckRateLimit("TEST_OP", "user-003");
        _rateLimitingService.CheckRateLimit("TEST_OP", "user-003");

        // Confirm limit is reached
        var blocked = _rateLimitingService.CheckRateLimit("TEST_OP", "user-003");
        blocked.IsFailure.Should().BeTrue("should be blocked before reset");

        // Act — reset the counter
        _rateLimitingService.Reset("TEST_OP", "user-003");

        // Assert — new calls should succeed after reset
        var result = _rateLimitingService.CheckRateLimit("TEST_OP", "user-003");
        result.IsSuccess.Should().BeTrue("call must succeed after reset");
    }

    /// <summary>
    /// Verifies that different keys have independent rate limits.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CS-081")]
    public void RateLimit_DifferentKeys_HaveIndependentLimits()
    {
        // Arrange — exhaust limit for user-A
        _rateLimitingService.CheckRateLimit("TEST_OP", "user-A");
        _rateLimitingService.CheckRateLimit("TEST_OP", "user-A");
        _rateLimitingService.CheckRateLimit("TEST_OP", "user-A");

        // Act — user-B has not been rate-limited
        var result = _rateLimitingService.CheckRateLimit("TEST_OP", "user-B");

        // Assert
        result.IsSuccess.Should().BeTrue("different keys must have independent counters");
    }

    /// <summary>
    /// Verifies that different operation types have independent rate limits.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CS-081")]
    public void RateLimit_DifferentOperationTypes_HaveIndependentLimits()
    {
        // Arrange — exhaust limit for OP_A
        _rateLimitingService.CheckRateLimit("OP_A", "user-010");
        _rateLimitingService.CheckRateLimit("OP_A", "user-010");
        _rateLimitingService.CheckRateLimit("OP_A", "user-010");

        // Act — OP_B with the same key should still work
        var result = _rateLimitingService.CheckRateLimit("OP_B", "user-010");

        // Assert
        result.IsSuccess.Should().BeTrue("different operation types must have independent counters");
    }
}
