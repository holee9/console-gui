using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;

namespace HnVue.UI.ViewModels;

/// <summary>
/// ViewModel for the login screen.
/// Handles user credential input and delegates authentication to <see cref="ISecurityService"/>.
/// </summary>
public sealed partial class LoginViewModel : ObservableObject
{
    private readonly ISecurityService _securityService;
    private readonly ISecurityContext _securityContext;

    /// <summary>Initialises a new instance of <see cref="LoginViewModel"/>.</summary>
    /// <param name="securityService">Service used to authenticate users.</param>
    /// <param name="securityContext">Context updated upon successful login.</param>
    public LoginViewModel(ISecurityService securityService, ISecurityContext securityContext)
    {
        _securityService = securityService;
        _securityContext = securityContext;
    }

    /// <summary>Gets or sets the username entered by the user.</summary>
    [ObservableProperty]
    private string _username = string.Empty;

    /// <summary>Gets or sets the password entered by the user.</summary>
    [ObservableProperty]
    private string _password = string.Empty;

    /// <summary>Gets or sets a value indicating whether a login attempt is in progress.</summary>
    [ObservableProperty]
    private bool _isLoggingIn;

    /// <summary>Gets or sets the error message to display on login failure.</summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>Raised when authentication succeeds.</summary>
    public event EventHandler<LoginSuccessEventArgs>? LoginSucceeded;

    /// <summary>Attempts to authenticate the user with the entered credentials.</summary>
    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        IsLoggingIn = true;
        ErrorMessage = null;

        try
        {
            var result = await _securityService.AuthenticateAsync(Username, Password);
            if (result.IsSuccess)
            {
                var authUser = new AuthenticatedUser(
                    result.Value.UserId,
                    result.Value.Username,
                    result.Value.Role);
                _securityContext.SetCurrentUser(authUser);
                LoginSucceeded?.Invoke(this, new LoginSuccessEventArgs(authUser));
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Authentication failed.";
            }
        }
        finally
        {
            IsLoggingIn = false;
        }
    }

    private bool CanLogin() =>
        !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) && !IsLoggingIn;

    partial void OnUsernameChanged(string value) => LoginCommand.NotifyCanExecuteChanged();
    partial void OnPasswordChanged(string value) => LoginCommand.NotifyCanExecuteChanged();
    partial void OnIsLoggingInChanged(bool value) => LoginCommand.NotifyCanExecuteChanged();
}
