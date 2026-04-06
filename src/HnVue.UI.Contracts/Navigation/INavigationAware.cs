namespace HnVue.UI.Contracts.Navigation;

/// <summary>
/// Implemented by ViewModels that need lifecycle callbacks during navigation.
/// </summary>
public interface INavigationAware
{
    /// <summary>Called when the ViewModel's view is navigated to.</summary>
    void OnNavigatedTo(object? parameter);

    /// <summary>Called when the ViewModel's view is navigated away from.</summary>
    void OnNavigatedFrom();
}
