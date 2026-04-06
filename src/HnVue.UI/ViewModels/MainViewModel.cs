using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;

namespace HnVue.UI.ViewModels;

/// <summary>
/// ViewModel for the main shell window.
/// Manages navigation state and the currently active view.
/// </summary>
public sealed partial class MainViewModel : ObservableObject, IDisposable
{
    private const int SessionTimeoutMinutes = 15;
    private const int TimeoutWarningSeconds = 180; // warn at 3 minutes remaining

    private readonly ISecurityContext _securityContext;
    private readonly ISecurityService _securityService;
    private readonly System.Timers.Timer _sessionTimer;
    private int _secondsUntilTimeout;

    /// <summary>Gets the ViewModel for the patient list panel.</summary>
    public PatientListViewModel PatientListViewModel { get; }

    /// <summary>Gets the ViewModel for the image viewer panel.</summary>
    public ImageViewerViewModel ImageViewerViewModel { get; }

    /// <summary>Gets the ViewModel for the workflow/exposure panel.</summary>
    public WorkflowViewModel WorkflowViewModel { get; }

    /// <summary>Gets the ViewModel for the dose display panel.</summary>
    public DoseDisplayViewModel DoseDisplayViewModel { get; }

    /// <summary>Gets the ViewModel for the CD/DVD burn panel.</summary>
    public CDBurnViewModel CDBurnViewModel { get; }

    /// <summary>Gets the ViewModel for the system administration panel.</summary>
    public SystemAdminViewModel SystemAdminViewModel { get; }

    /// <summary>Initialises a new instance of <see cref="MainViewModel"/>.</summary>
    /// <remarks>Issue #17 — CDBurnViewModel and SystemAdminViewModel added to navigation graph.</remarks>
    /// <remarks>Issue #29 — ISecurityService injected for logout audit logging.</remarks>
    public MainViewModel(
        ISecurityContext securityContext,
        ISecurityService securityService,
        PatientListViewModel patientListViewModel,
        ImageViewerViewModel imageViewerViewModel,
        WorkflowViewModel workflowViewModel,
        DoseDisplayViewModel doseDisplayViewModel,
        CDBurnViewModel cdburnViewModel,
        SystemAdminViewModel systemAdminViewModel)
    {
        ArgumentNullException.ThrowIfNull(securityContext, nameof(securityContext));
        ArgumentNullException.ThrowIfNull(securityService, nameof(securityService));
        ArgumentNullException.ThrowIfNull(patientListViewModel, nameof(patientListViewModel));
        ArgumentNullException.ThrowIfNull(imageViewerViewModel, nameof(imageViewerViewModel));
        ArgumentNullException.ThrowIfNull(workflowViewModel, nameof(workflowViewModel));
        ArgumentNullException.ThrowIfNull(doseDisplayViewModel, nameof(doseDisplayViewModel));
        ArgumentNullException.ThrowIfNull(cdburnViewModel, nameof(cdburnViewModel));
        ArgumentNullException.ThrowIfNull(systemAdminViewModel, nameof(systemAdminViewModel));
        _securityContext = securityContext;
        _securityService = securityService;
        PatientListViewModel = patientListViewModel;
        ImageViewerViewModel = imageViewerViewModel;
        WorkflowViewModel = workflowViewModel;
        DoseDisplayViewModel = doseDisplayViewModel;
        CDBurnViewModel = cdburnViewModel;
        SystemAdminViewModel = systemAdminViewModel;

        // Session timeout timer — SWR-CS-075 / Issue #14
        _secondsUntilTimeout = SessionTimeoutMinutes * 60;
        _sessionTimer = new System.Timers.Timer(1000);
        _sessionTimer.Elapsed += OnSessionTimerTick;
        _sessionTimer.AutoReset = true;
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
    /// Gets or sets a value indicating whether TLS is inactive on the DICOM network connection.
    /// When true, a permanent yellow warning banner is shown. SWR-CS-079 / Issue #13.
    /// </summary>
    [ObservableProperty]
    private bool _isTlsInactive;

    /// <summary>
    /// Gets or sets a value indicating whether the session timeout warning countdown is visible.
    /// True when ≤3 minutes remain before automatic session logout. SWR-CS-075 / Issue #14.
    /// </summary>
    [ObservableProperty]
    private bool _isTimeoutWarningVisible;

    /// <summary>
    /// Gets or sets the remaining seconds displayed in the session timeout countdown banner.
    /// SWR-CS-075 / Issue #14.
    /// </summary>
    [ObservableProperty]
    private int _sessionTimeoutCountdown;

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
        ResetSessionTimer();
        _sessionTimer.Start();
    }

    /// <summary>Resets the session inactivity timer to the full timeout duration.</summary>
    public void ResetSessionTimer()
    {
        _secondsUntilTimeout = SessionTimeoutMinutes * 60;
        SessionTimeoutCountdown = 0;
        IsTimeoutWarningVisible = false;
    }

    /// <summary>Navigates to the specified section.</summary>
    /// <param name="navItem">The identifier of the navigation target.</param>
    [RelayCommand]
    private void Navigate(string navItem)
    {
        ActiveNavItem = navItem;
        ResetSessionTimer();
    }

    /// <summary>
    /// Initiates emergency patient registration workflow.
    /// SWR-NF-UX-026 (Safety-Critical, HAZ-RAD). Issue #11.
    /// </summary>
    [RelayCommand]
    private void Emergency()
    {
        ActiveNavItem = "Emergency";
        // TODO: Navigate to emergency patient registration view when implemented.
    }

    /// <summary>Logs out the current user and returns to the login screen.</summary>
    /// <remarks>Issue #29 — Writes LOGOUT audit entry before clearing session context.</remarks>
    [RelayCommand]
    private void Logout()
    {
        _sessionTimer.Stop();

        // Fire-and-forget audit log for logout. Audit failure must not block session teardown.
        var userId = _securityContext.CurrentUserId;
        if (userId is not null)
        {
            _ = _securityService.LogoutAsync(userId);
        }

        _securityContext.ClearCurrentUser();
        CurrentUsername = null;
        CurrentRoleDisplay = null;
        IsAuthenticated = false;
        IsMainContentVisible = false;
        IsLoginVisible = true;
        ActiveNavItem = string.Empty;
        IsTimeoutWarningVisible = false;
    }

    private void OnSessionTimerTick(object? sender, ElapsedEventArgs e)
    {
        _secondsUntilTimeout--;
        if (_secondsUntilTimeout <= 0)
        {
            _sessionTimer.Stop();
            // Auto-logout on timeout — SWR-CS-075
            LogoutCommand.Execute(null);
            return;
        }

        if (_secondsUntilTimeout <= TimeoutWarningSeconds)
        {
            IsTimeoutWarningVisible = true;
            SessionTimeoutCountdown = _secondsUntilTimeout;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _sessionTimer.Dispose();
    }
}
