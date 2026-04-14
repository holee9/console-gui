using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using HnVue.Common.Abstractions;
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
/// Targeted tests to close coverage gaps in HnVue.Security module.
/// Covers constructor validation, async JWT paths, DI registration,
/// RBAC edge cases, and error handling branches.
/// </summary>
[Collection("Security-Sequential")]
public sealed class CoverageGapTests
{
    // ── AuditService constructor validation ──────────────────────────────────

    [Fact]
    public void AuditService_NullRepository_ThrowsArgumentNullException()
    {
        var options = Options.Create(new AuditOptions { HmacKey = "TestKey-32CharMinimum-For-Test!!" });

        var act = () => new AuditService(null!, options);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("auditRepository");
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
        var options = Options.Create(new AuditOptions { HmacKey = "" });

        var act = () => new AuditService(repo, options);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("options");
    }

    [Fact]
    public void AuditService_WhitespaceHmacKey_ThrowsArgumentException()
    {
        var repo = Substitute.For<IAuditRepository>();
        var options = Options.Create(new AuditOptions { HmacKey = "   " });

        var act = () => new AuditService(repo, options);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("options");
    }

    // ── JwtTokenService async validation paths ───────────────────────────────

    [Fact]
    public async Task ValidateAsync_ValidToken_ReturnsSuccess()
    {
        var options = new JwtOptions
        {
            SecretKey = "TestSecretKey-32CharMinimumForHs256!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var sut = new JwtTokenService(options);
        var (token, _) = sut.Issue("user-1", "testuser", UserRole.Radiographer);

        var result = await sut.ValidateAsync(token);

        result.IsSuccess.Should().BeTrue();
        result.Value.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_InvalidToken_ReturnsTokenInvalid()
    {
        var options = new JwtOptions
        {
            SecretKey = "TestSecretKey-32CharMinimumForHs256!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var sut = new JwtTokenService(options);

        var result = await sut.ValidateAsync("not.a.valid.token");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.TokenInvalid);
    }

    [Fact]
    public async Task ValidateAsync_ExpiredToken_ReturnsTokenExpired()
    {
        var options = new JwtOptions
        {
            SecretKey = "TestSecretKey-32CharMinimumForHs256!",
            ExpiryMinutes = -1,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var sut = new JwtTokenService(options);
        var (token, _) = sut.Issue("user-1", "testuser", UserRole.Radiographer);

        // Validate with a service that uses positive expiry (so it checks lifetime)
        var validOptions = new JwtOptions
        {
            SecretKey = "TestSecretKey-32CharMinimumForHs256!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var validSut = new JwtTokenService(validOptions);

        var result = await validSut.ValidateAsync(token);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.TokenExpired);
    }

    [Fact]
    public async Task ValidateAsync_WithPreviousKey_FallsBackSuccessfully()
    {
        const string oldKey = "OldSecretKey-32CharMinimumForHs256!!";
        const string newKey = "NewSecretKey-32CharMinimumForHs256!!";

        // Issue with old key
        var oldOptions = new JwtOptions
        {
            SecretKey = oldKey,
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var oldSut = new JwtTokenService(oldOptions);
        var (token, _) = oldSut.Issue("user-1", "testuser", UserRole.Admin);

        // Validate with new key + PreviousSecretKey
        var newOptions = new JwtOptions
        {
            SecretKey = newKey,
            PreviousSecretKey = oldKey,
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var newSut = new JwtTokenService(newOptions);

        var result = await newSut.ValidateAsync(token);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithPreviousKey_UnknownKeyFails()
    {
        const string unknownKey = "UnknownKey-32CharMinimumForHs256!!!";
        const string newKey = "NewSecretKey-32CharMinimumForHs256!!";
        const string oldKey = "OldSecretKey-32CharMinimumForHs256!!";

        // Issue with unknown key
        var unknownOptions = new JwtOptions
        {
            SecretKey = unknownKey,
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var unknownSut = new JwtTokenService(unknownOptions);
        var (token, _) = unknownSut.Issue("user-1", "testuser", UserRole.Admin);

        // Validate with new key + PreviousSecretKey
        var newOptions = new JwtOptions
        {
            SecretKey = newKey,
            PreviousSecretKey = oldKey,
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var newSut = new JwtTokenService(newOptions);

        var result = await newSut.ValidateAsync(token);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.TokenInvalid);
    }

    [Fact]
    public async Task ValidateAsync_WithDenylist_RevokedToken_ReturnsTokenRevoked()
    {
        var options = new JwtOptions
        {
            SecretKey = "TestSecretKey-32CharMinimumForHs256!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var sut = new JwtTokenService(options);
        var (token, jti) = sut.Issue("user-1", "testuser", UserRole.Radiographer);

        var denylist = Substitute.For<ITokenDenylist>();
        denylist.IsRevokedAsync(jti, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var result = await sut.ValidateAsync(token, denylist);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.TokenRevoked);
    }

    [Fact]
    public async Task ValidateAsync_WithDenylist_NotRevoked_ReturnsSuccess()
    {
        var options = new JwtOptions
        {
            SecretKey = "TestSecretKey-32CharMinimumForHs256!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var sut = new JwtTokenService(options);
        var (token, jti) = sut.Issue("user-1", "testuser", UserRole.Radiographer);

        var denylist = Substitute.For<ITokenDenylist>();
        denylist.IsRevokedAsync(jti, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        var result = await sut.ValidateAsync(token, denylist);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_NullToken_ThrowsArgumentNullException()
    {
        var options = new JwtOptions
        {
            SecretKey = "TestSecretKey-32CharMinimumForHs256!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var sut = new JwtTokenService(options);

        var act = async () => await sut.ValidateAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── RbacPolicy edge cases ────────────────────────────────────────────────

    [Fact]
    public void RbacPolicy_UndefinedRole_Check_ReturnsInsufficientPermission()
    {
        // Cast to an undefined enum value to hit the default case
        var undefinedRole = (UserRole)99;

        var result = RbacPolicy.Check(undefinedRole, Permissions.ViewPatients);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InsufficientPermission);
    }

    [Fact]
    public void RbacPolicy_GetPermissions_UndefinedRole_ReturnsEmptySet()
    {
        var undefinedRole = (UserRole)99;

        var permissions = RbacPolicy.GetPermissions(undefinedRole);

        permissions.Should().BeEmpty();
    }

    [Fact]
    public void RbacPolicy_HasRoleOrHigher_UndefinedRole_ReturnsFalse()
    {
        var undefinedRole = (UserRole)99;

        var result = RbacPolicy.HasRoleOrHigher(undefinedRole, UserRole.Radiographer);

        result.Should().BeFalse("Undefined role should have hierarchy level 0.");
    }

    [Fact]
    public void RbacPolicy_Check_NullPermission_ThrowsArgumentNullException()
    {
        var act = () => RbacPolicy.Check(UserRole.Admin, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── SecurityService additional branch coverage ───────────────────────────

    [Fact]
    public async Task ChangePassword_UpdatePasswordFails_ReturnsFailure()
    {
        var userRepository = Substitute.For<IUserRepository>();
        var auditRepository = Substitute.For<IAuditRepository>();
        var securityContext = Substitute.For<ISecurityContext>();
        var tokenDenylist = Substitute.For<ITokenDenylist>();
        var jwtOptions = new JwtOptions
        {
            SecretKey = "TestSecretKey-32CharMinimumForHs256!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var auditOptions = Options.Create(new AuditOptions { HmacKey = "TestHmacKey-32CharMinimumForAudit!" });

        auditRepository.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string?>(ErrorCode.NotFound, "empty"));
        auditRepository.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        const string currentPassword = "OldPass1!";
        var hash = BCrypt.Net.BCrypt.HashPassword(currentPassword, workFactor: 4);
        var user = new UserRecord("user-1", "testuser", "Test User", hash, UserRole.Radiographer, 0, false, null);
        userRepository.GetByIdAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        userRepository.UpdatePasswordHashAsync("user-1", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DatabaseError, "db error"));

        var sut = new SecurityService(userRepository, auditRepository, securityContext, jwtOptions, auditOptions, tokenDenylist);

        var result = await sut.ChangePasswordAsync("user-1", currentPassword, "NewPass1!");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task SetQuickPin_RepositoryFails_ReturnsFailure()
    {
        var userRepository = Substitute.For<IUserRepository>();
        var auditRepository = Substitute.For<IAuditRepository>();
        var securityContext = Substitute.For<ISecurityContext>();
        var tokenDenylist = Substitute.For<ITokenDenylist>();
        var jwtOptions = new JwtOptions
        {
            SecretKey = "TestSecretKey-32CharMinimumForHs256!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var auditOptions = Options.Create(new AuditOptions { HmacKey = "TestHmacKey-32CharMinimumForAudit!" });

        auditRepository.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string?>(ErrorCode.NotFound, "empty"));
        auditRepository.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        userRepository.SetQuickPinHashAsync("user-1", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DatabaseError, "db error"));

        var sut = new SecurityService(userRepository, auditRepository, securityContext, jwtOptions, auditOptions, tokenDenylist);

        var result = await sut.SetQuickPinAsync("user-1", "1234");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task VerifyQuickPin_UserNotFound_ReturnsFailure()
    {
        var userRepository = Substitute.For<IUserRepository>();
        var auditRepository = Substitute.For<IAuditRepository>();
        var securityContext = Substitute.For<ISecurityContext>();
        var tokenDenylist = Substitute.For<ITokenDenylist>();
        var jwtOptions = new JwtOptions
        {
            SecretKey = "TestSecretKey-32CharMinimumForHs256!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var auditOptions = Options.Create(new AuditOptions { HmacKey = "TestHmacKey-32CharMinimumForAudit!" });

        auditRepository.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string?>(ErrorCode.NotFound, "empty"));

        userRepository.GetByIdAsync("missing-user", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<UserRecord>(ErrorCode.NotFound, "not found"));

        var sut = new SecurityService(userRepository, auditRepository, securityContext, jwtOptions, auditOptions, tokenDenylist);

        var result = await sut.VerifyQuickPinAsync("missing-user", "1234");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── SecurityService WriteAuditInternal - lastHash success branch ─────────

    [Fact]
    public async Task Authenticate_WhenAuditRepositoryHasExistingHash_ChainsHash()
    {
        var userRepository = Substitute.For<IUserRepository>();
        var auditRepository = Substitute.For<IAuditRepository>();
        var securityContext = Substitute.For<ISecurityContext>();
        var tokenDenylist = Substitute.For<ITokenDenylist>();
        var jwtOptions = new JwtOptions
        {
            SecretKey = "TestSecretKey-32CharMinimumForHs256!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var auditOptions = Options.Create(new AuditOptions { HmacKey = "TestHmacKey-32CharMinimumForAudit!" });

        // Return a valid existing hash (not NotFound)
        auditRepository.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success<string?>("existing-hash-abc123"));
        auditRepository.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        const string password = "Password1";
        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 4);
        var user = new UserRecord("user-1", "testuser", "Test User", hash, UserRole.Radiographer, 0, false, null);
        userRepository.GetByUsernameAsync("testuser", Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        userRepository.UpdateFailedLoginCountAsync("user-1", 0, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = new SecurityService(userRepository, auditRepository, securityContext, jwtOptions, auditOptions, tokenDenylist);

        var result = await sut.AuthenticateAsync("testuser", password);

        result.IsSuccess.Should().BeTrue();
        // Verify that the audit entry was appended with a non-null previous hash
        await auditRepository.Received(1).AppendAsync(
            Arg.Is<AuditEntry>(e => e.PreviousHash == "existing-hash-abc123"),
            Arg.Any<CancellationToken>());
    }

    // ── ServiceCollectionExtensions ──────────────────────────────────────────

    [Fact]
    public void AddHnVueSecurity_WithValidOptions_RegistersAllServices()
    {
        var services = new ServiceCollection();
        var jwtOptions = new JwtOptions
        {
            SecretKey = "TestSecretKey-32CharMinimumForHs256!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var auditOptions = new AuditOptions { HmacKey = "TestHmacKey-32CharMinimumForAudit!" };

        services.AddHnVueSecurity(jwtOptions, auditOptions);

        // Verify registrations exist
        services.Should().Contain(d => d.ServiceType == typeof(JwtOptions));
        services.Should().Contain(d => d.ServiceType == typeof(ITokenDenylist));
        services.Should().Contain(d => d.ServiceType == typeof(ISecurityService));
        services.Should().Contain(d => d.ServiceType == typeof(IAuditService));
    }

    [Fact]
    public void AddHnVueSecurity_NullSecretKey_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();
        var jwtOptions = new JwtOptions
        {
            SecretKey = null!,
            ExpiryMinutes = 15,
        };

        var act = () => services.AddHnVueSecurity(jwtOptions);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT SecretKey must be at least 32 characters*");
    }

    [Fact]
    public void AddHnVueSecurity_ShortSecretKey_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();
        var jwtOptions = new JwtOptions
        {
            SecretKey = "short",
            ExpiryMinutes = 15,
        };

        var act = () => services.AddHnVueSecurity(jwtOptions);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT SecretKey must be at least 32 characters*");
    }

    [Fact]
    public void AddHnVueSecurity_ShortPreviousSecretKey_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();
        var jwtOptions = new JwtOptions
        {
            SecretKey = "TestSecretKey-32CharMinimumForHs256!",
            PreviousSecretKey = "short",
            ExpiryMinutes = 15,
        };

        var act = () => services.AddHnVueSecurity(jwtOptions);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*PreviousSecretKey*must be at least 32 characters*");
    }

    [Fact]
    public void AddHnVueSecurity_DefaultOptions_ThrowsBecauseEmptyKey()
    {
        var services = new ServiceCollection();

        // Default JwtOptions has empty SecretKey
        var act = () => services.AddHnVueSecurity();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddHnVueSecurity_NullJwtOptions_UsesDefaultAndThrows()
    {
        var services = new ServiceCollection();

        // null jwtOptions should create default (which has empty key)
        var act = () => services.AddHnVueSecurity(null, null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddHnVueSecurity_WithValidPreviousKey_Succeeds()
    {
        var services = new ServiceCollection();
        var jwtOptions = new JwtOptions
        {
            SecretKey = "TestSecretKey-32CharMinimumForHs256!",
            PreviousSecretKey = "OldSecretKey-32CharMinimumForHs256!!",
            ExpiryMinutes = 15,
        };
        var auditOptions = new AuditOptions { HmacKey = "TestHmacKey-32CharMinimumForAudit!" };

        services.AddHnVueSecurity(jwtOptions, auditOptions);

        services.Should().Contain(d => d.ServiceType == typeof(ISecurityService));
    }

    [Fact]
    public void AddHnVueSecurity_TokenDenylistCanBeResolved()
    {
        var services = new ServiceCollection();
        var jwtOptions = new JwtOptions
        {
            SecretKey = "TestSecretKey-32CharMinimumForHs256!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var auditOptions = new AuditOptions { HmacKey = "TestHmacKey-32CharMinimumForAudit!" };

        services.AddHnVueSecurity(jwtOptions, auditOptions);

        var sp = services.BuildServiceProvider();
        var denylist = sp.GetRequiredService<ITokenDenylist>();
        denylist.Should().NotBeNull();
        denylist.Should().BeOfType<PersistentTokenDenylist>();
    }

    // ── PhiEncryptionService edge cases ──────────────────────────────────────

    [Fact]
    public void PhiEncryptionService_NullKey_ThrowsArgumentNullException()
    {
        var act = () => new PhiEncryptionService(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PhiEncryptionService_WrongKeyLength_ThrowsArgumentException()
    {
        var act = () => new PhiEncryptionService(new byte[16]);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*32 bytes*");
    }

    [Fact]
    public void PhiEncryptionService_EncryptNull_ReturnsNull()
    {
        var key = new byte[32];
        new Random(42).NextBytes(key);
        var sut = new PhiEncryptionService(key);

        var result = sut.Encrypt(null!);

        result.Should().BeNull();
    }

    [Fact]
    public void PhiEncryptionService_EncryptEmpty_ReturnsEmpty()
    {
        var key = new byte[32];
        new Random(42).NextBytes(key);
        var sut = new PhiEncryptionService(key);

        var result = sut.Encrypt(string.Empty);

        result.Should().BeEmpty();
    }

    [Fact]
    public void PhiEncryptionService_DecryptNull_ReturnsNull()
    {
        var key = new byte[32];
        new Random(42).NextBytes(key);
        var sut = new PhiEncryptionService(key);

        var result = sut.Decrypt(null!);

        result.Should().BeNull();
    }

    [Fact]
    public void PhiEncryptionService_DecryptEmpty_ReturnsEmpty()
    {
        var key = new byte[32];
        new Random(42).NextBytes(key);
        var sut = new PhiEncryptionService(key);

        var result = sut.Decrypt(string.Empty);

        result.Should().BeEmpty();
    }

    [Fact]
    public void PhiEncryptionService_DecryptTooShortCiphertext_ThrowsFormatException()
    {
        var key = new byte[32];
        new Random(42).NextBytes(key);
        var sut = new PhiEncryptionService(key);

        // Base64 of a very short byte array (less than nonce + tag = 28 bytes)
        var shortData = Convert.ToBase64String(new byte[10]);

        var act = () => sut.Decrypt(shortData);

        act.Should().Throw<FormatException>()
            .WithMessage("*Invalid ciphertext format*");
    }

    // ── JwtTokenService Issue - JTI uniqueness ───────────────────────────────

    [Fact]
    public void Issue_ReturnsUniqueJtiPerCall()
    {
        var options = new JwtOptions
        {
            SecretKey = "TestSecretKey-32CharMinimumForHs256!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var sut = new JwtTokenService(options);

        var (_, jti1) = sut.Issue("user-1", "testuser", UserRole.Radiographer);
        var (_, jti2) = sut.Issue("user-1", "testuser", UserRole.Radiographer);

        jti1.Should().NotBe(jti2, "Each JWT should have a unique JTI.");
    }

    // ── AuditService WriteAudit - AppendAsync failure with null ErrorMessage ──

    [Fact]
    public async Task WriteAudit_WhenAppendFailsWithNullMessage_ReturnsDefaultMessage()
    {
        var repo = Substitute.For<IAuditRepository>();
        var options = Options.Create(new AuditOptions { HmacKey = "TestKey-32CharMinimum-For-Test!!" });
        var sut = new AuditService(repo, options);

        repo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string?>(ErrorCode.NotFound, "empty"));
        // Return failure with null ErrorMessage
        repo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DatabaseError, null!));

        var entry = new AuditEntry(DateTimeOffset.UtcNow, "user-1", "LOGIN", currentHash: "placeholder");

        var result = await sut.WriteAuditAsync(entry);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.IncidentLogFailed);
        result.ErrorMessage.Should().Contain("Failed to append audit entry.");
    }

    // ── PersistentTokenDenylist - Dispose persists final state ────────────────

    [Fact]
    public async Task PersistentTokenDenylist_Dispose_PersistsFinalState()
    {
        var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var persistPath = System.IO.Path.Combine(tempDir, "denylist.json");

        try
        {
            var jti = Guid.NewGuid().ToString();
            using (var denylist = new PersistentTokenDenylist(TimeSpan.FromMinutes(15), persistPath))
            {
                await denylist.RevokeAsync(jti);
            }

            // After dispose, create new instance and verify persistence
            var secondDenylist = new PersistentTokenDenylist(TimeSpan.FromMinutes(15), persistPath);
            var isRevoked = await secondDenylist.IsRevokedAsync(jti, CancellationToken.None);
            isRevoked.Should().BeTrue("Token should survive after Dispose persistence.");
        }
        finally
        {
            try { System.IO.Directory.Delete(tempDir, true); } catch { /* cleanup */ }
        }
    }

    // ── SecurityService.VerifyQuickPinAsync - correct pin, 0 failed count ────

    [Fact]
    public async Task VerifyQuickPin_CorrectPin_ZeroFailedCount_DoesNotResetCounter()
    {
        var userRepository = Substitute.For<IUserRepository>();
        var auditRepository = Substitute.For<IAuditRepository>();
        var securityContext = Substitute.For<ISecurityContext>();
        var tokenDenylist = Substitute.For<ITokenDenylist>();
        var jwtOptions = new JwtOptions
        {
            SecretKey = "TestSecretKey-32CharMinimumForHs256!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var auditOptions = Options.Create(new AuditOptions { HmacKey = "TestHmacKey-32CharMinimumForAudit!" });

        auditRepository.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string?>(ErrorCode.NotFound, "empty"));
        auditRepository.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        const string pin = "1234";
        var hash = BCrypt.Net.BCrypt.HashPassword(pin, workFactor: 4);
        var user = new UserRecord("user-1", "testuser", "Test User", "dummy", UserRole.Radiographer, 0, false, null)
            with { QuickPinHash = hash, QuickPinFailedCount = 0 };

        userRepository.GetByIdAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));

        var sut = new SecurityService(userRepository, auditRepository, securityContext, jwtOptions, auditOptions, tokenDenylist);

        var result = await sut.VerifyQuickPinAsync("user-1", pin);

        result.IsSuccess.Should().BeTrue();
        // Should NOT call UpdateQuickPinFailureAsync when QuickPinFailedCount is already 0
        await userRepository.DidNotReceive().UpdateQuickPinFailureAsync(
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>());
    }
}
