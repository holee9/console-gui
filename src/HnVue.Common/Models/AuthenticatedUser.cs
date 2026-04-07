using HnVue.Common.Enums;

namespace HnVue.Common.Models;

// @MX:NOTE AuthenticatedUser record - Security context primitive held in ISecurityContext for auth decisions
/// <summary>
/// Lightweight representation of the currently authenticated user held in
/// <c>ISecurityContext</c> for the duration of the session.
/// </summary>
/// <param name="UserId">Unique identifier of the user.</param>
/// <param name="Username">Login name of the user.</param>
/// <param name="Role">Role that governs access permissions.</param>
/// <param name="Jti">Unique JWT identifier for token revocation (SWR-CS-077).</param>
public sealed record AuthenticatedUser(
    string UserId,
    string Username,
    UserRole Role,
    string? Jti = null);
