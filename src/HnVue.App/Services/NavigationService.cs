using HnVue.UI.Contracts.Navigation;
using HnVue.UI.Contracts.ViewModels;

namespace HnVue.App.Services;

/// <summary>
/// Shell-level navigation service. Delegates to <see cref="IMainViewModel"/> which owns
/// the navigation stack and the <c>CurrentView</c> observable property.
/// Registered as Singleton in the DI container — safe because <see cref="IMainViewModel"/>
/// is also Singleton (one shell per app lifetime).
/// </summary>
// @MX:ANCHOR NavigationService - @MX:REASON: Sole implementation of INavigationService; all VM-initiated navigation flows through here
public sealed class NavigationService : INavigationService
{
    private readonly IMainViewModel _shell;

    /// <inheritdoc/>
    public event EventHandler<NavigationToken>? Navigated;

    /// <summary>Initialises the service with the shell ViewModel.</summary>
    /// <param name="shell">The main shell ViewModel that owns the navigation stack.</param>
    public NavigationService(IMainViewModel shell)
    {
        ArgumentNullException.ThrowIfNull(shell, nameof(shell));
        _shell = shell;
    }

    /// <inheritdoc/>
    public bool CanGoBack => _shell.NavigationHistory.Count > 0;

    /// <inheritdoc/>
    public void NavigateTo(NavigationToken token) => NavigateTo(token, null);

    /// <inheritdoc/>
    public void NavigateTo(NavigationToken token, object? parameter)
    {
        _shell.NavigateTo(token, parameter);
        Navigated?.Invoke(this, token);
    }

    /// <inheritdoc/>
    public bool GoBack()
    {
        if (!CanGoBack) return false;
        _shell.NavigateBack();
        return true;
    }
}
