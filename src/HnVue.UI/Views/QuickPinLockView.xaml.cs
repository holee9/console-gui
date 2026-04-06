using System.Windows;
using HnVue.UI.Contracts.ViewModels;

namespace HnVue.UI.Views;

/// <summary>Code-behind for <see cref="QuickPinLockView"/>.</summary>
public partial class QuickPinLockView : System.Windows.Controls.UserControl
{
    /// <summary>Initialises a new instance of <see cref="QuickPinLockView"/>.</summary>
    public QuickPinLockView()
    {
        InitializeComponent();
    }

    /// <summary>Initialises a new instance of <see cref="QuickPinLockView"/> with the given ViewModel.</summary>
    public QuickPinLockView(IQuickPinLockViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    // WPF PasswordBox.Password binding via code-behind (same pattern as LoginView).
    private void PinBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is IQuickPinLockViewModel vm)
            vm.Pin = PinBox.Password;
    }
}
