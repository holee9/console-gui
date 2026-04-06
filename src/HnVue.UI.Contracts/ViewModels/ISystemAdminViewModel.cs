using System.Windows.Input;
using HnVue.Common.Models;

namespace HnVue.UI.Contracts.ViewModels;

/// <summary>Contract for the system administration ViewModel.</summary>
public interface ISystemAdminViewModel : IViewModelBase
{
    /// <summary>Gets or sets the system settings being edited.</summary>
    SystemSettings Settings { get; set; }

    /// <summary>Gets a value indicating whether the current session has administrator privileges.</summary>
    bool IsAdminUser { get; }

    /// <summary>Gets a value indicating whether a settings load or save operation is in progress.</summary>
    bool IsBusy { get; }

    /// <summary>Gets the human-readable status message for the last settings operation.</summary>
    string? StatusMessage { get; }

    /// <summary>Gets the command that loads current settings from persistent storage.</summary>
    ICommand LoadSettingsCommand { get; }

    /// <summary>Gets the command that persists the current settings to storage.</summary>
    ICommand SaveSettingsCommand { get; }
}
