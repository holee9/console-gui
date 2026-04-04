using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

/// <summary>
/// Defines data-access operations for user account records.
/// Implemented by the HnVue.Data module.
/// </summary>
public interface IUserRepository
{
    /// <summary>Retrieves a user record by login name.</summary>
    /// <param name="username">The login name to look up.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the <see cref="UserRecord"/>,
    /// or a failure with <see cref="ErrorCode.NotFound"/>.
    /// </returns>
    Task<Result<UserRecord>> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a user record by unique identifier.</summary>
    /// <param name="userId">The unique user identifier to look up.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the <see cref="UserRecord"/>,
    /// or a failure with <see cref="ErrorCode.NotFound"/>.
    /// </returns>
    Task<Result<UserRecord>> GetByIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>Updates the consecutive failed login attempt counter for a user.</summary>
    /// <param name="userId">Unique identifier of the user.</param>
    /// <param name="count">New failed login count to persist.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result> UpdateFailedLoginCountAsync(
        string userId,
        int count,
        CancellationToken cancellationToken = default);

    /// <summary>Sets or clears the account lock state for a user.</summary>
    /// <param name="userId">Unique identifier of the user.</param>
    /// <param name="isLocked"><see langword="true"/> to lock the account; <see langword="false"/> to unlock.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result> SetLockedAsync(
        string userId,
        bool isLocked,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces the stored password hash for a user.</summary>
    /// <param name="userId">Unique identifier of the user.</param>
    /// <param name="passwordHash">New bcrypt/PBKDF2 hash to store.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result> UpdatePasswordHashAsync(
        string userId,
        string passwordHash,
        CancellationToken cancellationToken = default);

    /// <summary>Returns all user records in the system.</summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result<IReadOnlyList<UserRecord>>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
