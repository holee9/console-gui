namespace HnVue.UI.Contracts.Dialogs;

/// <summary>
/// Defines the contract for displaying modal dialogs.
/// Abstracts WPF MessageBox and custom dialog windows.
/// </summary>
public interface IDialogService
{
    /// <summary>Shows a confirmation dialog with OK/Cancel.</summary>
    Task<bool> ShowConfirmAsync(string title, string message);

    /// <summary>Shows an error dialog.</summary>
    Task ShowErrorAsync(string title, string message);

    /// <summary>Shows a warning dialog.</summary>
    Task ShowWarningAsync(string title, string message);
}
