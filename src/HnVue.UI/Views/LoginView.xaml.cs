using System.Windows;
using System.Windows.Controls;
using HnVue.UI.ViewModels;

namespace HnVue.UI.Views;

/// <summary>
/// Code-behind for <see cref="LoginView"/>.
/// Handles the <see cref="PasswordBox"/> password-changed event because
/// WPF's <see cref="PasswordBox.Password"/> property is not a dependency property
/// and cannot be data-bound directly for security reasons.
/// </summary>
public partial class LoginView : UserControl
{
    /// <summary>Initialises the LoginView and its generated XAML components.</summary>
    public LoginView() => InitializeComponent();

    /// <summary>
    /// Propagates the <see cref="PasswordBox.Password"/> value to the bound
    /// <see cref="LoginViewModel.Password"/> property whenever the user types.
    /// </summary>
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm && sender is PasswordBox pb)
        {
            vm.Password = pb.Password;
        }
    }
}
