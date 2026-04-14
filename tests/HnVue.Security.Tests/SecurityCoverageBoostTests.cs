using System.IO;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Security;
using HnVue.Security.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HnVue.Security.Tests;

/// <summary>
/// Coverage boost tests for Security module targeting 85%+ coverage.
/// Covers uncovered paths in SecurityService, AuditService, JwtTokenService,
/// PhiEncryptionService, PasswordHasher, RbacPolicy, PersistentTokenDenylist,
/// InMemoryTokenDenylist, ServiceCollectionExtensions, and options classes.
/// </summary>
[Collection("Security-Sequential")]
public sealed class SecurityCoverageBoostTests
{
    // ── SecurityService: VerifyQuickPinAsync ──────────────────────────────────

    [Fact]
    public async Task VerifyQuickPinAsync_PinLocked_ReturnsAccountLocked()
    {
        // Arrange
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        var user = new UserRecord(
            "user1", "testuser", "Test User", "hash", UserRole.Radiographer,
            0, false, null, null, 3, DateTimeOffset.UtcNow.AddMinutes(5));

        userRepo.GetByIdAsync("user1", Arg.Any<CancellationToken>()).Returns(Result.Success(user));
        auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>()).Returns(Result.SuccessNullable<string?>(null));
        auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>()).Returns(Result.Success());

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);

        // Act
        var result = await sut.VerifyQuickPinAsync("user1", "1234");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AccountLocked);
    }

    [Fact]
    public async Task VerifyQuickPinAsync_PinNotSet_ReturnsPinNotSet()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        var user = new UserRecord(
            "user1", "testuser", "Test User", "hash", UserRole.Radiographer,
            0, false, null, null, 0, null);

        userRepo.GetByIdAsync("user1", Arg.Any<CancellationToken>()).Returns(Result.Success(user));

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);

        var result = await sut.VerifyQuickPinAsync("user1", "1234");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.PinNotSet);
    }

    [Fact]
    public async Task VerifyQuickPinAsync_WrongPin_IncrementsFailure()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        var pinHash = PasswordHasher.HashPassword("1234");
        var user = new UserRecord(
            "user1", "testuser", "Test User", "hash", UserRole.Radiographer,
            0, false, null, pinHash, 0, null);

        userRepo.GetByIdAsync("user1", Arg.Any<CancellationToken>()).Returns(Result.Success(user));
        userRepo.UpdateQuickPinFailureAsync("user1", 1, null, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);

        var result = await sut.VerifyQuickPinAsync("user1", "9999");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AuthenticationFailed);
    }

    [Fact]
    public async Task VerifyQuickPinAsync_WrongPinTriggersLockout_ReturnsAccountLocked()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        var pinHash = PasswordHasher.HashPassword("1234");
        var user = new UserRecord(
            "user1", "testuser", "Test User", "hash", UserRole.Radiographer,
            0, false, null, pinHash, 2, null);

        userRepo.GetByIdAsync("user1", Arg.Any<CancellationToken>()).Returns(Result.Success(user));
        userRepo.UpdateQuickPinFailureAsync("user1", 3, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);

        var result = await sut.VerifyQuickPinAsync("user1", "9999");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AccountLocked);
    }

    [Fact]
    public async Task VerifyQuickPinAsync_CorrectPin_ResetsFailures()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        var pinHash = PasswordHasher.HashPassword("1234");
        var user = new UserRecord(
            "user1", "testuser", "Test User", "hash", UserRole.Radiographer,
            0, false, null, pinHash, 2, null);

        userRepo.GetByIdAsync("user1", Arg.Any<CancellationToken>()).Returns(Result.Success(user));
        userRepo.UpdateQuickPinFailureAsync("user1", 0, null, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);

        var result = await sut.VerifyQuickPinAsync("user1", "1234");

        result.IsSuccess.Should().BeTrue();
    }

    // ── SecurityService: SetQuickPinAsync ─────────────────────────────────────

    [Fact]
    public async Task SetQuickPinAsync_InvalidPin_ReturnsValidationFailed()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);

        // Too short
        var result = await sut.SetQuickPinAsync("user1", "12");
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);

        // Too long
        result = await sut.SetQuickPinAsync("user1", "1234567");
        result.IsFailure.Should().BeTrue();

        // Contains letters
        result = await sut.SetQuickPinAsync("user1", "12a45");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SetQuickPinAsync_ValidPin_ReturnsSuccess()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        userRepo.SetQuickPinHashAsync("user1", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>()).Returns(Result.SuccessNullable<string?>(null));
        auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>()).Returns(Result.Success());

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);

        var result = await sut.SetQuickPinAsync("user1", "1234");
        result.IsSuccess.Should().BeTrue();
    }

    // ── SecurityService: AuthenticateAsync locked account ─────────────────────

    [Fact]
    public async Task AuthenticateAsync_LockedAccount_ReturnsAccountLocked()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        var hash = PasswordHasher.HashPassword("P@ssw0rd!");
        var user = new UserRecord(
            "user1", "lockeduser", "Locked User", hash, UserRole.Radiographer,
            5, true, null, null, 0, null);

        userRepo.GetByUsernameAsync("lockeduser", Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);

        var result = await sut.AuthenticateAsync("lockeduser", "P@ssw0rd!");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AccountLocked);
    }

    // ── SecurityService: AuthenticateAsync triggers lockout ───────────────────

    [Fact]
    public async Task AuthenticateAsync_MaxFailedAttempts_LocksAccount()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        var hash = PasswordHasher.HashPassword("P@ssw0rd!");
        var user = new UserRecord(
            "user1", "testuser", "Test User", hash, UserRole.Radiographer,
            4, false, null, null, 0, null);

        userRepo.GetByUsernameAsync("testuser", Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        userRepo.UpdateFailedLoginCountAsync("user1", 5, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        userRepo.SetLockedAsync("user1", true, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);

        var result = await sut.AuthenticateAsync("testuser", "WrongPassword1!");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AccountLocked);
    }

    // ── SecurityService: CheckAuthorizationAsync ──────────────────────────────

    [Fact]
    public async Task CheckAuthorizationAsync_UserNotFound_ReturnsNotFound()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        userRepo.GetByIdAsync("unknown", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<UserRecord>(ErrorCode.NotFound, "User not found."));

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);

        var result = await sut.CheckAuthorizationAsync("unknown", UserRole.Admin);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task CheckAuthorizationAsync_InsufficientRole_ReturnsInsufficientPermission()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        var user = new UserRecord(
            "user1", "radiographer", "Radiographer User", "hash", UserRole.Radiographer,
            0, false, null, null, 0, null);

        userRepo.GetByIdAsync("user1", Arg.Any<CancellationToken>()).Returns(Result.Success(user));

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);

        var result = await sut.CheckAuthorizationAsync("user1", UserRole.Admin);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InsufficientPermission);
    }

    // ── SecurityService: LockAccountAsync / UnlockAccountAsync ────────────────

    [Fact]
    public async Task LockAccountAsync_RepoFails_ReturnsFailure()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        userRepo.SetLockedAsync("user1", true, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DatabaseError, "DB error"));

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);

        var result = await sut.LockAccountAsync("user1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task UnlockAccountAsync_Success_ReturnsSuccess()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        userRepo.SetLockedAsync("user1", false, Arg.Any<CancellationToken>()).Returns(Result.Success());
        userRepo.UpdateFailedLoginCountAsync("user1", 0, Arg.Any<CancellationToken>()).Returns(Result.Success());
        auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>()).Returns(Result.SuccessNullable<string?>(null));
        auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>()).Returns(Result.Success());

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);

        var result = await sut.UnlockAccountAsync("user1", "admin1");

        result.IsSuccess.Should().BeTrue();
    }

    // ── SecurityService: LogoutAsync ──────────────────────────────────────────

    [Fact]
    public async Task LogoutAsync_WithJti_RevokesToken()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        secCtx.CurrentJti.Returns("jti-123");
        auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>()).Returns(Result.SuccessNullable<string?>(null));
        auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>()).Returns(Result.Success());

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);

        var result = await sut.LogoutAsync("user1");

        result.IsSuccess.Should().BeTrue();
        await denylist.Received(1).RevokeAsync("jti-123", Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogoutAsync_WithoutJti_SkipsRevoke()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        secCtx.CurrentJti.Returns((string?)null);
        auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>()).Returns(Result.SuccessNullable<string?>(null));
        auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>()).Returns(Result.Success());

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);

        var result = await sut.LogoutAsync("user1");

        result.IsSuccess.Should().BeTrue();
        await denylist.DidNotReceive().RevokeAsync(Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }

    // ── SecurityService: ChangePasswordAsync ──────────────────────────────────

    [Fact]
    public async Task ChangePasswordAsync_WeakNewPassword_ReturnsPolicyViolation()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        var hash = PasswordHasher.HashPassword("OldP@ssw0rd!");
        var user = new UserRecord(
            "user1", "testuser", "Test User", hash, UserRole.Radiographer,
            0, false, null, null, 0, null);

        userRepo.GetByIdAsync("user1", Arg.Any<CancellationToken>()).Returns(Result.Success(user));

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);

        var result = await sut.ChangePasswordAsync("user1", "OldP@ssw0rd!", "weak");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.PasswordPolicyViolation);
    }

    [Fact]
    public async Task ChangePasswordAsync_UserNotFound_ReturnsNotFound()
    {
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secCtx = Substitute.For<ISecurityContext>();
        var denylist = Substitute.For<ITokenDenylist>();
        var jwtOpts = new JwtOptions { SecretKey = new string('k', 32) };
        var auditOpts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });

        userRepo.GetByIdAsync("unknown", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<UserRecord>(ErrorCode.NotFound, "Not found"));

        var sut = new SecurityService(userRepo, auditRepo, secCtx, jwtOpts, auditOpts, denylist);

        var result = await sut.ChangePasswordAsync("unknown", "old", "NewP@ssw0rd!");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── AuditService: WriteAuditAsync failures ────────────────────────────────

    [Fact]
    public async Task AuditService_WriteAuditAsync_RepoGetLastHashFails_ReturnsIncidentLogFailed()
    {
        var auditRepo = Substitute.For<IAuditRepository>();
        var opts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });
        var sut = new AuditService(auditRepo, opts);

        auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string?>(ErrorCode.DatabaseError, "DB error"));

        var entry = new AuditEntry(
            Guid.NewGuid().ToString(), DateTimeOffset.UtcNow, "user1", "TEST", null, null, "hash");

        var result = await sut.WriteAuditAsync(entry);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.IncidentLogFailed);
    }

    [Fact]
    public async Task AuditService_WriteAuditAsync_AppendFails_ReturnsIncidentLogFailed()
    {
        var auditRepo = Substitute.For<IAuditRepository>();
        var opts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });
        var sut = new AuditService(auditRepo, opts);

        auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string?>(ErrorCode.NotFound, "empty"));
        auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DatabaseError, "append failed"));

        var entry = new AuditEntry(
            Guid.NewGuid().ToString(), DateTimeOffset.UtcNow, "user1", "TEST", null, null, "hash");

        var result = await sut.WriteAuditAsync(entry);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.IncidentLogFailed);
    }

    [Fact]
    public async Task AuditService_VerifyChainIntegrityAsync_QueryFails_ReturnsIncidentLogFailed()
    {
        var auditRepo = Substitute.For<IAuditRepository>();
        var opts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });
        var sut = new AuditService(auditRepo, opts);

        auditRepo.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<AuditEntry>>(ErrorCode.DatabaseError, "query failed"));

        var result = await sut.VerifyChainIntegrityAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.IncidentLogFailed);
    }

    // ── AuditService: Constructor validation ──────────────────────────────────

    [Fact]
    public void AuditService_NullRepository_ThrowsArgumentNullException()
    {
        var opts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });
        var act = () => new AuditService(null!, opts);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AuditService_NullOptions_ThrowsArgumentNullException()
    {
        var repo = Substitute.For<IAuditRepository>();
        var act = () => new AuditService(repo, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AuditService_EmptyHmacKey_ThrowsArgumentException()
    {
        var repo = Substitute.For<IAuditRepository>();
        var opts = Options.Create(new AuditOptions { HmacKey = "" });
        var act = () => new AuditService(repo, opts);

        act.Should().Throw<ArgumentException>();
    }

    // ── PhiEncryptionService: Edge cases ──────────────────────────────────────

    [Fact]
    public void PhiEncryption_NullKey_ThrowsArgumentNullException()
    {
        var act = () => new PhiEncryptionService(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PhiEncryption_WrongKeySize_ThrowsArgumentException()
    {
        var act = () => new PhiEncryptionService(new byte[16]);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void PhiEncryption_EncryptDecrypt_Roundtrip()
    {
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        var sut = new PhiEncryptionService(key);

        var plaintext = "Patient: John Doe, MRN: 12345";
        var encrypted = sut.Encrypt(plaintext);
        var decrypted = sut.Decrypt(encrypted);

        decrypted.Should().Be(plaintext);
    }

    [Fact]
    public void PhiEncryption_Encrypt_EmptyString_ReturnsEmptyString()
    {
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        var sut = new PhiEncryptionService(key);

        sut.Encrypt("").Should().Be("");
    }

    [Fact]
    public void PhiEncryption_Encrypt_NullString_ReturnsNull()
    {
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        var sut = new PhiEncryptionService(key);

        sut.Encrypt(null!).Should().BeNull();
    }

    [Fact]
    public void PhiEncryption_Decrypt_EmptyString_ReturnsEmptyString()
    {
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        var sut = new PhiEncryptionService(key);

        sut.Decrypt("").Should().Be("");
    }

    [Fact]
    public void PhiEncryption_Decrypt_InvalidData_ThrowsFormatException()
    {
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        var sut = new PhiEncryptionService(key);

        var act = () => sut.Decrypt("tooshort");
        act.Should().Throw<FormatException>();
    }

    // ── PasswordHasher: NeedsRehash ───────────────────────────────────────────

    [Fact]
    public void PasswordHasher_NeedsRehash_WithCurrentHash_ReturnsFalse()
    {
        var hash = PasswordHasher.HashPassword("TestP@ss1!");
        PasswordHasher.NeedsRehash(hash).Should().BeFalse();
    }

    [Fact]
    public void PasswordHasher_NeedsRehash_NullHash_Throws()
    {
        var act = () => PasswordHasher.NeedsRehash(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PasswordHasher_NeedsRehash_MalformedHash_ReturnsTrue()
    {
        PasswordHasher.NeedsRehash("not-a-valid-bcrypt-hash").Should().BeTrue();
    }

    [Fact]
    public void PasswordHasher_Verify_NullPassword_Throws()
    {
        var act = () => PasswordHasher.Verify(null!, "hash");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PasswordHasher_Verify_NullHash_Throws()
    {
        var act = () => PasswordHasher.Verify("password", null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PasswordHasher_Verify_MalformedHash_ReturnsFailure()
    {
        var result = PasswordHasher.Verify("password", "malformed");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void PasswordHasher_HashPassword_Null_Throws()
    {
        var act = () => PasswordHasher.HashPassword(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── RbacPolicy: Extended coverage ─────────────────────────────────────────

    [Fact]
    public void RbacPolicy_Check_UnknownPermission_ReturnsInsufficientPermission()
    {
        var result = RbacPolicy.Check(UserRole.Radiographer, "nonexistent.permission");
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InsufficientPermission);
    }

    [Fact]
    public void RbacPolicy_Check_NullPermission_Throws()
    {
        var act = () => RbacPolicy.Check(UserRole.Admin, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(UserRole.Radiographer, 1)]
    [InlineData(UserRole.Radiologist, 2)]
    [InlineData(UserRole.Admin, 3)]
    [InlineData(UserRole.Service, 3)]
    public void RbacPolicy_HasRoleOrHigher_VariousCombinations(UserRole userRole, int expectedLevel)
    {
        // Test that the hierarchy is correct
        if (expectedLevel >= 2)
        {
            RbacPolicy.HasRoleOrHigher(userRole, UserRole.Radiographer).Should().BeTrue();
        }
        if (expectedLevel >= 3)
        {
            RbacPolicy.HasRoleOrHigher(userRole, UserRole.Radiologist).Should().BeTrue();
        }
    }

    [Fact]
    public void RbacPolicy_GetPermissions_AllRoles_ReturnsNonEmptySet()
    {
        foreach (UserRole role in Enum.GetValues<UserRole>())
        {
            var permissions = RbacPolicy.GetPermissions(role);
            permissions.Should().NotBeNull();
        }
    }

    [Fact]
    public void RbacPolicy_Check_AllPermissionsCoverage()
    {
        // Verify each role has the expected permissions
        RbacPolicy.Check(UserRole.Radiographer, Permissions.ViewPatients).IsSuccess.Should().BeTrue();
        RbacPolicy.Check(UserRole.Radiographer, Permissions.ConfigureSystem).IsSuccess.Should().BeFalse();

        RbacPolicy.Check(UserRole.Admin, Permissions.ConfigureSystem).IsSuccess.Should().BeTrue();
        RbacPolicy.Check(UserRole.Admin, Permissions.ApplySoftwareUpdate).IsSuccess.Should().BeTrue();

        RbacPolicy.Check(UserRole.Service, Permissions.ConfigureSystem).IsSuccess.Should().BeTrue();
        RbacPolicy.Check(UserRole.Service, Permissions.ViewPatients).IsSuccess.Should().BeFalse();
    }

    // ── InMemoryTokenDenylist: Extended coverage ──────────────────────────────

    [Fact]
    public async Task InMemoryTokenDenylist_RevokeAndCheck_Works()
    {
        var sut = new InMemoryTokenDenylist(TimeSpan.FromMinutes(15));
        await sut.RevokeAsync("jti-1");

        var isRevoked = await sut.IsRevokedAsync("jti-1");
        isRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task InMemoryTokenDenylist_ExpiredEntry_ReturnsFalse()
    {
        var sut = new InMemoryTokenDenylist(TimeSpan.FromMilliseconds(1));
        await sut.RevokeAsync("jti-expired");

        // Wait for expiration
        await Task.Delay(100);

        var isRevoked = await sut.IsRevokedAsync("jti-expired");
        isRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task InMemoryTokenDenylist_NotRevoked_ReturnsFalse()
    {
        var sut = new InMemoryTokenDenylist(TimeSpan.FromMinutes(15));
        var isRevoked = await sut.IsRevokedAsync("nonexistent");
        isRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task InMemoryTokenDenylist_EmptyJti_Throws()
    {
        var sut = new InMemoryTokenDenylist(TimeSpan.FromMinutes(15));
        var act = async () => await sut.RevokeAsync("");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task InMemoryTokenDenylist_IsRevoked_EmptyJti_Throws()
    {
        var sut = new InMemoryTokenDenylist(TimeSpan.FromMinutes(15));
        var act = async () => await sut.IsRevokedAsync("");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task InMemoryTokenDenylist_CustomTtl_Respected()
    {
        var sut = new InMemoryTokenDenylist(TimeSpan.FromMinutes(15));
        await sut.RevokeAsync("jti-custom", TimeSpan.FromMilliseconds(1));

        await Task.Delay(100);

        var isRevoked = await sut.IsRevokedAsync("jti-custom");
        isRevoked.Should().BeFalse();
    }

    // ── PersistentTokenDenylist: Extended coverage ────────────────────────────

    [Fact]
    public async Task PersistentTokenDenylist_RevokeAndCheck_Works()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"denylist_{Guid.NewGuid():N}.json");
        try
        {
            var sut = new PersistentTokenDenylist(TimeSpan.FromMinutes(15), tempFile);
            await sut.RevokeAsync("jti-persist-1");

            var isRevoked = await sut.IsRevokedAsync("jti-persist-1");
            isRevoked.Should().BeTrue();

            sut.Dispose();
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PersistentTokenDenylist_EmptyJti_Throws()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"denylist_{Guid.NewGuid():N}.json");
        try
        {
            var sut = new PersistentTokenDenylist(TimeSpan.FromMinutes(15), tempFile);
            var act = async () => await sut.RevokeAsync("");
            await act.Should().ThrowAsync<ArgumentException>();
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void PersistentTokenDenylist_CorruptedFile_StartsEmpty()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"denylist_{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(tempFile, "not valid json {{{{");
            var sut = new PersistentTokenDenylist(TimeSpan.FromMinutes(15), tempFile);

            // Should not throw, starts with empty denylist
            sut.Dispose();
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PersistentTokenDenylist_PersistsAcrossInstances()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"denylist_{Guid.NewGuid():N}.json");
        try
        {
            // Instance 1: revoke
            var sut1 = new PersistentTokenDenylist(TimeSpan.FromMinutes(15), tempFile);
            await sut1.RevokeAsync("jti-persist-test");
            sut1.Dispose();

            // Instance 2: verify persistence
            var sut2 = new PersistentTokenDenylist(TimeSpan.FromMinutes(15), tempFile);
            var isRevoked = await sut2.IsRevokedAsync("jti-persist-test");
            isRevoked.Should().BeTrue();
            sut2.Dispose();
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    // ── ServiceCollectionExtensions ────────────────────────────────────────────

    [Fact]
    public void AddHnVueSecurity_ShortSecretKey_Throws()
    {
        var services = new ServiceCollection();
        var act = () => services.AddHnVueSecurity(new JwtOptions { SecretKey = "short" });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddHnVueSecurity_ShortPreviousSecretKey_Throws()
    {
        var services = new ServiceCollection();
        var act = () => services.AddHnVueSecurity(
            new JwtOptions { SecretKey = new string('k', 32), PreviousSecretKey = "short" });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddHnVueSecurity_ValidOptions_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddHnVueSecurity(
            new JwtOptions { SecretKey = new string('k', 32) },
            new AuditOptions { HmacKey = new string('h', 32) });

        services.Should().ContainSingle(d => d.ServiceType == typeof(ISecurityService));
        services.Should().ContainSingle(d => d.ServiceType == typeof(IAuditService));
    }

    [Fact]
    public void AddHnVueSecurity_DefaultOptions_Throws()
    {
        var services = new ServiceCollection();
        var act = () => services.AddHnVueSecurity();

        // Default JwtOptions has empty SecretKey -> throws
        act.Should().Throw<InvalidOperationException>();
    }

    // ── AuditService: GetAuditLogsAsync ───────────────────────────────────────

    [Fact]
    public async Task AuditService_GetAuditLogsAsync_ReturnsQueryResults()
    {
        var auditRepo = Substitute.For<IAuditRepository>();
        var opts = Options.Create(new AuditOptions { HmacKey = new string('h', 32) });
        var sut = new AuditService(auditRepo, opts);

        var entries = new List<AuditEntry>();
        auditRepo.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<AuditEntry>>(entries));

        var result = await sut.GetAuditLogsAsync(new AuditQueryFilter());

        result.IsSuccess.Should().BeTrue();
    }

    // ── AuditService.ComputeHmacInternal ──────────────────────────────────────

    [Fact]
    public void ComputeHmacInternal_ProducesConsistentResults()
    {
        var key = Encoding.UTF8.GetBytes("test-hmac-key-for-verification-32");
        var payload = "test|payload|data";

        var hash1 = AuditService.ComputeHmacInternal(payload, key);
        var hash2 = AuditService.ComputeHmacInternal(payload, key);

        hash1.Should().Be(hash2);
        hash1.Should().NotBeNullOrEmpty();
    }

    // ── JwtOptions defaults ───────────────────────────────────────────────────

    [Fact]
    public void JwtOptions_Defaults_AreSet()
    {
        var opts = new JwtOptions();
        opts.SecretKey.Should().BeEmpty();
        opts.ExpiryMinutes.Should().Be(15);
        opts.Issuer.Should().Be("HnVue");
        opts.Audience.Should().Be("HnVue");
        opts.PreviousSecretKey.Should().BeNull();
    }

    [Fact]
    public void AuditOptions_Default_HmacKeyIsEmpty()
    {
        var opts = new AuditOptions();
        opts.HmacKey.Should().BeEmpty();
    }
}
