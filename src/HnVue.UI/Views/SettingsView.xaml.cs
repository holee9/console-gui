using System.Windows.Controls;

namespace HnVue.UI.Views;

/// <summary>
/// Code-behind for <see cref="SettingsView"/>.
/// All logic is delegated to <c>ISettingsViewModel</c> via data binding.
/// PPT 슬라이드 14~21: top-tab navigation, merged Network tab, renamed labels.
/// </summary>
public partial class SettingsView : UserControl
{
    /// <summary>Initialises a new instance of <see cref="SettingsView"/>.</summary>
    public SettingsView()
    {
        InitializeComponent();
    }
}
