using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Abstractions;
using HnVue.Common.Results;

namespace HnVue.UI.ViewModels;

/// <summary>
/// ViewModel for the login screen. Handles credential input, authentication flow,
/// and exposes the result via the <see cref="LoginSucceeded"/> event.
/// </summary>
public sealed partial class LoginViewModel : ObservableObject
{
    private readonly ISecurityService _securityService;
    private readonly ISecurityContext _securityContext;

    /// <summary>
    /// Initialises a new instance of <see cref="LoginViewModel"/>.
    /// </summary>
    /// <param name="securityService">Service used to authenticate users.</param>
    /// <param name="securityContext">Context updated upon successful authentication.</param>
    public LoginViewModel(ISecurityService securityService, ISecurityContext securityContext)
    {
        ArgumentNullException.ThrowIfNull(securityService);
        ArgumentNullException.ThrowIfNull(securityContext);
        _securityService = securityService;
        _securityContext = securityContext;
    }

    /// <summary>Raised when authentication succeeds, carrying the issued token.</summary>
    public event EventHandler<LoginSuccessEventArgs>? LoginSucceeded;

    /// <summary>Gets or sets the username entered by the user.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _username = string.Empty;

    /// <summary>Gets or sets the password entered by the user.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    /// <summary>Gets or sets a value indicating whether an authentication request is in progress.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool _isLoading;

    /// <summary>Gets or sets the user-facing error message from a failed login attempt, or <see langword="null"/>.</summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// Executes the login flow: calls <see cref="ISecurityService.AuthenticateAsync"/>,
    /// raises <see cref="LoginSucceeded"/> on success, or sets <see cref="ErrorMessage"/> on failure.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var result = await _securityService.AuthenticateAsync(Username, Password).ConfigureAwait(true);
            result.Match(
                token =>
                {
                    LoginSucceeded?.Invoke(this, new LoginSuccessEventArgs(token));
                    return true;
                },
                (code, _) =>
                {
                    ErrorMessage = code switch
                    {
                        ErrorCode.AccountLocked => "계정이 잠겼습니다. 관리자에게 문의하세요.",
                        _ => "사용자명 또는 비밀번호가 올바르지 않습니다.",
                    };
                    return false;
                });
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Returns <see langword="true"/> when the login command may execute:
    /// both username and password must be non-whitespace and no request must be in flight.
    /// </summary>
    private bool CanLogin()
        => !string.IsNullOrWhiteSpace(Username)
           && !string.IsNullOrWhiteSpace(Password)
           && !IsLoading;
}
