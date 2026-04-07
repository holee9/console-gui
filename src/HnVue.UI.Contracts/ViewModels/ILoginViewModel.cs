using System.Collections.Generic;
using System.Windows.Input;
using HnVue.UI.Contracts.Events;

namespace HnVue.UI.Contracts.ViewModels;

/// <summary>Contract for the login screen ViewModel.</summary>
public interface ILoginViewModel : IViewModelBase
{
    /// <summary>Gets or sets the username input value.</summary>
    string Username { get; set; }

    /// <summary>Gets the list of registered user IDs available for login selection.</summary>
    IReadOnlyList<string> AvailableUserIds { get; }

    /// <summary>Gets or sets the password input value.</summary>
    string Password { get; set; }

    /// <summary>Gets the command that initiates the login flow.</summary>
    ICommand LoginCommand { get; }

    /// <summary>Raised when authentication completes successfully.</summary>
    event EventHandler<LoginSuccessEventArgs>? LoginSucceeded;
}
