using System.Collections.Generic;
using System.Windows.Input;

namespace HnVue.UI.Contracts.ViewModels;

// @MX:NOTE ISettingsViewModel — Settings dialog contract. PPT 슬라이드 14~21.
//          Top-tab navigation (moved from left-side menu per PPT).
//          Network tab consolidates PACS + Worklist + Print.
//          "Login Popup" renamed to "Access Notice".
//          "Only No matching" renamed to "Un-Matched" (RIS Code sub-tab).
//          Account: Operator field removed; Role changed to ComboBox.
/// <summary>Contract for the Settings ViewModel. PPT 슬라이드 14~21.</summary>
public interface ISettingsViewModel : IViewModelBase
{
    // ── Tab navigation ──────────────────────────────────────────────────────

    /// <summary>Gets or sets the key of the currently active top-level tab.</summary>
    string ActiveTab { get; set; }

    /// <summary>Gets the ordered list of tab labels shown at the top of the dialog.</summary>
    IReadOnlyList<string> Tabs { get; }

    // ── System tab ──────────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the "Access Notice" text shown when a session starts.
    /// PPT: renamed from "Login Popup" to "Access Notice".
    /// </summary>
    string AccessNoticeText { get; set; }

    // ── Account tab ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets the ID for a new account being created.</summary>
    string NewAccountId { get; set; }

    /// <summary>
    /// Gets or sets the role assigned to the new account.
    /// Bound to a ComboBox. PPT: Operator field removed; role is now a dropdown.
    /// </summary>
    string NewAccountRole { get; set; }

    /// <summary>Gets the list of selectable roles (Admin, Technician, Radiologist).</summary>
    IReadOnlyList<string> AvailableRoles { get; }

    // ── Network tab (PACS + Worklist + Print merged) ─────────────────────────

    /// <summary>Gets or sets the PACS server host address.</summary>
    string PacsServerAddress { get; set; }

    /// <summary>Gets or sets the PACS server port (default 104).</summary>
    int PacsServerPort { get; set; }

    /// <summary>Gets or sets the Worklist server host address.</summary>
    string WorklistServerAddress { get; set; }

    /// <summary>Gets or sets the Worklist server port (default 4006).</summary>
    int WorklistServerPort { get; set; }

    // ── RIS Code tab ────────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the active RIS Code sub-tab.
    /// Values: "Matching" or "Un-Matched".
    /// PPT: "Only No matching" renamed to "Un-Matched".
    /// </summary>
    string ActiveRisTab { get; set; }

    // ── Commands ────────────────────────────────────────────────────────────

    /// <summary>Gets the command that persists all settings changes.</summary>
    ICommand SaveCommand { get; }

    /// <summary>Gets the command that discards changes and closes the dialog.</summary>
    ICommand CancelCommand { get; }

    /// <summary>Gets the command that activates a tab by label. Parameter: tab label string.</summary>
    ICommand SelectTabCommand { get; }

    /// <summary>Raised when settings are saved successfully.</summary>
    event EventHandler? SaveCompleted;

    /// <summary>Raised when the user cancels the dialog.</summary>
    event EventHandler? Cancelled;
}
