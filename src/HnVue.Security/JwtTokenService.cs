using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HnVue.Common.Enums;
using HnVue.Common.Results;
using Microsoft.IdentityModel.Tokens;

namespace HnVue.Security;

// @MX:NOTE JwtTokenService - HS256 JWT issuer/validator, 15-min expiry default, Role claim for RBAC, JTI for revocation
/// <summary>
/// Internal helper that issues and validates JWT bearer tokens using HS256 signing.
/// </summary>
internal sealed class JwtTokenService(JwtOptions options)
{
    private readonly JwtOptions _options = options;

    /// <summary>
    /// Issues a signed JWT containing userId, username, role, and JTI claims.
    /// </summary>
    /// <param name="userId">Unique identifier of the authenticated user.</param>
    /// <param name="username">Login name of the authenticated user.</param>
    /// <param name="role">Role assigned to the user.</param>
    /// <returns>A tuple containing the encoded JWT string and its unique JTI identifier.</returns>
    public (string Token, string Jti) Issue(string userId, string username, UserRole role)
    {
        var jti = Guid.NewGuid().ToString();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, jti), // SWR-CS-077: JTI claim for revocation
        };
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
            signingCredentials: credentials);
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, jti);
    }

    /// <summary>
    /// Validates a JWT token and returns its claims principal on success.
    /// </summary>
    /// <param name="token">The encoded JWT string to validate.</param>
    /// <param name="tokenDenylist">Optional denylist to check for revoked tokens.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the <see cref="ClaimsPrincipal"/> on valid token;
    /// otherwise a failure with <see cref="ErrorCode.TokenInvalid"/>, <see cref="ErrorCode.TokenExpired"/>,
    /// or <see cref="ErrorCode.TokenRevoked"/> (SWR-CS-077).
    /// </returns>
    public async Task<Result<ClaimsPrincipal>> ValidateAsync(
        string token,
        ITokenDenylist? tokenDenylist = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);

        // Try current key first
        var result = await ValidateWithKeyAsync(token, _options.SecretKey, tokenDenylist, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
            return result;

        // Fallback to previous key if configured (key rotation support)
        if (!string.IsNullOrEmpty(_options.PreviousSecretKey) &&
            result.Error == ErrorCode.TokenInvalid)
        {
            var fallbackResult = await ValidateWithKeyAsync(token, _options.PreviousSecretKey, tokenDenylist, cancellationToken).ConfigureAwait(false);
            if (fallbackResult.IsSuccess)
                return fallbackResult;
        }

        return result;
    }

    private async Task<Result<ClaimsPrincipal>> ValidateWithKeyAsync(
        string token,
        string secretKey,
        ITokenDenylist? tokenDenylist,
        CancellationToken cancellationToken)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, validationParameters, out _);

            // SWR-CS-077: Check JTI denylist for token revocation (async, no deadlock risk)
            if (tokenDenylist != null)
            {
                var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (jti != null)
                {
                    var isRevoked = await tokenDenylist.IsRevokedAsync(jti, cancellationToken).ConfigureAwait(false);
                    if (isRevoked)
                        return Result.Failure<ClaimsPrincipal>(ErrorCode.TokenRevoked, "Token has been revoked.");
                }
            }

            return Result.Success(principal);
        }
        catch (SecurityTokenExpiredException)
        {
            return Result.Failure<ClaimsPrincipal>(ErrorCode.TokenExpired, "Token has expired.");
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            return Result.Failure<ClaimsPrincipal>(ErrorCode.TokenInvalid, "Token is invalid or tampered.");
        }
    }

    /// <summary>
    /// Synchronous validation method for backward compatibility.
    /// WARNING: This method may cause deadlocks if a denylist is provided.
    /// Prefer using <see cref="ValidateAsync"/> instead.
    /// </summary>
    /// <param name="token">The encoded JWT string to validate.</param>
    /// <param name="tokenDenylist">Optional denylist to check for revoked tokens.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the <see cref="ClaimsPrincipal"/> on valid token;
    /// otherwise a failure with an appropriate error code.
    /// </returns>
    [Obsolete("Use ValidateAsync instead to avoid potential deadlocks.")]
    public Result<ClaimsPrincipal> Validate(string token, ITokenDenylist? tokenDenylist = null)
    {
        ArgumentNullException.ThrowIfNull(token);

        // Try current key first
        var result = ValidateWithKey(token, _options.SecretKey, tokenDenylist);
        if (result.IsSuccess)
            return result;

        // Fallback to previous key if configured (key rotation support)
        if (!string.IsNullOrEmpty(_options.PreviousSecretKey) &&
            result.Error == ErrorCode.TokenInvalid)
        {
            var fallbackResult = ValidateWithKey(token, _options.PreviousSecretKey, tokenDenylist);
            if (fallbackResult.IsSuccess)
                return fallbackResult;
        }

        return result;
    }

    private Result<ClaimsPrincipal> ValidateWithKey(string token, string secretKey, ITokenDenylist? tokenDenylist)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, validationParameters, out _);

            // SWR-CS-077: Check JTI denylist for token revocation
            if (tokenDenylist != null)
            {
                var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (jti != null)
                {
                    // Synchronously block on the async check (validation path is typically synchronous)
                    var isRevoked = tokenDenylist.IsRevokedAsync(jti, CancellationToken.None).GetAwaiter().GetResult();
                    if (isRevoked)
                        return Result.Failure<ClaimsPrincipal>(ErrorCode.TokenRevoked, "Token has been revoked.");
                }
            }

            return Result.Success(principal);
        }
        catch (SecurityTokenExpiredException)
        {
            return Result.Failure<ClaimsPrincipal>(ErrorCode.TokenExpired, "Token has expired.");
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            return Result.Failure<ClaimsPrincipal>(ErrorCode.TokenInvalid, "Token is invalid or tampered.");
        }
    }
}
