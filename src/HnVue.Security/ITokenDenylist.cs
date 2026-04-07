namespace HnVue.Security;

/// <summary>
/// Provides token revocation checking for JWT JTI denylist.
/// SWR-CS-077: Concurrent session handling and token revocation.
/// </summary>
public interface ITokenDenylist
{
    /// <summary>Revokes a token by its JTI claim.</summary>
    /// <param name="jti">Unique JWT identifier to revoke.</param>
    /// <param name="ttl">Optional time-to-live for revocation entry. Default matches token expiry.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task RevokeAsync(string jti, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    /// <summary>Checks if a token JTI has been revoked.</summary>
    /// <param name="jti">Unique JWT identifier to check.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>True if the token has been revoked; otherwise false.</returns>
    Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken = default);
}
