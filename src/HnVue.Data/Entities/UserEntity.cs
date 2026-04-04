using System.ComponentModel.DataAnnotations;

namespace HnVue.Data.Entities;

/// <summary>
/// EF Core entity that maps to the <c>Users</c> table in the encrypted SQLite database.
/// Role is stored as an integer to decouple the schema from the enum definition.
/// </summary>
public sealed class UserEntity
{
    /// <summary>Unique user identifier (primary key).</summary>
    [Key]
    [MaxLength(64)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>Login name; must be unique.</summary>
    [Required]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    /// <summary>Human-readable full name shown in the UI.</summary>
    [Required]
    [MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Bcrypt/PBKDF2 hash of the user's password.</summary>
    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary><see cref="HnVue.Common.Enums.UserRole"/> stored as integer.</summary>
    public int RoleValue { get; set; }

    /// <summary>Number of consecutive failed login attempts since last success.</summary>
    public int FailedLoginCount { get; set; }

    /// <summary>Whether the account is currently locked.</summary>
    public bool IsLocked { get; set; }

    /// <summary>UTC ticks of last successful login; null if never logged in.</summary>
    public long? LastLoginAtTicks { get; set; }

    /// <summary>UTC offset minutes for <see cref="LastLoginAtTicks"/>; null when <see cref="LastLoginAtTicks"/> is null.</summary>
    public int? LastLoginAtOffsetMinutes { get; set; }
}
