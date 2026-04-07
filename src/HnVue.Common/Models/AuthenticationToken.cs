using HnVue.Common.Enums;

namespace HnVue.Common.Models;

// @MX:NOTE AuthenticationToken record - JWT DTO with ExpiresAt and Jti for token lifecycle management
/// <summary>
/// Represents a JWT-based authentication token issued after a successful login.
/// Immutable data transfer object.
/// </summary>
/// <param name="UserId">Unique identifier of the authenticated user.</param>
/// <param name="Username">Login name of the authenticated user.</param>
/// <param name="Role">Role granted to the authenticated user.</param>
/// <param name="Token">Encoded JWT bearer token string.</param>
/// <param name="ExpiresAt">UTC timestamp at which the token expires.</param>
/// <param name="Jti">Unique JWT identifier for token revocation (SWR-CS-077).</param>
public sealed record AuthenticationToken(
    string UserId,
    string Username,
    UserRole Role,
    string Token,
    DateTimeOffset ExpiresAt,
    string Jti);
