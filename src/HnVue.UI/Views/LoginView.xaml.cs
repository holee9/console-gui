using System.Windows;
using HnVue.UI.Contracts.ViewModels;

namespace HnVue.UI.Views;

/// <summary>Code-behind for <see cref="LoginView"/>.</summary>
public partial class LoginView : System.Windows.Controls.UserControl
{
    /// <summary>Initialises a new instance of <see cref="LoginView"/>.</summary>
    public LoginView()
    {
        InitializeComponent();
    }

    /// <summary>Initialises a new instance of <see cref="LoginView"/> with the given ViewModel.</summary>
    /// <param name="viewModel">The ViewModel to bind to this view.</param>
    public LoginView(ILoginViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    // WPF PasswordBox.Password is not a DependencyProperty for security reasons,
    // so {Binding Password} does not work. Forward changes via code-behind.
    // SWR-CS-070 / Issue #9
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ILoginViewModel vm)
            vm.Password = PasswordBox.Password;
    }
}
