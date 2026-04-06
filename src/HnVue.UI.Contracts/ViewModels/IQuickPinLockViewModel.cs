using System.Windows.Input;

namespace HnVue.UI.Contracts.ViewModels;

/// <summary>Contract for the Quick PIN lock overlay ViewModel.</summary>
public interface IQuickPinLockViewModel : IViewModelBase
{
    /// <summary>Gets or sets the PIN digits entered by the user.</summary>
    string Pin { get; set; }

    /// <summary>Gets the number of verification attempts remaining before force-logout.</summary>
    int RemainingAttempts { get; }

    /// <summary>Gets a value indicating whether a PIN verification call is in progress.</summary>
    bool IsVerifying { get; }

    /// <summary>Gets the username whose session is currently locked, or null if the overlay is inactive.</summary>
    string? LockedUsername { get; }

    /// <summary>Activates the lock overlay for the currently authenticated user.</summary>
    void Activate();

    /// <summary>Gets the command that submits the entered PIN for verification.</summary>
    ICommand VerifyPinCommand { get; }

    /// <summary>Raised when PIN verification succeeds and the session is resumed.</summary>
    event EventHandler? SessionResumed;

    /// <summary>Raised when attempts are exhausted and the user must re-authenticate fully.</summary>
    event EventHandler? ForceLogout;
}
