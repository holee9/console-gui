using HnVue.Common.Enums;

namespace HnVue.Common.Models;

/// <summary>
/// Represents a JWT-based authentication token issued after a successful login.
/// Immutable data transfer object.
/// </summary>
/// <param name="UserId">Unique identifier of the authenticated user.</param>
/// <param name="Username">Login name of the authenticated user.</param>
/// <param name="Role">Role granted to the authenticated user.</param>
/// <param name="Token">Encoded JWT bearer token string.</param>
/// <param name="ExpiresAt">UTC timestamp at which the token expires.</param>
public sealed record AuthenticationToken(
    string UserId,
    string Username,
    UserRole Role,
    string Token,
    DateTimeOffset ExpiresAt);
