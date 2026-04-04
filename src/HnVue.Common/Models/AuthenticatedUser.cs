using HnVue.Common.Enums;

namespace HnVue.Common.Models;

/// <summary>
/// Lightweight representation of the currently authenticated user held in
/// <c>ISecurityContext</c> for the duration of the session.
/// </summary>
/// <param name="UserId">Unique identifier of the user.</param>
/// <param name="Username">Login name of the user.</param>
/// <param name="Role">Role that governs access permissions.</param>
public sealed record AuthenticatedUser(
    string UserId,
    string Username,
    UserRole Role);
