using System.Collections.Generic;
using HnVue.Common.Models;
using HnVue.UI.Contracts.Navigation;

namespace HnVue.UI.Contracts.ViewModels;

/// <summary>Contract for the main shell ViewModel.</summary>
public interface IMainViewModel : IViewModelBase
{
    /// <summary>Gets a value indicating whether the login panel is currently visible.</summary>
    bool IsLoginVisible { get; }

    /// <summary>Gets a value indicating whether the main application content is visible.</summary>
    bool IsMainContentVisible { get; }

    /// <summary>Gets a value indicating whether a user session is currently authenticated.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Gets the display name of the authenticated user, or null if unauthenticated.</summary>
    string? CurrentUsername { get; }

    /// <summary>Gets the localised role label for the current user, or null if unauthenticated.</summary>
    string? CurrentRoleDisplay { get; }

    /// <summary>Gets the navigation key identifying the currently active content area.</summary>
    string ActiveNavItem { get; }

    /// <summary>Gets a value indicating whether the TLS connection to the backend is inactive.</summary>
    bool IsTlsInactive { get; }

    /// <summary>Gets a value indicating whether the session-timeout warning overlay is visible.</summary>
    bool IsTimeoutWarningVisible { get; }

    /// <summary>Gets the number of seconds remaining until automatic session logout.</summary>
    int SessionTimeoutCountdown { get; }

    /// <summary>Gets the patient list sub-ViewModel.</summary>
    IPatientListViewModel PatientListViewModel { get; }

    /// <summary>Gets the image viewer sub-ViewModel.</summary>
    IImageViewerViewModel ImageViewerViewModel { get; }

    /// <summary>Gets the workflow control sub-ViewModel.</summary>
    IWorkflowViewModel WorkflowViewModel { get; }

    /// <summary>Gets the dose display sub-ViewModel.</summary>
    IDoseDisplayViewModel DoseDisplayViewModel { get; }

    /// <summary>Gets the CD/DVD burn sub-ViewModel.</summary>
    ICDBurnViewModel CDBurnViewModel { get; }

    /// <summary>Gets the system administration sub-ViewModel.</summary>
    ISystemAdminViewModel SystemAdminViewModel { get; }

    /// <summary>Transitions the shell into the authenticated state for the given user.</summary>
    /// <param name="user">The authenticated user details.</param>
    void OnLoginSuccess(AuthenticatedUser user);

    /// <summary>Re-reads shared context (e.g. after a patient selection) and refreshes bound properties.</summary>
    void RefreshFromContext();

    /// <summary>Resets the inactivity timer, deferring the session-timeout countdown.</summary>
    void ResetSessionTimer();

    // ── Navigation shell API (DESIGN_PLAN_v2.md / team-design request) ────────

    /// <summary>Gets the ViewModel currently displayed in the main content region.</summary>
    object? CurrentView { get; }

    /// <summary>Gets the navigation token history stack (most recent = last item).</summary>
    IReadOnlyList<NavigationToken> NavigationHistory { get; }

    /// <summary>Navigates the shell to the specified view.</summary>
    /// <param name="token">The navigation target.</param>
    /// <param name="parameter">Optional parameter passed to the target ViewModel.</param>
    void NavigateTo(NavigationToken token, object? parameter = null);

    /// <summary>Navigates back to the previous view in the navigation stack.</summary>
    void NavigateBack();
}
