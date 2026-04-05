using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;

namespace HnVue.UI.ViewModels;

/// <summary>
/// ViewModel for the main shell window.
/// Manages navigation state and the currently active view.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly ISecurityContext _securityContext;

    /// <summary>Gets the ViewModel for the patient list panel.</summary>
    public PatientListViewModel PatientListViewModel { get; }

    /// <summary>Gets the ViewModel for the image viewer panel.</summary>
    public ImageViewerViewModel ImageViewerViewModel { get; }

    /// <summary>Gets the ViewModel for the workflow/exposure panel.</summary>
    public WorkflowViewModel WorkflowViewModel { get; }

    /// <summary>Gets the ViewModel for the dose display panel.</summary>
    public DoseDisplayViewModel DoseDisplayViewModel { get; }

    /// <summary>Initialises a new instance of <see cref="MainViewModel"/>.</summary>
    public MainViewModel(
        ISecurityContext securityContext,
        PatientListViewModel patientListViewModel,
        ImageViewerViewModel imageViewerViewModel,
        WorkflowViewModel workflowViewModel,
        DoseDisplayViewModel doseDisplayViewModel)
    {
        ArgumentNullException.ThrowIfNull(securityContext, nameof(securityContext));
        ArgumentNullException.ThrowIfNull(patientListViewModel, nameof(patientListViewModel));
        ArgumentNullException.ThrowIfNull(imageViewerViewModel, nameof(imageViewerViewModel));
        ArgumentNullException.ThrowIfNull(workflowViewModel, nameof(workflowViewModel));
        ArgumentNullException.ThrowIfNull(doseDisplayViewModel, nameof(doseDisplayViewModel));
        _securityContext = securityContext;
        PatientListViewModel = patientListViewModel;
        ImageViewerViewModel = imageViewerViewModel;
        WorkflowViewModel = workflowViewModel;
        DoseDisplayViewModel = doseDisplayViewModel;
    }

    /// <summary>Gets or sets a value indicating whether the login view is visible.</summary>
    [ObservableProperty]
    private bool _isLoginVisible = true;

    /// <summary>Gets or sets a value indicating whether the main content is visible.</summary>
    [ObservableProperty]
    private bool _isMainContentVisible;

    /// <summary>Gets or sets a value indicating whether the current user is authenticated.</summary>
    [ObservableProperty]
    private bool _isAuthenticated;

    /// <summary>Gets or sets the username displayed in the shell header.</summary>
    [ObservableProperty]
    private string? _currentUsername;

    /// <summary>Gets or sets the display string for the current user's role.</summary>
    [ObservableProperty]
    private string? _currentRoleDisplay;

    /// <summary>Gets or sets the currently active navigation item label.</summary>
    [ObservableProperty]
    private string _activeNavItem = string.Empty;

    /// <summary>
    /// Reads authentication state from <see cref="ISecurityContext"/> and updates
    /// <see cref="IsAuthenticated"/>, <see cref="CurrentUsername"/>, and <see cref="CurrentRoleDisplay"/>.
    /// </summary>
    public void RefreshFromContext()
    {
        IsAuthenticated = _securityContext.IsAuthenticated;
        CurrentUsername = _securityContext.IsAuthenticated ? _securityContext.CurrentUsername : null;
        CurrentRoleDisplay = _securityContext.IsAuthenticated && _securityContext.CurrentRole.HasValue
            ? _securityContext.CurrentRole.Value.ToString()
            : null;
    }

    /// <summary>Handles a successful login event and transitions to the main content.</summary>
    /// <param name="user">The authenticated user.</param>
    public void OnLoginSuccess(Common.Models.AuthenticatedUser user)
    {
        CurrentUsername = user.Username;
        IsLoginVisible = false;
        IsMainContentVisible = true;
        ActiveNavItem = "PatientList";
        RefreshFromContext();
    }

    /// <summary>Navigates to the specified section.</summary>
    /// <param name="navItem">The identifier of the navigation target.</param>
    [RelayCommand]
    private void Navigate(string navItem)
    {
        ActiveNavItem = navItem;
    }

    /// <summary>Logs out the current user and returns to the login screen.</summary>
    [RelayCommand]
    private void Logout()
    {
        _securityContext.ClearCurrentUser();
        CurrentUsername = null;
        CurrentRoleDisplay = null;
        IsAuthenticated = false;
        IsMainContentVisible = false;
        IsLoginVisible = true;
        ActiveNavItem = string.Empty;
    }
}
