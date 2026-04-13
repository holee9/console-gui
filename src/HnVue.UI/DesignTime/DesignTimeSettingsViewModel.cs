using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace HnVue.UI.DesignTime;

/// <summary>
/// Designer-only mock data for SettingsView.
/// Keeps the VS2022 designer usable without running the application.
/// PPT Slides 14-22.
/// </summary>
public sealed class DesignTimeSettingsViewModel
{
    public DesignTimeSettingsViewModel()
    {
        SelectTabCommand = new RelayCommand<string?>(tab =>
        {
            if (!string.IsNullOrWhiteSpace(tab))
            {
                ActiveTab = tab;
            }
        });
        SaveCommand = new RelayCommand(() => { });
        CancelCommand = new RelayCommand(() => { });
    }

    // Tab navigation
    public string ActiveTab { get; set; } = "System";

    public IReadOnlyList<string> Tabs { get; } = new[]
    {
        "System", "Account", "Detector", "Generator",
        "Network", "Display", "Option", "Database", "DicomSet", "RIS Code"
    };

    // System tab
    public string AccessNoticeText { get; set; } =
        "This system is for authorized medical personnel only.\nUnauthorized access is prohibited.";

    // Account tab
    public string NewAccountId { get; set; } = "tech01";
    public string NewAccountRole { get; set; } = "Technician";

    public IReadOnlyList<string> AvailableRoles { get; } = new[]
    {
        "Admin", "Technician", "Radiologist"
    };

    // Network tab
    public string PacsServerAddress { get; set; } = "192.168.1.100";
    public int PacsServerPort { get; set; } = 104;
    public string WorklistServerAddress { get; set; } = "192.168.1.100";
    public int WorklistServerPort { get; set; } = 4006;

    // RIS Code tab
    public string ActiveRisTab { get; set; } = "Matching";

    // Error / status
    public string? ErrorMessage { get; set; }

    // Commands
    public ICommand SelectTabCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
}
