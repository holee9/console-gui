using System.Collections.Generic;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.UI.Contracts.Navigation;
using HnVue.UI.Contracts.ViewModels;

namespace HnVue.UI.ViewModels;

/// <summary>
/// ViewModel for the main shell window.
/// Manages navigation state and the currently active view.
/// </summary>
public sealed partial class MainViewModel : ObservableObject, IMainViewModel, IDisposable
{
    private const int SessionTimeoutMinutes = 15;
    private const int TimeoutWarningSeconds = 180; // warn at 3 minutes remaining

    private readonly ISecurityContext _securityContext;
    private readonly ISecurityService _securityService;
    // @MX:NOTE Session timeout uses System.Timers.Timer (thread-safe, not UI thread); requires Dispatcher.Invoke for UI updates
    private readonly System.Timers.Timer _sessionTimer;
    private int _secondsUntilTimeout;

    // Navigation state — DESIGN_PLAN_v2.md
    private readonly Stack<NavigationToken> _navigationStack = new();
    private NavigationToken _currentToken;

    /// <summary>Gets the ViewModel for the patient list panel.</summary>
    public IPatientListViewModel PatientListViewModel { get; }

    /// <summary>Gets the ViewModel for the image viewer panel.</summary>
    public IImageViewerViewModel ImageViewerViewModel { get; }

    /// <summary>Gets the ViewModel for the workflow/exposure panel.</summary>
    public IWorkflowViewModel WorkflowViewModel { get; }

    /// <summary>Gets the ViewModel for the dose display panel.</summary>
    public IDoseDisplayViewModel DoseDisplayViewModel { get; }

    /// <summary>Gets the ViewModel for the CD/DVD burn panel.</summary>
    public ICDBurnViewModel CDBurnViewModel { get; }

    /// <summary>Gets the ViewModel for the system administration panel.</summary>
    public ISystemAdminViewModel SystemAdminViewModel { get; }

    /// <summary>Gets the ViewModel for the study list full-screen panel.</summary>
    public IStudylistViewModel StudylistViewModel { get; }

    /// <summary>Gets the ViewModel for the Sync Study (merge) full-screen panel.</summary>
    public IMergeViewModel MergeViewModel { get; }

    /// <summary>Gets the ViewModel for the settings full-screen panel.</summary>
    public ISettingsViewModel SettingsViewModel { get; }

    /// <summary>
    /// Gets a value indicating whether an operation is in progress.
    /// The shell itself has no loading state; this always returns <see langword="false"/>.
    /// </summary>
    public bool IsLoading => false;

    /// <summary>
    /// Gets the current error message.
    /// The shell itself does not surface errors; this always returns <see langword="null"/>.
    /// </summary>
    public string? ErrorMessage => null;

    /// <summary>Initialises a new instance of <see cref="MainViewModel"/>.</summary>
    /// <remarks>Issue #17 — CDBurnViewModel and SystemAdminViewModel added to navigation graph.</remarks>
    /// <remarks>Issue #29 — ISecurityService injected for logout audit logging.</remarks>
    /// <remarks>DESIGN_PLAN_v2.md — Studylist, Merge, Settings ViewModels added; NavigateTo/NavigateBack shell API.</remarks>
    public MainViewModel(
        ISecurityContext securityContext,
        ISecurityService securityService,
        IPatientListViewModel patientListViewModel,
        IImageViewerViewModel imageViewerViewModel,
        IWorkflowViewModel workflowViewModel,
        IDoseDisplayViewModel doseDisplayViewModel,
        ICDBurnViewModel cdburnViewModel,
        ISystemAdminViewModel systemAdminViewModel,
        IStudylistViewModel studylistViewModel,
        IMergeViewModel mergeViewModel,
        ISettingsViewModel settingsViewModel)
    {
        ArgumentNullException.ThrowIfNull(securityContext, nameof(securityContext));
        ArgumentNullException.ThrowIfNull(securityService, nameof(securityService));
        ArgumentNullException.ThrowIfNull(patientListViewModel, nameof(patientListViewModel));
        ArgumentNullException.ThrowIfNull(imageViewerViewModel, nameof(imageViewerViewModel));
        ArgumentNullException.ThrowIfNull(workflowViewModel, nameof(workflowViewModel));
        ArgumentNullException.ThrowIfNull(doseDisplayViewModel, nameof(doseDisplayViewModel));
        ArgumentNullException.ThrowIfNull(cdburnViewModel, nameof(cdburnViewModel));
        ArgumentNullException.ThrowIfNull(systemAdminViewModel, nameof(systemAdminViewModel));
        ArgumentNullException.ThrowIfNull(studylistViewModel, nameof(studylistViewModel));
        ArgumentNullException.ThrowIfNull(mergeViewModel, nameof(mergeViewModel));
        ArgumentNullException.ThrowIfNull(settingsViewModel, nameof(settingsViewModel));
        _securityContext = securityContext;
        _securityService = securityService;
        PatientListViewModel = patientListViewModel;
        ImageViewerViewModel = imageViewerViewModel;
        WorkflowViewModel = workflowViewModel;
        DoseDisplayViewModel = doseDisplayViewModel;
        CDBurnViewModel = cdburnViewModel;
        SystemAdminViewModel = systemAdminViewModel;
        StudylistViewModel = studylistViewModel;
        MergeViewModel = mergeViewModel;
        SettingsViewModel = settingsViewModel;

        // Session timeout timer — SWR-CS-075 / Issue #14
        _secondsUntilTimeout = SessionTimeoutMinutes * 60;
        _sessionTimer = new System.Timers.Timer(1000);
        _sessionTimer.Elapsed += OnSessionTimerTick;
        _sessionTimer.AutoReset = true;
    }

