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

    /// <summary>Returns <see langword="true"/> when the current user holds the specified <paramref name="role"/>.</summary>
    /// <param name="role">Role to test against the current user's role.</param>
    bool HasRole(UserRole role);

    /// <summary>Sets the authenticated user for the current session.</summary>
    /// <param name="user">The authenticated user to store in context.</param>
    void SetCurrentUser(AuthenticatedUser user);

    /// <summary>Clears the current user, effectively logging out the session.</summary>
    void ClearCurrentUser();
}
