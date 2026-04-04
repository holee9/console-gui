using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

/// <summary>
/// Defines authentication and account management operations.
/// Implemented by the HnVue.Security module.
/// </summary>
public interface ISecurityService
{
    /// <summary>
    /// Authenticates a user with the provided credentials.
    /// </summary>
    /// <param name="username">Login name of the user attempting to sign in.</param>
    /// <param name="password">Plain-text password to verify against the stored hash.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing an <see cref="AuthenticationToken"/>,
    /// or a failure with <see cref="ErrorCode.AuthenticationFailed"/> or <see cref="ErrorCode.AccountLocked"/>.
    /// </returns>
    Task<Result<AuthenticationToken>> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies that the specified user holds at least the required role.
    /// </summary>
    /// <param name="userId">Unique identifier of the user to check.</param>
    /// <param name="requiredRole">Minimum role required for the operation.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when the user is authorised;
    /// otherwise a failure with <see cref="ErrorCode.InsufficientPermission"/>.
    /// </returns>
    Task<Result> CheckAuthorizationAsync(
        string userId,
        UserRole requiredRole,
        CancellationToken cancellationToken = default);

    /// <summary>Locks the specified user account, preventing further logins.</summary>
    /// <param name="userId">Unique identifier of the account to lock.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result> LockAccountAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>Unlocks a previously locked user account.</summary>
    /// <param name="userId">Unique identifier of the account to unlock.</param>
    /// <param name="adminId">Unique identifier of the administrator performing the unlock.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result> UnlockAccountAsync(
        string userId,
        string adminId,
        CancellationToken cancellationToken = default);

    /// <summary>Changes the password for the specified user after verifying the current password.</summary>
    /// <param name="userId">Unique identifier of the user changing their password.</param>
    /// <param name="currentPassword">The user's existing plain-text password for verification.</param>
    /// <param name="newPassword">The new plain-text password to hash and store.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result"/>, or a failure with
    /// <see cref="ErrorCode.AuthenticationFailed"/> or <see cref="ErrorCode.PasswordPolicyViolation"/>.
    /// </returns>
    Task<Result> ChangePasswordAsync(
        string userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);
}
