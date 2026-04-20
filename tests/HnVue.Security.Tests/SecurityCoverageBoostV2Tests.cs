using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Configuration;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Security;
using HnVue.Security.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HnVue.Security.Tests;

/// <summary>
/// Additional coverage tests for Security module targeting 90%+.
/// Covers PhiMaskingService, RateLimitingService, RoleElevationValidator,
/// TlsConnectionService, PhiEncryptionService, ReauthenticateAsync.
/// </summary>
[Collection("Security-Sequential")]
public sealed class SecurityCoverageBoostV2Tests
{
    // ── PhiMaskingService ──────────────────────────────────────────────────────

    [Fact]
    public void MaskName_KoreanName_MasksSurname()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskName("홍길동");
        result.Should().Be("홍**");
    }

    [Fact]
    public void MaskName_KoreanTwoCharName_MasksGiven()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskName("김철");
        result.Should().Be("김*");
    }

    [Fact]
    public void MaskName_EnglishName_MasksRest()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskName("John Doe");
        result.Should().Be("J*******");
    }

    [Fact]
    public void MaskName_SingleChar_ReturnsStar()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskName("A");
        result.Should().Be("*");
    }

    [Fact]
    public void MaskName_Null_ReturnsNull()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskName(null!);
        result.Should().BeNull();
    }

    [Fact]
    public void MaskName_Empty_ReturnsEmpty()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskName(string.Empty);
        result.Should().BeEmpty();
    }

    [Fact]
    public void MaskName_Whitespace_ReturnsSame()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskName("   ");
        result.Should().Be("   ");
    }

    [Fact]
    public void MaskNationalId_ValidFormat_MasksLast7()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskNationalId("900101-1234567");
        result.Should().Be("900101-*******");
    }

    [Fact]
    public void MaskNationalId_NoHyphen_MasksLast7()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskNationalId("9001011234567");
        result.Should().Be("900101-*******");
    }

    [Fact]
    public void MaskNationalId_ShortValue_MasksAll()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskNationalId("12345");
        result.Should().Be("*****");
    }

    [Fact]
    public void MaskNationalId_Null_ReturnsNull()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskNationalId(null!);
        result.Should().BeNull();
    }

    [Fact]
    public void MaskNationalId_LongNonMatching_FallbackMask()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskNationalId("ABCDEFGH1234");
        result.Should().Be("ABCDEF******");
    }

    [Fact]
    public void MaskPhone_KoreanFormat_MasksLast4()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskPhone("010-1234-5678");
        result.Should().Be("010-1234-****");
    }

    [Fact]
    public void MaskPhone_NoHyphen_MasksLast4()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskPhone("021234567");
        result.Should().Be("02-123-****");
    }

    [Fact]
    public void MaskPhone_ShortValue_MasksAll()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskPhone("123");
        result.Should().Be("***");
    }

    [Fact]
    public void MaskPhone_Null_ReturnsNull()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskPhone(null!);
        result.Should().BeNull();
    }

    [Fact]
    public void MaskPhone_FallbackLong_MasksLast4()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskPhone("+1-234-567-8901");
        result.Should().Be("+1-234-567-****");
    }

    [Fact]
    public void MaskDateOfBirth_ValidDate_MasksMonthDay()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskDateOfBirth("1990-01-15");
        result.Should().Be("1990-**-**");
    }

    [Fact]
    public void MaskDateOfBirth_NoHyphen_MasksMonthDay()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskDateOfBirth("19900115");
        result.Should().Be("1990-**-**");
    }

    [Fact]
    public void MaskDateOfBirth_Null_ReturnsNull()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskDateOfBirth(null!);
        result.Should().BeNull();
    }

    [Fact]
    public void MaskDateOfBirth_InvalidFormat_ReturnsOriginal()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskDateOfBirth("Jan 15, 1990");
        result.Should().Be("Jan 15, 1990");
    }

    [Fact]
    public void MaskPatientDisplay_NonNull_ReturnsSame()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskPatientDisplay("patient info");
        result.Should().Be("patient info");
    }

    [Fact]
    public void MaskPatientDisplay_Null_ReturnsNull()
    {
        var sut = new PhiMaskingService();
        var result = sut.MaskPatientDisplay(null!);
        result.Should().BeNull();
    }

    // ── RateLimitingService ────────────────────────────────────────────────────

    [Fact]
    public void CheckRateLimit_DefaultAllows_UpToLimit()
    {
        var sut = new RateLimitingService(3, TimeSpan.FromMinutes(5));
        for (int i = 0; i < 3; i++)
            sut.CheckRateLimit("TEST", "key1").IsSuccess.Should().BeTrue();

        var result = sut.CheckRateLimit("TEST", "key1");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void CheckRateLimit_LoginPolicy_Allows10Attempts()
    {
        var sut = new RateLimitingService();
        for (int i = 0; i < 10; i++)
            sut.CheckRateLimit("LOGIN", "user1").IsSuccess.Should().BeTrue();

        sut.CheckRateLimit("LOGIN", "user1").IsFailure.Should().BeTrue();
    }

    [Fact]
    public void CheckRateLimit_ExposurePolicy_Allows30Attempts()
    {
        var sut = new RateLimitingService();
        for (int i = 0; i < 30; i++)
            sut.CheckRateLimit("EXPOSURE", "device1").IsSuccess.Should().BeTrue();

        sut.CheckRateLimit("EXPOSURE", "device1").IsFailure.Should().BeTrue();
    }

    [Fact]
    public void CheckRateLimit_PasswordChangePolicy_Allows5()
    {
        var sut = new RateLimitingService();
        for (int i = 0; i < 5; i++)
            sut.CheckRateLimit("PASSWORD_CHANGE", "user1").IsSuccess.Should().BeTrue();

        sut.CheckRateLimit("PASSWORD_CHANGE", "user1").IsFailure.Should().BeTrue();
    }

    [Fact]
    public void CheckRateLimit_PinVerifyPolicy_Allows5()
    {
        var sut = new RateLimitingService();
        for (int i = 0; i < 5; i++)
            sut.CheckRateLimit("PIN_VERIFY", "user1").IsSuccess.Should().BeTrue();

        sut.CheckRateLimit("PIN_VERIFY", "user1").IsFailure.Should().BeTrue();
    }

    [Fact]
    public void CheckRateLimit_DifferentKeys_IndependentCounters()
    {
        var sut = new RateLimitingService(1, TimeSpan.FromMinutes(5));
        sut.CheckRateLimit("TEST", "key1").IsSuccess.Should().BeTrue();
        sut.CheckRateLimit("TEST", "key1").IsFailure.Should().BeTrue();
        sut.CheckRateLimit("TEST", "key2").IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CheckRateLimit_NullOperation_Throws()
    {
        var sut = new RateLimitingService();
        var act = () => sut.CheckRateLimit(null!, "key");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CheckRateLimit_NullKey_Throws()
    {
        var sut = new RateLimitingService();
        var act = () => sut.CheckRateLimit("TEST", null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Reset_AllowsNewAttempts()
    {
        var sut = new RateLimitingService(1, TimeSpan.FromMinutes(5));
        sut.CheckRateLimit("TEST", "key1").IsSuccess.Should().BeTrue();
        sut.CheckRateLimit("TEST", "key1").IsFailure.Should().BeTrue();

        sut.Reset("TEST", "key1");
        sut.CheckRateLimit("TEST", "key1").IsSuccess.Should().BeTrue();
    }

    // ── RoleElevationValidator ──────────────────────────────────────────────────

    [Fact]
    public void ValidateRoleAssignment_AdminCannotAssignAdmin()
    {
        var result = RoleElevationValidator.ValidateRoleAssignment(UserRole.Admin, UserRole.Admin);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ValidateRoleAssignment_AdminCannotAssignService()
    {
        var result = RoleElevationValidator.ValidateRoleAssignment(UserRole.Admin, UserRole.Service);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ValidateRoleAssignment_AdminCanAssignRadiologist()
    {
        var result = RoleElevationValidator.ValidateRoleAssignment(UserRole.Admin, UserRole.Radiologist);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateRoleAssignment_AdminCanAssignRadiographer()
    {
        var result = RoleElevationValidator.ValidateRoleAssignment(UserRole.Admin, UserRole.Radiographer);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateRoleAssignment_RadiologistCannotAssignRadiologist()
    {
        var result = RoleElevationValidator.ValidateRoleAssignment(UserRole.Radiologist, UserRole.Radiologist);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ValidateRoleAssignment_RadiologistCanAssignRadiographer()
    {
        var result = RoleElevationValidator.ValidateRoleAssignment(UserRole.Radiologist, UserRole.Radiographer);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateRoleAssignment_RadiographerCannotAssignRadiologist()
    {
        var result = RoleElevationValidator.ValidateRoleAssignment(UserRole.Radiographer, UserRole.Radiologist);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ValidateRoleAssignment_ServiceCannotAssignAdmin()
    {
        var result = RoleElevationValidator.ValidateRoleAssignment(UserRole.Service, UserRole.Admin);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ValidateRoleAssignment_ServiceCanAssignRadiographer()
    {
        var result = RoleElevationValidator.ValidateRoleAssignment(UserRole.Service, UserRole.Radiographer);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateNoSelfElevation_HigherRole_Blocked()
    {
        var result = RoleElevationValidator.ValidateNoSelfElevation(UserRole.Radiographer, UserRole.Admin);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ValidateNoSelfElevation_SameRole_Allowed()
    {
        var result = RoleElevationValidator.ValidateNoSelfElevation(UserRole.Radiographer, UserRole.Radiographer);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateNoSelfElevation_LowerRole_Allowed()
    {
        var result = RoleElevationValidator.ValidateNoSelfElevation(UserRole.Admin, UserRole.Radiographer);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void RequiresReauthentication_Admin_ReturnsTrue()
    {
        RoleElevationValidator.RequiresReauthentication(UserRole.Admin).Should().BeTrue();
    }

    [Fact]
    public void RequiresReauthentication_Service_ReturnsTrue()
    {
        RoleElevationValidator.RequiresReauthentication(UserRole.Service).Should().BeTrue();
    }

    [Fact]
    public void RequiresReauthentication_Radiographer_ReturnsFalse()
    {
        RoleElevationValidator.RequiresReauthentication(UserRole.Radiographer).Should().BeFalse();
    }

    [Fact]
    public void RequiresReauthentication_Radiologist_ReturnsFalse()
    {
        RoleElevationValidator.RequiresReauthentication(UserRole.Radiologist).Should().BeFalse();
    }

    // ── TlsConnectionService ────────────────────────────────────────────────────

    [Fact]
    public void ConnectAsync_EmptyHost_Throws()
    {
        var sut = new TlsConnectionService();
        var act = async () => await sut.ConnectAsync("", 443);
        act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ConnectAsync_InvalidPort_ReturnsFailure()
    {
        var sut = new TlsConnectionService();
        var result = await sut.ConnectAsync("localhost", 0);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ConnectAsync_PortTooLarge_ReturnsFailure()
    {
        var sut = new TlsConnectionService();
        var result = await sut.ConnectAsync("localhost", 70000);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ConnectAsync_UnreachableHost_ReturnsFailure()
    {
        var sut = new TlsConnectionService();
        var result = await sut.ConnectAsync("192.0.2.1", 12345);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ValidateCertificate_NullBytes_Throws()
    {
        var sut = new TlsConnectionService();
        var act = () => sut.ValidateCertificate(null!, "localhost");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateCertificate_EmptyHost_Throws()
    {
        var sut = new TlsConnectionService();
        var act = () => sut.ValidateCertificate([], "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateCertificate_InvalidBytes_ReturnsFailure()
    {
        var sut = new TlsConnectionService();
        var result = sut.ValidateCertificate([1, 2, 3, 4], "localhost");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ValidateCertificate_ExpiredCert_ReturnsFailure()
    {
        var sut = new TlsConnectionService();
        using var cert = CreateSelfSignedCert(DateTimeOffset.UtcNow.AddYears(-2), DateTimeOffset.UtcNow.AddDays(-1));
        var bytes = cert.Export(X509ContentType.Cert);
        var result = sut.ValidateCertificate(bytes, "localhost");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ValidateCertificate_ValidCertWithWrongHost_ReturnsFailure()
    {
        var sut = new TlsConnectionService();
        using var cert = CreateSelfSignedCert(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1), "correct.host");
        var bytes = cert.Export(X509ContentType.Cert);
        var result = sut.ValidateCertificate(bytes, "wrong.host");
        result.IsFailure.Should().BeTrue();
    }

    // ── PhiEncryptionService additional coverage ────────────────────────────────

    [Fact]
    public void PhiEncryption_NullKey_Throws()
    {
        var act = () => new PhiEncryptionService(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PhiEncryption_WrongKeySize_Throws()
    {
        var act = () => new PhiEncryptionService(new byte[16]);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Encrypt_Null_ReturnsNull()
    {
        var sut = new PhiEncryptionService(new byte[32]);
        var result = sut.Encrypt(null!);
        result.Should().BeNull();
    }

    [Fact]
    public void Encrypt_Empty_ReturnsEmpty()
    {
        var sut = new PhiEncryptionService(new byte[32]);
        var result = sut.Encrypt(string.Empty);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Decrypt_Null_ReturnsNull()
    {
        var sut = new PhiEncryptionService(new byte[32]);
        var result = sut.Decrypt(null!);
        result.Should().BeNull();
    }

    [Fact]
    public void Decrypt_Empty_ReturnsEmpty()
    {
        var sut = new PhiEncryptionService(new byte[32]);
        var result = sut.Decrypt(string.Empty);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Decrypt_InvalidBase64_Throws()
    {
        var sut = new PhiEncryptionService(new byte[32]);
        var act = () => sut.Decrypt("not-valid-base64!!!");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Decrypt_TruncatedData_Throws()
    {
        var sut = new PhiEncryptionService(new byte[32]);
        var truncated = Convert.ToBase64String(new byte[10]);
        var act = () => sut.Decrypt(truncated);
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void VerifyTag_Empty_ReturnsSuccess()
    {
        var sut = new PhiEncryptionService(new byte[32]);
        var result = sut.VerifyTag(string.Empty);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void VerifyTag_Null_ReturnsSuccess()
    {
        var sut = new PhiEncryptionService(new byte[32]);
        var result = sut.VerifyTag(null!);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void VerifyTag_InvalidBase64_ReturnsFailure()
    {
        var sut = new PhiEncryptionService(new byte[32]);
        var result = sut.VerifyTag("!!!invalid!!!");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void VerifyTag_TruncatedData_ReturnsFailure()
    {
        var sut = new PhiEncryptionService(new byte[32]);
        var truncated = Convert.ToBase64String(new byte[10]);
        var result = sut.VerifyTag(truncated);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void VerifyTag_TamperedData_ReturnsFailure()
    {
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        var sut = new PhiEncryptionService(key);

        var encrypted = sut.Encrypt("test data");
        var bytes = Convert.FromBase64String(encrypted);
        bytes[^1] ^= 0xFF; // Flip last byte
        var tampered = Convert.ToBase64String(bytes);

        var result = sut.VerifyTag(tampered);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void VerifyTag_ValidData_ReturnsSuccess()
    {
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        var sut = new PhiEncryptionService(key);

        var encrypted = sut.Encrypt("test data");
        var result = sut.VerifyTag(encrypted);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void GenerateKey_Returns32BytesBase64()
    {
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        var sut = new PhiEncryptionService(key);

        var generatedKey = sut.GenerateKey();
        var keyBytes = Convert.FromBase64String(generatedKey);
        keyBytes.Length.Should().Be(32);
    }

    [Fact]
    public void EncryptDecrypt_Roundtrip_Works()
    {
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        var sut = new PhiEncryptionService(key);

        var original = "환자 PHD 데이터 encryption test 홍길동";
        var encrypted = sut.Encrypt(original);
        var decrypted = sut.Decrypt(encrypted);
        decrypted.Should().Be(original);
    }

    [Fact]
    public void Encrypt_DifferentNonceEachCall()
    {
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        var sut = new PhiEncryptionService(key);

        var enc1 = sut.Encrypt("same input");
        var enc2 = sut.Encrypt("same input");
        enc1.Should().NotBe(enc2);
    }

    // ── ReauthenticateAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task ReauthenticateAsync_ValidPin_ReturnsSuccess()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        var pinHash = PasswordHasher.HashPassword("1234");
        var user = new UserRecord(
            "user1", "testuser", "Test User", "hash", UserRole.Admin,
            0, false, null, pinHash, 0, null);

        userRepo.GetByIdAsync("user1", Arg.Any<CancellationToken>()).Returns(Result.Success(user));
        userRepo.GetQuickPinHashAsync("user1", Arg.Any<CancellationToken>())
            .Returns(Result.Success<string?>(pinHash));
        auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<string?>(null));
        auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);
        var result = await sut.ReauthenticateAsync("user1", "1234");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ReauthenticateAsync_InvalidPin_ReturnsFailure()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        var pinHash = PasswordHasher.HashPassword("1234");
        var user = new UserRecord(
            "user1", "testuser", "Test User", "hash", UserRole.Admin,
            0, false, null, pinHash, 0, null);

        userRepo.GetByIdAsync("user1", Arg.Any<CancellationToken>()).Returns(Result.Success(user));
        userRepo.GetQuickPinHashAsync("user1", Arg.Any<CancellationToken>())
            .Returns(Result.Success<string?>(pinHash));
        userRepo.UpdateQuickPinFailureAsync("user1", 1, null, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<string?>(null));
        auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);
        var result = await sut.ReauthenticateAsync("user1", "wrong-pin");
        result.IsFailure.Should().BeTrue();
    }

    // ── AuditService.DetectTamperedEntriesAsync ──────────────────────────────────

    [Fact]
    public async Task DetectTamperedEntriesAsync_NoTampering_ReturnsEmptyList()
    {
        var auditRepo = Substitute.For<IAuditRepository>();
        var hmacKey = new string('h', 32);
        var auditOpts = Options.Create(new AuditOptions { HmacKey = hmacKey });
        var sut = new AuditService(auditRepo, auditOpts);

        var keyBytes = Encoding.UTF8.GetBytes(hmacKey);
        var ts = DateTimeOffset.UtcNow;
        var payload1 = $"id1|{ts:O}|user1|LOGIN||";
        var hash1 = ComputeTestHmac(payload1, keyBytes);
        var payload2 = $"id2|{ts:O}|user1|LOGOUT||{hash1}";
        var hash2 = ComputeTestHmac(payload2, keyBytes);

        var entries = new List<AuditEntry>
        {
            new("id1", ts, "user1", "LOGIN", null, null, hash1),
            new("id2", ts, "user1", "LOGOUT", null, hash1, hash2),
        };

        auditRepo.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<AuditEntry>>(entries));

        var result = await sut.DetectTamperedEntriesAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task DetectTamperedEntriesAsync_TamperedHash_ReturnsEntryId()
    {
        var auditRepo = Substitute.For<IAuditRepository>();
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });
        var sut = new AuditService(auditRepo, auditOpts);

        var ts = DateTimeOffset.UtcNow;
        var entries = new List<AuditEntry>
        {
            new("id1", ts, "user1", "LOGIN", null, null, "tampered_hash"),
        };

        auditRepo.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<AuditEntry>>(entries));

        var result = await sut.DetectTamperedEntriesAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("id1");
    }

    [Fact]
    public async Task DetectTamperedEntriesAsync_BrokenChain_ReturnsTamperedEntry()
    {
        var auditRepo = Substitute.For<IAuditRepository>();
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });
        var sut = new AuditService(auditRepo, auditOpts);

        var keyBytes = Encoding.UTF8.GetBytes(new string('h', 32));
        var ts = DateTimeOffset.UtcNow;
        var hash1 = ComputeTestHmac($"id1|{ts:O}|user1|LOGIN||", keyBytes);
        var hash2 = ComputeTestHmac($"id2|{ts:O}|user1|LOGOUT||{hash1}", keyBytes);

        var entries = new List<AuditEntry>
        {
            new("id1", ts, "user1", "LOGIN", null, null, hash1),
            new("id2", ts, "user1", "LOGOUT", null, "wrong_previous_hash", hash2),
        };

        auditRepo.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<AuditEntry>>(entries));

        var result = await sut.DetectTamperedEntriesAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("id2");
    }

    [Fact]
    public async Task DetectTamperedEntriesAsync_RepoFails_ReturnsFailure()
    {
        var auditRepo = Substitute.For<IAuditRepository>();
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });
        var sut = new AuditService(auditRepo, auditOpts);

        auditRepo.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<AuditEntry>>(ErrorCode.IncidentLogFailed, "DB error"));

        var result = await sut.DetectTamperedEntriesAsync();
        result.IsFailure.Should().BeTrue();
    }

    // ── ServiceCollectionExtensions additional coverage ─────────────────────────

    [Fact]
    public void AddHnVueSecurity_ShortSecretKey_Throws()
    {
        var services = new ServiceCollection();
        var act = () => services.AddHnVueSecurity(new JwtOptions { SecretKey = "short" });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddHnVueSecurity_ShortPreviousKey_Throws()
    {
        var services = new ServiceCollection();
        var act = () => services.AddHnVueSecurity(new JwtOptions
        {
            SecretKey = new string('k', 32),
            PreviousSecretKey = "short",
        });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddHnVueSecurity_ValidOptions_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddHnVueSecurity(
            new JwtOptions { SecretKey = new string('k', 32) },
            new AuditOptions { HmacKey = new string('h', 32) });

        services.Count.Should().BeGreaterThan(5);

        using var sp = services.BuildServiceProvider();
        sp.GetRequiredService<IPhiMaskingService>().Should().NotBeNull();
        sp.GetRequiredService<IRateLimitingService>().Should().NotBeNull();
        sp.GetRequiredService<ITlsConnectionService>().Should().NotBeNull();
    }

    [Fact]
    public void AddHnVueSecurity_DefaultJwtOptions_Throws()
    {
        var services = new ServiceCollection();
        var act = () => services.AddHnVueSecurity();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddHnVueSecurity_NullAuditOptions_UsesDefault()
    {
        var services = new ServiceCollection();
        services.AddHnVueSecurity(
            new JwtOptions { SecretKey = new string('k', 32) },
            auditOptions: null);

        services.Count.Should().BeGreaterThan(5);
    }

    [Fact]
    public void AddPhiEncryption_NoKey_Throws()
    {
        var services = new ServiceCollection();
        services.AddOptions<HnVueOptions>();
        services.AddPhiEncryption();

        using var sp = services.BuildServiceProvider();
        var act = () => sp.GetRequiredService<IPhiEncryptionService>();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddPhiEncryption_ValidKey_RegistersService()
    {
        var key = Convert.ToBase64String(new byte[32]);
        var services = new ServiceCollection();
        services.AddOptions<HnVueOptions>().Configure(o => o.PhiEncryptionKey = key);
        services.AddPhiEncryption();

        using var sp = services.BuildServiceProvider();
        var svc = sp.GetRequiredService<IPhiEncryptionService>();
        svc.Should().NotBeNull();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static string ComputeTestHmac(string payload, byte[] key)
    {
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static X509Certificate2 CreateSelfSignedCert(
        DateTimeOffset notBefore,
        DateTimeOffset notAfter,
        string? subjectName = null)
    {
        subjectName ??= "localhost";
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var req = new CertificateRequest(
            $"CN={subjectName}", ecdsa, HashAlgorithmName.SHA256);
        var sanBuilder = new SubjectAlternativeNameBuilder();
        sanBuilder.AddDnsName(subjectName);
        req.CertificateExtensions.Add(sanBuilder.Build());

        // Add basic constraints for CA=false
        req.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(false, false, 0, false));

        return req.CreateSelfSigned(notBefore, notAfter);
    }
}
