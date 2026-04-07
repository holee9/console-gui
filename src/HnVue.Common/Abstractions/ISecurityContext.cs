using HnVue.Common.Enums;
using HnVue.Common.Models;

namespace HnVue.Common.Abstractions;

/// <summary>
/// Provides access to the currently authenticated user within the application session.
/// Registered as a singleton and updated by <c>ISecurityService</c> upon login and logout.
/// </summary>
public interface ISecurityContext
{
    /// <summary>Gets the unique identifier of the currently authenticated user, or <see langword="null"/>.</summary>
    string? CurrentUserId { get; }

    /// <summary>Gets the login name of the currently authenticated user, or <see langword="null"/>.</summary>
    string? CurrentUsername { get; }

    /// <summary>Gets the role of the currently authenticated user, or <see langword="null"/>.</summary>
    UserRole? CurrentRole { get; }

    /// <summary>Gets a value indicating whether a user is currently authenticated.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Gets the JWT identifier (JTI) of the current session token, or <see langword="null"/>.</summary>
    string? CurrentJti { get; }

    /// <summary>
    /// Returns <see langword="true"/> when the current user holds exactly the specified <paramref name="role"/>.
    /// </summary>
    /// <remarks>
    /// Role matching is exact: <c>HasRole(UserRole.Radiographer)</c> returns <see langword="false"/>
    /// for an <c>Admin</c> user even though Admin shares some permissions.
    /// This is by design — each role has a distinct, non-hierarchical permission set per the SRS.
    /// Callers that need to permit multiple roles must call <c>HasRole</c> for each allowed role,
    /// or compare <see cref="CurrentRole"/> directly.
    /// </remarks>
    /// <param name="role">Role to test against the current user's role.</param>
    bool HasRole(UserRole role);

    /// <summary>Sets the authenticated user for the current session.</summary>
    /// <param name="user">The authenticated user to store in context.</param>
    void SetCurrentUser(AuthenticatedUser user);

    /// <summary>Clears the current user, effectively logging out the session.</summary>
    void ClearCurrentUser();
}
