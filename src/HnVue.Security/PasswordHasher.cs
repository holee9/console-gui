using HnVue.Common.Results;

namespace HnVue.Security;

/// <summary>
/// Provides password hashing and verification using bcrypt with a cost factor of 12.
/// Satisfies IEC 62304 Class B security requirements for credential storage.
/// </summary>
/// <remarks>
/// BCrypt cost factor 12 provides ~300ms hashing time on modern hardware,
/// balancing security against brute-force attacks and usability.
/// </remarks>
public sealed class PasswordHasher
{
    private const int WorkFactor = 12;

    /// <summary>
    /// Hashes a plain-text password using bcrypt with cost factor 12.
    /// </summary>
    /// <param name="plainTextPassword">The plain-text password to hash.</param>
    /// <returns>A bcrypt hash string suitable for storage.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plainTextPassword"/> is null.</exception>
    public static string HashPassword(string plainTextPassword)
    {
        ArgumentNullException.ThrowIfNull(plainTextPassword);
        return BCrypt.Net.BCrypt.HashPassword(plainTextPassword, WorkFactor);
    }

    /// <summary>
    /// Verifies a plain-text password against a stored bcrypt hash.
    /// </summary>
    /// <param name="plainTextPassword">The plain-text password to verify.</param>
    /// <param name="hash">The stored bcrypt hash to verify against.</param>
    /// <returns>
    /// A successful <see cref="Result"/> if the password matches;
    /// otherwise a failure with a generic message to avoid timing attacks.
    /// </returns>
    public static Result Verify(string plainTextPassword, string hash)
    {
        ArgumentNullException.ThrowIfNull(plainTextPassword);
        ArgumentNullException.ThrowIfNull(hash);

        try
        {
            var matches = BCrypt.Net.BCrypt.Verify(plainTextPassword, hash);
            return matches
                ? Result.Success()
                : Result.Failure(ErrorCode.AuthenticationFailed, "Password verification failed.");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            // Treat malformed hashes as failed verification to avoid leaking information.
            return Result.Failure(ErrorCode.AuthenticationFailed, "Password verification failed.");
        }
    }

    /// <summary>
    /// Checks whether an existing hash needs re-hashing due to a lower work factor.
    /// Use after successful verification to upgrade legacy hashes transparently.
    /// </summary>
    /// <param name="hash">The stored bcrypt hash to inspect.</param>
    /// <returns>True if the hash was created with a lower cost factor than current.</returns>
    public static bool NeedsRehash(string hash)
    {
        ArgumentNullException.ThrowIfNull(hash);
        try
        {
            return BCrypt.Net.BCrypt.PasswordNeedsRehash(hash, WorkFactor);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return true;
        }
    }
}
