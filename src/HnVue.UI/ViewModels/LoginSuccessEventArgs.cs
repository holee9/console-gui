using HnVue.Common.Models;

namespace HnVue.UI.ViewModels;

/// <summary>
/// Event arguments raised when a user successfully authenticates.
/// </summary>
public sealed class LoginSuccessEventArgs : EventArgs
{
    /// <summary>Initialises a new instance of <see cref="LoginSuccessEventArgs"/>.</summary>
    /// <param name="user">The authenticated user.</param>
    public LoginSuccessEventArgs(AuthenticatedUser user)
    {
        User = user;
    }

    /// <summary>Gets the authenticated user.</summary>
    public AuthenticatedUser User { get; }
}