    /// <summary>Gets or sets the ViewModel currently displayed in the main content region.</summary>
    [ObservableProperty]
    private object? _currentView;

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

    // @MX:ANCHOR OnLoginSuccess - @MX:REASON: Called by MainWindow after authentication; transitions UI to main content
    /// <summary>Handles a successful login event and transitions to the main content.</summary>
    /// <param name="user">The authenticated user.</param>
    public void OnLoginSuccess(Common.Models.AuthenticatedUser user)
    {
        CurrentUsername = user.Username;
        IsLoginVisible = false;
        IsMainContentVisible = true;
        RefreshFromContext();
        ResetSessionTimer();
        _sessionTimer.Start();
        NavigateTo(NavigationToken.PatientList);
    }

    // @MX:ANCHOR NavigateTo - @MX:REASON: Shell navigation entry point; all view switches go through here
    /// <inheritdoc/>
    public void NavigateTo(NavigationToken token, object? parameter = null)
    {
        if (_currentToken != token || CurrentView is null)
        {
            _navigationStack.Push(_currentToken);
            _currentToken = token;
        }

        CurrentView = token switch
        {
            NavigationToken.PatientList  => PatientListViewModel,
            NavigationToken.Workflow     => WorkflowViewModel,
            NavigationToken.ImageViewer  => ImageViewerViewModel,
            NavigationToken.DoseDisplay  => DoseDisplayViewModel,
            NavigationToken.CDBurn       => CDBurnViewModel,
            NavigationToken.SystemAdmin  => SystemAdminViewModel,
            NavigationToken.Studylist    => StudylistViewModel,
            NavigationToken.Merge        => MergeViewModel,
            NavigationToken.Settings     => SettingsViewModel,
            _                            => CurrentView
        };

        ActiveNavItem = token.ToString();
    }

    /// <inheritdoc/>
    public void NavigateBack()
    {
        if (_navigationStack.Count == 0) return;
        var previous = _navigationStack.Pop();
        _currentToken = previous;
        CurrentView = previous switch
        {
            NavigationToken.PatientList  => PatientListViewModel,
            NavigationToken.Workflow     => WorkflowViewModel,
            NavigationToken.ImageViewer  => ImageViewerViewModel,
            NavigationToken.DoseDisplay  => DoseDisplayViewModel,
            NavigationToken.CDBurn       => CDBurnViewModel,
            NavigationToken.SystemAdmin  => SystemAdminViewModel,
            NavigationToken.Studylist    => StudylistViewModel,
            NavigationToken.Merge        => MergeViewModel,
            NavigationToken.Settings     => SettingsViewModel,
            _                            => CurrentView
        };
        ActiveNavItem = _currentToken.ToString();
    }

    /// <inheritdoc/>
    public IReadOnlyList<NavigationToken> NavigationHistory =>
        _navigationStack.ToArray();

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

    // @MX:NOTE Emergency patient registration view not yet implemented.
//          Tracked via SWR-NF-UX-026 (Safety-Critical, HAZ-RAD) and Issue #11.
    //          Current behavior: sets ActiveNavItem = "Emergency" so the shell highlights
    //          the emergency sidebar item; NavigateTo(NavigationToken.Emergency) wiring will
    //          be added once the Emergency registration view ships (PPT slide TBD).
    /// <summary>
    /// Initiates emergency patient registration workflow.
    /// SWR-NF-UX-026 (Safety-Critical, HAZ-RAD). Issue #11.
    /// </summary>
    [RelayCommand]
    private void Emergency()
    {
        ActiveNavItem = "Emergency";
        // @MX:NOTE Wire to INavigationService.NavigateTo(NavigationToken.Emergency) once the
        //          emergency patient registration view is implemented. See SWR-NF-UX-026 / Issue #11.
        //          BLOCKED: Emergency view not yet created (PPT slide TBD).
    }

    // @MX:ANCHOR Logout - @MX:REASON: Session termination with audit logging; critical security operation
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
