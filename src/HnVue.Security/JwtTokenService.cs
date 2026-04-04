using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HnVue.Common.Enums;
using HnVue.Common.Results;
using Microsoft.IdentityModel.Tokens;

namespace HnVue.Security;

/// <summary>
/// Internal helper that issues and validates JWT bearer tokens using HS256 signing.
/// </summary>
internal sealed class JwtTokenService(JwtOptions options)
{
    private readonly JwtOptions _options = options;

    /// <summary>
    /// Issues a signed JWT containing userId, username, and role claims.
    /// </summary>
    /// <param name="userId">Unique identifier of the authenticated user.</param>
    /// <param name="username">Login name of the authenticated user.</param>
    /// <param name="role">Role assigned to the user.</param>
    /// <returns>Encoded JWT string.</returns>
    public string Issue(string userId, string username, UserRole role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(ClaimTypes.Role, role.ToString()),
        };
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validates a JWT token and returns its claims principal on success.
    /// </summary>
    /// <param name="token">The encoded JWT string to validate.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the <see cref="ClaimsPrincipal"/> on valid token;
    /// otherwise a failure with <see cref="ErrorCode.TokenInvalid"/> or <see cref="ErrorCode.TokenExpired"/>.
    /// </returns>
    public Result<ClaimsPrincipal> Validate(string token)
    {
        ArgumentNullException.ThrowIfNull(token);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
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
