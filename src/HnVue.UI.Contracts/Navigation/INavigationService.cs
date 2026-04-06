namespace HnVue.UI.Contracts.Navigation;

/// <summary>
/// Defines the contract for shell-level navigation between views.
/// Implementations switch the content displayed in the shell's main region.
/// </summary>
public interface INavigationService
{
    /// <summary>Navigates to the specified view.</summary>
    void NavigateTo(NavigationToken token);

    /// <summary>Navigates to the specified view with a parameter.</summary>
    void NavigateTo(NavigationToken token, object? parameter);

    /// <summary>Navigates to the previous view.</summary>
    bool GoBack();

    /// <summary>Gets a value indicating whether backward navigation is possible.</summary>
    bool CanGoBack { get; }

    /// <summary>Raised when navigation occurs.</summary>
    event EventHandler<NavigationToken>? Navigated;
}
