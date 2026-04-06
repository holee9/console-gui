using HnVue.Common.Models;

namespace HnVue.UI.Contracts.Events;

/// <summary>
/// Event arguments raised when a user successfully authenticates.
/// Defined in HnVue.UI.Contracts so that both the interface (ILoginViewModel)
/// and the implementation (LoginViewModel) can reference the same type without
/// creating a circular dependency.
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
