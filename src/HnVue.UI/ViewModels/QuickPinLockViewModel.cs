using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Abstractions;

namespace HnVue.UI.ViewModels;

/// <summary>
/// ViewModel for the Quick PIN lock overlay shown during an active workflow session.
/// Allows the operator to resume the session without full re-authentication.
/// SWR-CS-076 (Safety+Security-related, HAZ-SEC, HAZ-RAD). Issue #12, #34.
/// </summary>
public sealed partial class QuickPinLockViewModel : ObservableObject
{
    private const int MaxPinAttempts = 3;

    private readonly ISecurityContext _securityContext;
    private readonly ISecurityService _securityService;
    private int _failedAttempts;

    /// <summary>Raised when the PIN is verified and the session can resume.</summary>
    public event EventHandler? SessionResumed;

    /// <summary>Raised when the PIN fails 3 times and a full logout is required.</summary>
    public event EventHandler? ForceLogout;

    /// <summary>Initialises a new instance of <see cref="QuickPinLockViewModel"/>.</summary>
    public QuickPinLockViewModel(ISecurityContext securityContext, ISecurityService securityService)
    {
        _securityContext = securityContext ?? throw new ArgumentNullException(nameof(securityContext));
        _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
    }

    /// <summary>Gets or sets the PIN entered by the user (4-6 digits).</summary>
    [ObservableProperty]
    private string _pin = string.Empty;

    /// <summary>Gets or sets the error message displayed after a failed PIN attempt.</summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>Gets or sets the number of remaining PIN attempts before forced logout.</summary>
    [ObservableProperty]
    private int _remainingAttempts = MaxPinAttempts;

    /// <summary>Gets or sets a value indicating whether a PIN verification is in progress.</summary>
    [ObservableProperty]
    private bool _isVerifying;

    /// <summary>Gets or sets the display name of the locked-in user.</summary>
    [ObservableProperty]
    private string? _lockedUsername;

    /// <summary>Activates the lock overlay for the current authenticated user.</summary>
    public void Activate()
    {
        Pin = string.Empty;
        ErrorMessage = null;
        _failedAttempts = 0;
        RemainingAttempts = MaxPinAttempts;
        LockedUsername = _securityContext.IsAuthenticated ? _securityContext.CurrentUsername : null;
    }

    /// <summary>
    /// Verifies the entered PIN against the current user's stored Quick PIN.
    /// On success, raises <see cref="SessionResumed"/>.
    /// After 3 failures, raises <see cref="ForceLogout"/>.
    /// SWR-CS-076.
    /// </summary>
    [RelayCommand]
    private async Task VerifyPinAsync()
    {
        if (Pin.Length < 4 || Pin.Length > 6)
        {
            ErrorMessage = "PIN은 4~6자리여야 합니다.";
            return;
        }

        IsVerifying = true;
        ErrorMessage = null;

        try
        {
            var userId = _securityContext.CurrentUserId;
            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "No authenticated user.";
                return;
            }

            var result = await _securityService.VerifyQuickPinAsync(userId, Pin).ConfigureAwait(true);
            if (result.IsSuccess)
            {
                SessionResumed?.Invoke(this, EventArgs.Empty);
                return;
            }

            _failedAttempts++;
            RemainingAttempts = MaxPinAttempts - _failedAttempts;

            if (_failedAttempts >= MaxPinAttempts)
            {
                ErrorMessage = "PIN 3회 실패 — 전체 로그아웃됩니다.";
                await Task.Delay(1500).ConfigureAwait(true);
                ForceLogout?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = $"PIN이 올바르지 않습니다. 남은 시도: {RemainingAttempts}회";
                Pin = string.Empty;
            }
        }
        finally
        {
            IsVerifying = false;
        }
    }
}
