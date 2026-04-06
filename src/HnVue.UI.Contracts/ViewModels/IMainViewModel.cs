using HnVue.Common.Models;

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
}
