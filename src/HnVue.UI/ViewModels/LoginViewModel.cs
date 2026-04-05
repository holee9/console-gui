using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;

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
        ArgumentNullException.ThrowIfNull(securityService);
        ArgumentNullException.ThrowIfNull(securityContext);
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
    private bool _isLoading;

    /// <summary>Gets or sets the error message to display on login failure.</summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>Raised when authentication succeeds.</summary>
    public event EventHandler<LoginSuccessEventArgs>? LoginSucceeded;

    /// <summary>Attempts to authenticate the user with the entered credentials.</summary>
    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var result = await _securityService.AuthenticateAsync(Username, Password);
            if (result.IsSuccess)
            {
                var token = result.Value;
                var authUser = new AuthenticatedUser(
                    token.UserId,
                    token.Username,
                    token.Role);
                _securityContext.SetCurrentUser(authUser);
                LoginSucceeded?.Invoke(this, new LoginSuccessEventArgs(token));
            }
            else
            {
                ErrorMessage = result.Error switch
                {
                    Common.Results.ErrorCode.AccountLocked => "계정이 잠겼습니다. 관리자에게 문의하세요.",
                    _ => "사용자명 또는 비밀번호가 올바르지 않습니다."
                };
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanLogin() =>
        !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) && !IsLoading;

    partial void OnUsernameChanged(string value) => LoginCommand.NotifyCanExecuteChanged();
    partial void OnPasswordChanged(string value) => LoginCommand.NotifyCanExecuteChanged();
    partial void OnIsLoadingChanged(bool value) => LoginCommand.NotifyCanExecuteChanged();
}
