using HnVue.Common.Models;

namespace HnVue.UI.ViewModels;

/// <summary>
/// Event arguments raised when a login attempt succeeds.
/// Carries the issued <see cref="AuthenticationToken"/> for downstream consumers.
/// </summary>
public sealed class LoginSuccessEventArgs : EventArgs
{
    /// <summary>
    /// Initialises a new instance of <see cref="LoginSuccessEventArgs"/> with the given token.
    /// </summary>
    /// <param name="token">The authentication token issued on successful login.</param>
    public LoginSuccessEventArgs(AuthenticationToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        Token = token;
    }

    /// <summary>Gets the authentication token that was issued upon successful login.</summary>
    public AuthenticationToken Token { get; }
}
