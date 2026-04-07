using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Security;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HnVue.Security.Tests;

public sealed class SecurityServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ISecurityContext _securityContext;
    private readonly ITokenDenylist _tokenDenylist;
    private readonly SecurityService _sut;

    private static readonly JwtOptions TestJwtOptions = new()
    {
        SecretKey = "TestSecretKey-32CharMinimumForHs256!",
        ExpiryMinutes = 15,
        Issuer = "HnVue",
        Audience = "HnVue",
    };

    private static readonly IOptions<AuditOptions> TestAuditOptions =
        Options.Create(new AuditOptions { HmacKey = "TestHmacKey-32CharMinimumForAudit!" });

    public SecurityServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _auditRepository = Substitute.For<IAuditRepository>();
        _securityContext = Substitute.For<ISecurityContext>();
        _tokenDenylist = Substitute.For<ITokenDenylist>();
        _sut = new SecurityService(_userRepository, _auditRepository, _securityContext, TestJwtOptions, TestAuditOptions, _tokenDenylist);

        // Default: audit repository GetLastHashAsync returns NotFound (empty log = null previous hash).
        _auditRepository.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string?>(ErrorCode.NotFound, "log is empty"));
        _auditRepository.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
    }

    // ── AuthenticateAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task Authenticate_ValidCredentials_ReturnsToken()
    {
        const string password = "Password1";
        var user = MakeUser(password: password, isLocked: false);
        _userRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        _userRepository.UpdateFailedLoginCountAsync(user.UserId, 0, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.AuthenticateAsync(user.Username, password);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(user.UserId);
        result.Value.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Authenticate_WrongPassword_ReturnsAuthFailed()
    {
        const string correctPassword = "Correct1";
        var user = MakeUser(password: correctPassword, isLocked: false);
        _userRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        _userRepository.UpdateFailedLoginCountAsync(user.UserId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.AuthenticateAsync(user.Username, "WrongPassword1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AuthenticationFailed);
    }

    [Fact]
    public async Task Authenticate_LockedAccount_ReturnsAccountLocked()
    {
        const string password = "Password1";
        var user = MakeUser(password: password, isLocked: true);
        _userRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));

        var result = await _sut.AuthenticateAsync(user.Username, password);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AccountLocked);
    }

    [Fact]
    public async Task Authenticate_UserNotFound_ReturnsAuthFailed()
    {
        _userRepository.GetByUsernameAsync("unknown", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<UserRecord>(ErrorCode.NotFound, "not found"));

        var result = await _sut.AuthenticateAsync("unknown", "Password1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AuthenticationFailed);
    }

    [Fact]
    public async Task Authenticate_FailedLogin5Times_LocksAccount()
    {
        const string password = "Password1";
        // User already has 4 failed attempts; this attempt will be the 5th.
        var user = MakeUser(password: password, isLocked: false, failedLoginCount: 4);
        _userRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        _userRepository.UpdateFailedLoginCountAsync(user.UserId, 5, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _userRepository.SetLockedAsync(user.UserId, true, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.AuthenticateAsync(user.Username, "WrongPassword1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AccountLocked);
        await _userRepository.Received(1).SetLockedAsync(user.UserId, true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Authenticate_SuccessResetsFailedLoginCount()
    {
        const string password = "Password1";
        var user = MakeUser(password: password, isLocked: false, failedLoginCount: 3);
        _userRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        _userRepository.UpdateFailedLoginCountAsync(user.UserId, 0, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.AuthenticateAsync(user.Username, password);

        result.IsSuccess.Should().BeTrue();
        await _userRepository.Received(1).UpdateFailedLoginCountAsync(user.UserId, 0, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Authenticate_Success_SetsSecurityContext()
    {
        const string password = "Password1";
        var user = MakeUser(password: password, isLocked: false);
        _userRepository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        _userRepository.UpdateFailedLoginCountAsync(user.UserId, 0, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        await _sut.AuthenticateAsync(user.Username, password);

        _securityContext.Received(1).SetCurrentUser(Arg.Is<AuthenticatedUser>(u =>
            u.UserId == user.UserId && u.Username == user.Username));
    }

    // ── CheckAuthorizationAsync ────────────────────────────────────────────────

    [Fact]
    public async Task CheckAuthorization_CorrectRole_ReturnsSuccess()
    {
        var user = MakeUser(role: UserRole.Admin);
        _userRepository.GetByIdAsync(user.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));

        var result = await _sut.CheckAuthorizationAsync(user.UserId, UserRole.Admin);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CheckAuthorization_WrongRole_ReturnsInsufficientPermission()
    {
        var user = MakeUser(role: UserRole.Radiographer);
        _userRepository.GetByIdAsync(user.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));

        var result = await _sut.CheckAuthorizationAsync(user.UserId, UserRole.Admin);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InsufficientPermission);
    }

    [Fact]
    public async Task CheckAuthorization_UserNotFound_ReturnsNotFound()
    {
        _userRepository.GetByIdAsync("missing", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<UserRecord>(ErrorCode.NotFound, "not found"));

        var result = await _sut.CheckAuthorizationAsync("missing", UserRole.Admin);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task CheckAuthorization_HigherRoleSatisfiesLowerRequired_ReturnsSuccess()
    {
        // Admin (level 3) should satisfy Radiographer (level 1) requirement.
        var user = MakeUser(role: UserRole.Admin);
        _userRepository.GetByIdAsync(user.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));

        var result = await _sut.CheckAuthorizationAsync(user.UserId, UserRole.Radiographer);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CheckAuthorization_LowerRoleFailsHigherRequired_ReturnsInsufficientPermission()
    {
        // Radiographer (level 1) should NOT satisfy Radiologist (level 2) requirement.
        var user = MakeUser(role: UserRole.Radiographer);
        _userRepository.GetByIdAsync(user.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));

        var result = await _sut.CheckAuthorizationAsync(user.UserId, UserRole.Radiologist);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InsufficientPermission);
    }

    // ── LockAccountAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task LockAccount_Success_WritesAuditAndReturnsSuccess()
    {
        _userRepository.SetLockedAsync("user-1", true, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.LockAccountAsync("user-1");

        result.IsSuccess.Should().BeTrue();
        await _auditRepository.Received(1).AppendAsync(
            Arg.Is<AuditEntry>(e => e.Action == "ACCOUNT_LOCKED" && e.UserId == "user-1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LockAccount_RepositoryFails_ReturnsFailure()
    {
        _userRepository.SetLockedAsync("user-1", true, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DatabaseError, "db error"));

        var result = await _sut.LockAccountAsync("user-1");

        result.IsFailure.Should().BeTrue();
    }

    // ── UnlockAccountAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task UnlockAccount_Success_ResetsCountAndWritesAudit()
    {
        _userRepository.SetLockedAsync("user-1", false, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _userRepository.UpdateFailedLoginCountAsync("user-1", 0, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.UnlockAccountAsync("user-1", "admin-1");

        result.IsSuccess.Should().BeTrue();
        await _userRepository.Received(1).UpdateFailedLoginCountAsync("user-1", 0, Arg.Any<CancellationToken>());
        await _auditRepository.Received(1).AppendAsync(
            Arg.Is<AuditEntry>(e => e.Action == "ACCOUNT_UNLOCKED" && e.UserId == "admin-1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnlockAccount_RepositoryFails_ReturnsFailure()
    {
        _userRepository.SetLockedAsync("user-1", false, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DatabaseError, "db error"));

        var result = await _sut.UnlockAccountAsync("user-1", "admin-1");

        result.IsFailure.Should().BeTrue();
    }

    // ── ChangePasswordAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task ChangePassword_Valid_UpdatesHash()
    {
        const string currentPassword = "OldPass1";
        const string newPassword = "NewPass1!";
        var user = MakeUser(password: currentPassword);
        _userRepository.GetByIdAsync(user.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        _userRepository.UpdatePasswordHashAsync(user.UserId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.ChangePasswordAsync(user.UserId, currentPassword, newPassword);

        result.IsSuccess.Should().BeTrue();
        await _userRepository.Received(1).UpdatePasswordHashAsync(
            user.UserId, Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangePassword_WeakPassword_TooShort_ReturnsPolicyViolation()
    {
        const string currentPassword = "OldPass1";
        var user = MakeUser(password: currentPassword);
        _userRepository.GetByIdAsync(user.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));

        var result = await _sut.ChangePasswordAsync(user.UserId, currentPassword, "Sh1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.PasswordPolicyViolation);
    }

    [Fact]
    public async Task ChangePassword_WeakPassword_NoUppercase_ReturnsPolicyViolation()
    {
        const string currentPassword = "OldPass1";
        var user = MakeUser(password: currentPassword);
        _userRepository.GetByIdAsync(user.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));

        var result = await _sut.ChangePasswordAsync(user.UserId, currentPassword, "password1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.PasswordPolicyViolation);
    }

    [Fact]
    public async Task ChangePassword_WeakPassword_NoDigit_ReturnsPolicyViolation()
    {
        const string currentPassword = "OldPass1";
        var user = MakeUser(password: currentPassword);
        _userRepository.GetByIdAsync(user.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));

        var result = await _sut.ChangePasswordAsync(user.UserId, currentPassword, "PasswordNoDigit");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.PasswordPolicyViolation);
    }

    [Fact]
    public async Task ChangePassword_WeakPassword_NoSpecialChar_ReturnsPolicyViolation()
    {
        // SWR-NF-SC-042: special character is required. Issue #19.
        const string currentPassword = "OldPass1";
        var user = MakeUser(password: currentPassword);
        _userRepository.GetByIdAsync(user.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));

        var result = await _sut.ChangePasswordAsync(user.UserId, currentPassword, "NoSpecial1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.PasswordPolicyViolation);
    }

    [Fact]
    public async Task ChangePassword_WeakPassword_NoLowercase_ReturnsPolicyViolation()
    {
        // SWR-NF-SC-042: lowercase is required. Issue #19.
        const string currentPassword = "OldPass1";
        var user = MakeUser(password: currentPassword);
        _userRepository.GetByIdAsync(user.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));

        var result = await _sut.ChangePasswordAsync(user.UserId, currentPassword, "NOLOWER1!");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.PasswordPolicyViolation);
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_ReturnsAuthFailed()
    {
        var user = MakeUser(password: "CorrectPass1");
        _userRepository.GetByIdAsync(user.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));

        var result = await _sut.ChangePasswordAsync(user.UserId, "WrongPass1", "NewPass1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AuthenticationFailed);
    }

    [Fact]
    public async Task ChangePassword_UserNotFound_ReturnsNotFound()
    {
        _userRepository.GetByIdAsync("missing", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<UserRecord>(ErrorCode.NotFound, "not found"));

        var result = await _sut.ChangePasswordAsync("missing", "OldPass1", "NewPass1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task ChangePassword_Valid_WritesAuditEntry()
    {
        const string currentPassword = "OldPass1";
        var user = MakeUser(password: currentPassword);
        _userRepository.GetByIdAsync(user.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        _userRepository.UpdatePasswordHashAsync(user.UserId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        await _sut.ChangePasswordAsync(user.UserId, currentPassword, "NewPass1!");

        await _auditRepository.Received(1).AppendAsync(
            Arg.Is<AuditEntry>(e => e.Action == "PASSWORD_CHANGED"),
            Arg.Any<CancellationToken>());
    }

    // ── SetQuickPinAsync ─────────────────────────────────────────────────────

    [Theory]
    [InlineData("abc")]
    [InlineData("12ab")]
    [InlineData("12 34")]
    public async Task SetQuickPin_NonNumericPin_ReturnsValidationFailed(string badPin)
    {
        var result = await _sut.SetQuickPinAsync("user-1", badPin);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12")]
    [InlineData("1")]
    public async Task SetQuickPin_TooShort_ReturnsValidationFailed(string shortPin)
    {
        var result = await _sut.SetQuickPinAsync("user-1", shortPin);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("12345678")]
    public async Task SetQuickPin_TooLong_ReturnsValidationFailed(string longPin)
    {
        var result = await _sut.SetQuickPinAsync("user-1", longPin);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("12345")]
    [InlineData("123456")]
    public async Task SetQuickPin_ValidPin_StoresHashedPin(string pin)
    {
        _userRepository.SetQuickPinHashAsync("user-1", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.SetQuickPinAsync("user-1", pin);

        result.IsSuccess.Should().BeTrue();
        await _userRepository.Received(1).SetQuickPinHashAsync(
            "user-1",
            Arg.Is<string>(h => h != pin && h.StartsWith("$2")),
            Arg.Any<CancellationToken>());
    }

    // ── VerifyQuickPinAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task VerifyQuickPin_CorrectPin_ReturnsSuccess()
    {
        const string pin = "1234";
        var hash = BCrypt.Net.BCrypt.HashPassword(pin, workFactor: 4);
        var user = MakeUser(userId: "user-1") with { QuickPinHash = hash };
        _userRepository.GetByIdAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        _userRepository.UpdateQuickPinFailureAsync("user-1", Arg.Any<int>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.VerifyQuickPinAsync("user-1", pin);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyQuickPin_CorrectPin_ResetsFailureCount()
    {
        const string pin = "1234";
        var hash = BCrypt.Net.BCrypt.HashPassword(pin, workFactor: 4);
        var user = MakeUser(userId: "user-1") with { QuickPinHash = hash, QuickPinFailedCount = 2 };
        _userRepository.GetByIdAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        _userRepository.UpdateQuickPinFailureAsync("user-1", Arg.Any<int>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.VerifyQuickPinAsync("user-1", pin);

        result.IsSuccess.Should().BeTrue();
        await _userRepository.Received(1).UpdateQuickPinFailureAsync(
            "user-1", 0, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerifyQuickPin_WrongPin_IncrementsFailureCount()
    {
        const string pin = "1234";
        var hash = BCrypt.Net.BCrypt.HashPassword(pin, workFactor: 4);
        var user = MakeUser(userId: "user-1") with { QuickPinHash = hash, QuickPinFailedCount = 0 };
        _userRepository.GetByIdAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        _userRepository.UpdateQuickPinFailureAsync("user-1", Arg.Any<int>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.VerifyQuickPinAsync("user-1", "9999");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AuthenticationFailed);
        await _userRepository.Received(1).UpdateQuickPinFailureAsync(
            "user-1", 1, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerifyQuickPin_ThreeFailures_SetsLockout()
    {
        const string pin = "1234";
        var hash = BCrypt.Net.BCrypt.HashPassword(pin, workFactor: 4);
        var user = MakeUser(userId: "user-1") with { QuickPinHash = hash, QuickPinFailedCount = 2 };
        _userRepository.GetByIdAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        _userRepository.UpdateQuickPinFailureAsync("user-1", Arg.Any<int>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.VerifyQuickPinAsync("user-1", "9999");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AccountLocked);
        await _userRepository.Received(1).UpdateQuickPinFailureAsync(
            "user-1",
            3,
            Arg.Is<DateTimeOffset?>(d => d.HasValue && d.Value > DateTimeOffset.UtcNow),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerifyQuickPin_DuringLockout_ReturnsAccountLocked()
    {
        var user = MakeUser(userId: "user-1") with
        {
            QuickPinHash = BCrypt.Net.BCrypt.HashPassword("1234", workFactor: 4),
            QuickPinFailedCount = 3,
            QuickPinLockedUntil = DateTimeOffset.UtcNow.AddMinutes(4),
        };
        _userRepository.GetByIdAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));

        var result = await _sut.VerifyQuickPinAsync("user-1", "1234");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AccountLocked);
        // Should NOT call UpdateQuickPinFailureAsync when already locked.
        await _userRepository.DidNotReceive().UpdateQuickPinFailureAsync(
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerifyQuickPin_WrongPin_ReturnsAuthFailed()
    {
        const string pin = "1234";
        var hash = BCrypt.Net.BCrypt.HashPassword(pin, workFactor: 4);
        var user = MakeUser(userId: "user-1") with { QuickPinHash = hash };
        _userRepository.GetByIdAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        _userRepository.UpdateQuickPinFailureAsync("user-1", Arg.Any<int>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.VerifyQuickPinAsync("user-1", "9999");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AuthenticationFailed);
    }

    [Fact]
    public async Task VerifyQuickPin_NoPinSet_ReturnsPinNotSet()
    {
        var user = MakeUser(userId: "user-1") with { QuickPinHash = null };
        _userRepository.GetByIdAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));

        var result = await _sut.VerifyQuickPinAsync("user-1", "1234");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.PinNotSet);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static UserRecord MakeUser(
        string? userId = null,
        string? username = null,
        string password = "Password1",
        UserRole role = UserRole.Radiographer,
        bool isLocked = false,
        int failedLoginCount = 0)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 4); // low workFactor for speed
        return new UserRecord(
            UserId: userId ?? Guid.NewGuid().ToString(),
            Username: username ?? "testuser",
            DisplayName: "Test User",
            PasswordHash: hash,
            Role: role,
            FailedLoginCount: failedLoginCount,
            IsLocked: isLocked,
            LastLoginAt: null);
    }

    // ── LogoutAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task LogoutAsync_ValidUserId_WritesLogoutAuditEntry()
    {
        // Issue #37: LogoutAsync must produce a LOGOUT audit entry.
        var result = await _sut.LogoutAsync("user-1");

        result.IsSuccess.Should().BeTrue();
        await _auditRepository.Received(1).AppendAsync(
            Arg.Is<AuditEntry>(e => e.Action == "LOGOUT" && e.UserId == "user-1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogoutAsync_WithActiveSession_RevokesJtiToken()
    {
        // SWR-CS-077: JTI must be revoked on logout.
        const string testJti = "test-jti-abc123";
        _securityContext.CurrentJti.Returns(testJti);
        _securityContext.IsAuthenticated.Returns(true);

        await _sut.LogoutAsync("user-1");

        await _tokenDenylist.Received(1).RevokeAsync(
            testJti,
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogoutAsync_WithNoActiveSession_DoesNotRevokeToken()
    {
        // No JTI when not authenticated — denylist must not be called.
        _securityContext.CurrentJti.Returns((string?)null);

        await _sut.LogoutAsync("user-1");

        await _tokenDenylist.DidNotReceive().RevokeAsync(
            Arg.Any<string>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogoutAsync_EmptyUserId_ThrowsArgumentException()
    {
        // Issue #37: Guard against empty userId.
        var act = async () => await _sut.LogoutAsync(string.Empty);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task LogoutAsync_WhitespaceUserId_ThrowsArgumentException()
    {
        var act = async () => await _sut.LogoutAsync("   ");
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
