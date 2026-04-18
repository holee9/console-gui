using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.UI.Contracts.ViewModels;

namespace HnVue.UI.ViewModels;

// @MX:NOTE SettingsViewModel — Settings dialog ViewModel. PPT 슬라이드 14~21.
//          Top-tab navigation; Network tab consolidates PACS+Worklist+Print.
//          "Access Notice" replaces "Login Popup".
//          "Un-Matched" replaces "Only No matching" in RIS Code tab.
//          Operator field removed from Account tab; Role is now a ComboBox.
/// <summary>
/// ViewModel for the Settings dialog.
/// PPT 슬라이드 14~21: top-tab layout, merged Network tab, renamed labels.
/// </summary>
public sealed partial class SettingsViewModel : ObservableObject, ISettingsViewModel
{
    // IViewModelBase explicit mapping
    bool IViewModelBase.IsLoading => IsLoading;
    ICommand ISettingsViewModel.SaveCommand => SaveCommand;
    ICommand ISettingsViewModel.CancelCommand => CancelCommand;
    ICommand ISettingsViewModel.SelectTabCommand => SelectTabCommand;

    private readonly List<string> _tabs = new()
    {
        "System", "Account", "Detector", "Generator",
        "Network", "Display", "Option", "Database", "DicomSet", "RIS Code"
    };

    private readonly List<string> _availableRoles = new()
    {
        "Admin", "Technician", "Radiologist"
    };

    /// <inheritdoc/>
    public IReadOnlyList<string> Tabs => _tabs;

    /// <inheritdoc/>
    public IReadOnlyList<string> AvailableRoles => _availableRoles;

    // ── Observable properties ──────────────────────────────────────────────

    [ObservableProperty] private string _activeTab = "System";
    [ObservableProperty] private string _accessNoticeText = string.Empty;
    [ObservableProperty] private string _newAccountId = string.Empty;
    [ObservableProperty] private string _newAccountRole = "Technician";
    [ObservableProperty] private string _pacsServerAddress = string.Empty;
    [ObservableProperty] private int _pacsServerPort = 104;
    [ObservableProperty] private string _worklistServerAddress = string.Empty;
    [ObservableProperty] private int _worklistServerPort = 4006;
    [ObservableProperty] private string _activeRisTab = "Matching";
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _errorMessage;

    /// <inheritdoc/>
    public event EventHandler? SaveCompleted;

    /// <inheritdoc/>
    public event EventHandler? Cancelled;

    // ── Commands ──────────────────────────────────────────────────────────

    /// <summary>Activates the tab whose label matches <paramref name="tab"/>.</summary>
    [RelayCommand]
    private void SelectTab(string? tab)
    {
        if (!string.IsNullOrEmpty(tab))
            ActiveTab = tab;
    }

    /// <summary>
    /// Persists all settings values.
    /// </summary>
    /// <remarks>
    /// Currently a no-op placeholder that raises <see cref="SaveCompleted"/>.
    /// @MX:TODO Wire to ISettingsService.SaveAsync once the service surface is defined
    /// in UI.Contracts (tracked via SWR-UI-SE-011). Placeholder keeps the dialog flow
    /// exercised by unit tests while the Settings persistence layer is designed.
    /// </remarks>
    [RelayCommand]
    private async Task SaveAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            // @MX:TODO Replace with await _settingsService.SaveAsync(snapshot) once ISettingsService lands.
            await Task.Delay(1);
            SaveCompleted?.Invoke(this, EventArgs.Empty);
        }
        finally { IsLoading = false; }
    }

    /// <summary>Discards changes and raises <see cref="Cancelled"/>.</summary>
    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke(this, EventArgs.Empty);
}
