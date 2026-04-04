using HnVue.UI.ViewModels;

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
    public LoginView(LoginViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
