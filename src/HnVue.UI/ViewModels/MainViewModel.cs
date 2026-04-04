using CommunityToolkit.Mvvm.ComponentModel;
using HnVue.Common.Abstractions;

namespace HnVue.UI.ViewModels;

/// <summary>
/// ViewModel for the main application shell. Reflects the current authentication state
/// sourced from <see cref="ISecurityContext"/>.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly ISecurityContext _securityContext;

    /// <summary>
    /// Initialises a new instance of <see cref="MainViewModel"/>.
    /// </summary>
    /// <param name="securityContext">The application-wide security context.</param>
    public MainViewModel(ISecurityContext securityContext)
    {
        ArgumentNullException.ThrowIfNull(securityContext);
        _securityContext = securityContext;
    }

    /// <summary>Gets or sets the login name of the currently authenticated user.</summary>
    [ObservableProperty]
    private string? _currentUsername;

    /// <summary>Gets or sets a display-friendly representation of the current user's role.</summary>
    [ObservableProperty]
    private string? _currentRoleDisplay;

    /// <summary>Gets or sets a value indicating whether a user is currently authenticated.</summary>
    [ObservableProperty]
    private bool _isAuthenticated;

    /// <summary>
    /// Synchronises this ViewModel's observable properties from the current
    /// <see cref="ISecurityContext"/> state. Call this after a login or logout event.
    /// </summary>
    public void RefreshFromContext()
    {
        IsAuthenticated = _securityContext.IsAuthenticated;
        CurrentUsername = _securityContext.CurrentUsername;
        CurrentRoleDisplay = _securityContext.CurrentRole?.ToString();
    }
}
