using HnVue.Common.Models;

namespace HnVue.UI.ViewModels;

/// <summary>
/// Event arguments raised when a user successfully authenticates.
/// </summary>
public sealed class LoginSuccessEventArgs : EventArgs
{
    /// <summary>Initialises a new instance of <see cref="LoginSuccessEventArgs"/>.</summary>
    /// <param name="token">The authentication token issued on successful login.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="token"/> is <see langword="null"/>.</exception>
    public LoginSuccessEventArgs(AuthenticationToken token)
    {
        ArgumentNullException.ThrowIfNull(token, nameof(token));
        Token = token;
    }

    /// <summary>Gets the authentication token issued on successful login.</summary>
    public AuthenticationToken Token { get; }
}
