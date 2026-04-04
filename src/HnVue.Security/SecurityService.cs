using System.Text.RegularExpressions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Security;

/// <summary>
/// Implements authentication, authorisation, and account management operations.
/// Satisfies IEC 62304 Class B requirements for safety-critical access control.
/// </summary>
public sealed class SecurityService(
    IUserRepository userRepository,
    IAuditRepository auditRepository,
    ISecurityContext securityContext,
    JwtOptions jwtOptions) : ISecurityService
{
    private const int MaxFailedAttempts = 5;

    private readonly IUserRepository _userRepository = userRepository;
    private readonly IAuditRepository _auditRepository = auditRepository;
    private readonly ISecurityContext _securityContext = securityContext;
    private readonly JwtTokenService _jwtTokenService = new(jwtOptions);
    private readonly int _expiryMinutes = jwtOptions.ExpiryMinutes;

    // Password policy: min 8 chars, at least 1 digit, at least 1 uppercase letter.
    private static readonly Regex PasswordPolicyRegex =
        new(@"^(?=.*[A-Z])(?=.*\d).{8,}$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

    /// <inheritdoc/>
    public async Task<Result<AuthenticationToken>> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var userResult = await _userRepository.GetByUsernameAsync(username, cancellationToken).ConfigureAwait(false);
        if (userResult.IsFailure)
            return Result.Failure<AuthenticationToken>(ErrorCode.AuthenticationFailed, "Invalid credentials.");

        var user = userResult.Value;

        if (user.IsLocked)
            return Result.Failure<AuthenticationToken>(ErrorCode.AccountLocked, "Account is locked.");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            var newCount = user.FailedLoginCount + 1;
            await _userRepository.UpdateFailedLoginCountAsync(user.UserId, newCount, cancellationToken).ConfigureAwait(false);

            if (newCount >= MaxFailedAttempts)
            {
                await _userRepository.SetLockedAsync(user.UserId, true, cancellationToken).ConfigureAwait(false);
                return Result.Failure<AuthenticationToken>(ErrorCode.AccountLocked, "Account locked after too many failed attempts.");
            }

            return Result.Failure<AuthenticationToken>(ErrorCode.AuthenticationFailed, "Invalid credentials.");
        }

        // Successful login: reset failed login counter.
        await _userRepository.UpdateFailedLoginCountAsync(user.UserId, 0, cancellationToken).ConfigureAwait(false);

        var tokenString = _jwtTokenService.Issue(user.UserId, user.Username, user.Role);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_expiryMinutes);
        var authToken = new AuthenticationToken(user.UserId, user.Username, user.Role, tokenString, expiresAt);

        _securityContext.SetCurrentUser(new AuthenticatedUser(user.UserId, user.Username, user.Role));

        await WriteAuditInternalAsync(user.UserId, "LOGIN", null, cancellationToken).ConfigureAwait(false);

        return Result.Success(authToken);
    }

    /// <inheritdoc/>
    public async Task<Result> CheckAuthorizationAsync(
        string userId,
        UserRole requiredRole,
        CancellationToken cancellationToken = default)
    {
        var userResult = await _userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (userResult.IsFailure)
            return Result.Failure(ErrorCode.NotFound, $"User '{userId}' not found.");

        var user = userResult.Value;
        return user.Role == requiredRole
            ? Result.Success()
            : Result.Failure(ErrorCode.InsufficientPermission, $"Role '{requiredRole}' required; user has '{user.Role}'.");
    }

    /// <inheritdoc/>
    public async Task<Result> LockAccountAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var lockResult = await _userRepository.SetLockedAsync(userId, true, cancellationToken).ConfigureAwait(false);
        if (lockResult.IsFailure)
            return lockResult;

        await WriteAuditInternalAsync(userId, "ACCOUNT_LOCKED", null, cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> UnlockAccountAsync(
        string userId,
        string adminId,
        CancellationToken cancellationToken = default)
    {
        var unlockResult = await _userRepository.SetLockedAsync(userId, false, cancellationToken).ConfigureAwait(false);
        if (unlockResult.IsFailure)
            return unlockResult;

        await _userRepository.UpdateFailedLoginCountAsync(userId, 0, cancellationToken).ConfigureAwait(false);
        await WriteAuditInternalAsync(adminId, "ACCOUNT_UNLOCKED", $"targetUserId={userId}", cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> ChangePasswordAsync(
        string userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var userResult = await _userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (userResult.IsFailure)
            return Result.Failure(ErrorCode.NotFound, $"User '{userId}' not found.");

        var user = userResult.Value;

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            return Result.Failure(ErrorCode.AuthenticationFailed, "Current password is incorrect.");

        if (!PasswordPolicyRegex.IsMatch(newPassword))
            return Result.Failure(ErrorCode.PasswordPolicyViolation,
                "Password must be at least 8 characters and contain at least one uppercase letter and one digit.");

        var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);
        var updateResult = await _userRepository.UpdatePasswordHashAsync(userId, newHash, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
            return updateResult;

        await WriteAuditInternalAsync(userId, "PASSWORD_CHANGED", null, cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    // ── Internal helpers ──────────────────────────────────────────────────────────

    private async Task WriteAuditInternalAsync(
        string userId,
        string action,
        string? details,
        CancellationToken cancellationToken)
    {
        var lastHashResult = await _auditRepository.GetLastHashAsync(cancellationToken).ConfigureAwait(false);
        // Ignore failures here — a missing or unavailable last hash means null (first entry or non-critical).
        var previousHash = (lastHashResult.IsSuccess) ? lastHashResult.Value : null;

        var timestamp = DateTimeOffset.UtcNow;
        var entryId = Guid.NewGuid().ToString();
        var payload = $"{entryId}|{timestamp:O}|{userId}|{action}|{details}|{previousHash}";
        var currentHash = AuditService.ComputeHmacInternal(payload, AuditService.DefaultHmacKey);

        var entry = new AuditEntry(entryId, timestamp, userId, action, details, previousHash, currentHash);
        await _auditRepository.AppendAsync(entry, cancellationToken).ConfigureAwait(false);
    }
}
