using HnVue.Common.Enums;

namespace HnVue.Common.Models;

// @MX:NOTE UserRecord record - Account entity with bcrypt hash, lockout flags, PIN tracking fields
/// <summary>
/// Represents a user account as stored in the data layer.
/// Passwords are never stored in plain text; only the hash is persisted.
/// </summary>
/// <param name="UserId">Unique identifier of the user account.</param>
/// <param name="Username">Login name used for authentication.</param>
/// <param name="DisplayName">Human-readable full name shown in the UI.</param>
/// <param name="PasswordHash">Bcrypt or PBKDF2 hash of the user's password.</param>
/// <param name="Role">Role that governs what operations the user may perform.</param>
/// <param name="FailedLoginCount">Number of consecutive failed login attempts since last success.</param>
/// <param name="IsLocked">Whether the account is currently locked from logging in.</param>
/// <param name="LastLoginAt">UTC timestamp of the most recent successful login, or null if never.</param>
/// <param name="QuickPinHash">Bcrypt hash of the user's Quick PIN, or null if no PIN is set.</param>
/// <param name="QuickPinFailedCount">Number of consecutive Quick PIN verification failures.</param>
/// <param name="QuickPinLockedUntil">UTC timestamp when Quick PIN lockout expires, or null if not locked.</param>
public sealed record UserRecord(
    string UserId,
    string Username,
    string DisplayName,
    string PasswordHash,
    UserRole Role,
    int FailedLoginCount,
    bool IsLocked,
    DateTimeOffset? LastLoginAt,
    string? QuickPinHash = null,
    int QuickPinFailedCount = 0,
    DateTimeOffset? QuickPinLockedUntil = null);
