using HnVue.UI.Contracts.Events;
using HnVue.UI.Contracts.ViewModels;
using MahApps.Metro.Controls;

namespace HnVue.App;

/// <summary>
/// Code-behind for the main application window.
/// Sets <see cref="IMainViewModel"/> as DataContext and wires the login success event.
/// </summary>
public partial class MainWindow : MetroWindow
{
    private readonly IMainViewModel _mainViewModel;

    /// <summary>Initialises the main window, sets its DataContext, and subscribes to login events.</summary>
    /// <param name="mainViewModel">The shell ViewModel injected by the DI container.</param>
    /// <param name="loginViewModel">The login ViewModel injected by the DI container.</param>
    public MainWindow(IMainViewModel mainViewModel, ILoginViewModel loginViewModel)
    {
        ArgumentNullException.ThrowIfNull(mainViewModel);
        ArgumentNullException.ThrowIfNull(loginViewModel);
        _mainViewModel = mainViewModel;
        DataContext = mainViewModel;
        InitializeComponent();

        // Set LoginView's DataContext from DI and wire the success event
        if (LoginViewControl != null)
        {
            LoginViewControl.DataContext = loginViewModel;
            loginViewModel.LoginSucceeded += OnLoginSucceeded;
        }
    }

    private void OnLoginSucceeded(object? sender, LoginSuccessEventArgs e)
    {
        var user = new HnVue.Common.Models.AuthenticatedUser(e.Token.UserId, e.Token.Username, e.Token.Role);
        _mainViewModel.OnLoginSuccess(user);
    }
}
