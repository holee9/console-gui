using FluentAssertions;
using HnVue.CDBurning;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dicom;
using HnVue.Dose;
using HnVue.Imaging;
using HnVue.PatientManagement;
using HnVue.Security;
using HnVue.SystemAdmin;
using HnVue.UI.Contracts.Navigation;
using HnVue.UI.Contracts.ViewModels;
using HnVue.UI.ViewModels;
using HnVue.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HnVue.IntegrationTests;

/// <summary>
/// Coordinator-specific integration tests that verify DI composition, navigation flows,
/// and cross-ViewModel interactions managed by the Coordinator team.
/// SWR-COORD-010: DI container resolves all ViewModels successfully.
/// SWR-COORD-020: Navigation flows update MainViewModel CurrentView correctly.
/// SWR-COORD-030: Authentication state propagates through MainViewModel.
/// SWR-COORD-040: Workflow state reflects in ViewModel.
/// SWR-COORD-050: Dose validation results surface in DoseDisplayViewModel.
/// </summary>
public sealed class CoordinatorIntegrationTests
{
    // ── Shared test data ─────────────────────────────────────────────────────

    private static readonly JwtOptions TestJwtOptions = new()
    {
        SecretKey = "CoordinatorIntegrationTestKey-32CharMin!",
        ExpiryMinutes = 15,
        Issuer = "HnVue",
        Audience = "HnVue",
    };

    private static readonly IOptions<AuditOptions> TestAuditOptions =
        Options.Create(new AuditOptions { HmacKey = "CoordinatorTestHmacKey-32CharMin!" });

    // ── Scenario 1: DI Resolve All ViewModels ────────────────────────────────

    /// <summary>
    /// Integration test: Build a DI container similar to App.xaml.cs and resolve
    /// all I*ViewModel interfaces. Verifies DI registration completeness.
    /// SWR-COORD-010: All ViewModels resolve successfully from DI container.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-010")]
    public void DI_ResolveAllViewModels_AllCreatedSuccessfully()
    {
        // Arrange — build service collection with NSubstitute stubs for repos
        var services = new ServiceCollection();
        SetupMinimalServices(services);
        var provider = services.BuildServiceProvider();

        // Act — resolve each ViewModel
        var mainViewModel = provider.GetService<IMainViewModel>();
        var loginViewModel = provider.GetService<ILoginViewModel>();
        var patientListViewModel = provider.GetService<IPatientListViewModel>();
        var workflowViewModel = provider.GetService<IWorkflowViewModel>();
        var doseDisplayViewModel = provider.GetService<IDoseDisplayViewModel>();
        var cdburnViewModel = provider.GetService<ICDBurnViewModel>();
        var systemAdminViewModel = provider.GetService<ISystemAdminViewModel>();
        var studylistViewModel = provider.GetService<IStudylistViewModel>();
        var mergeViewModel = provider.GetService<IMergeViewModel>();
        var settingsViewModel = provider.GetService<ISettingsViewModel>();
        var imageViewerViewModel = provider.GetService<IImageViewerViewModel>();
        var doseViewModel = provider.GetService<IDoseViewModel>();
        var quickPinLockViewModel = provider.GetService<IQuickPinLockViewModel>();
        var addPatientProcedureViewModel = provider.GetService<IAddPatientProcedureViewModel>();

        // Assert — all ViewModels resolved successfully
        mainViewModel.Should().NotBeNull("MainViewModel must be resolvable from DI container");
        loginViewModel.Should().NotBeNull("LoginViewModel must be resolvable");
        patientListViewModel.Should().NotBeNull("PatientListViewModel must be resolvable");
        workflowViewModel.Should().NotBeNull("WorkflowViewModel must be resolvable");
        doseDisplayViewModel.Should().NotBeNull("DoseDisplayViewModel must be resolvable");
        cdburnViewModel.Should().NotBeNull("CDBurnViewModel must be resolvable");
        systemAdminViewModel.Should().NotBeNull("SystemAdminViewModel must be resolvable");
        studylistViewModel.Should().NotBeNull("StudylistViewModel must be resolvable");
        mergeViewModel.Should().NotBeNull("MergeViewModel must be resolvable");
        settingsViewModel.Should().NotBeNull("SettingsViewModel must be resolvable");
        imageViewerViewModel.Should().NotBeNull("ImageViewerViewModel must be resolvable");
        doseViewModel.Should().NotBeNull("DoseViewModel must be resolvable");
        quickPinLockViewModel.Should().NotBeNull("QuickPinLockViewModel must be resolvable");
        addPatientProcedureViewModel.Should().NotBeNull("AddPatientProcedureViewModel must be resolvable from DI container");

        // Assert — NavigationService also resolves
        var navService = provider.GetService<INavigationService>();
        navService.Should().NotBeNull("NavigationService must be resolvable");
    }

    // ── Scenario 2: Navigation → MainViewModel CurrentView ───────────────────

    /// <summary>
    /// Integration test: NavigateTo updates MainViewModel.CurrentView and
    /// ActiveNavItem reflects the navigation target.
    /// SWR-COORD-020: Navigation flows correctly update shell view state.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-020")]
    public void Navigation_NavigateToWorkflow_UpdatesMainViewModelCurrentView()
    {
        // Arrange — create MainViewModel and NavigationService
        var services = new ServiceCollection();
        SetupMinimalServices(services);
        var provider = services.BuildServiceProvider();

        var mainViewModel = provider.GetRequiredService<IMainViewModel>();
        var navService = provider.GetRequiredService<INavigationService>();

        // Act — navigate to Workflow
        navService.NavigateTo(NavigationToken.Workflow);

        // Assert
        mainViewModel.CurrentView.Should().NotBeNull("CurrentView must be set after navigation");
        mainViewModel.CurrentView.Should().BeOfType<WorkflowViewModel>("CurrentView should be WorkflowViewModel");
    }

    /// <summary>
    /// Integration test: NavigateTo PatientList updates ActiveNavItem string.
    /// SWR-COORD-020: ActiveNavItem reflects current navigation target.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-020")]
    public void Navigation_NavigateToPatientList_UpdatesActiveNavItem()
    {
        // Arrange
        var services = new ServiceCollection();
        SetupMinimalServices(services);
        var provider = services.BuildServiceProvider();

        var mainViewModel = provider.GetRequiredService<IMainViewModel>();
        var navService = provider.GetRequiredService<INavigationService>();

        // Act
        navService.NavigateTo(NavigationToken.PatientList);

        // Assert
        mainViewModel.ActiveNavItem.Should().Be("PatientList",
            "ActiveNavItem must reflect the current navigation target");
    }

    // ── Scenario 3: Navigation History + GoBack ─────────────────────────────

    /// <summary>
    /// Integration test: Multiple navigations build history, and GoBack
    /// reverses through the stack.
    /// SWR-COORD-020: Navigation history is maintained and GoBack restores previous views.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-020")]
    public void Navigation_MultipleNavigations_GoBackReturnsToPrevious()
    {
        // Arrange
        var services = new ServiceCollection();
        SetupMinimalServices(services);
        var provider = services.BuildServiceProvider();

        var mainViewModel = provider.GetRequiredService<IMainViewModel>();
        var navService = provider.GetRequiredService<INavigationService>();

        // Act — navigate forward through multiple screens
        navService.NavigateTo(NavigationToken.PatientList);
        navService.NavigateTo(NavigationToken.Workflow);
        navService.NavigateTo(NavigationToken.Settings);

        // Assert — navigation history has entries
        mainViewModel.NavigationHistory.Should().NotBeEmpty("NavigationHistory must contain previous views");
        mainViewModel.NavigationHistory.Count.Should().BeGreaterOrEqualTo(2,
            "History should have at least 2 entries after 3 navigations");

        // Act — go back twice
        var firstBackResult = navService.GoBack();
        var secondBackResult = navService.GoBack();

        // Assert
        firstBackResult.Should().BeTrue("First GoBack should succeed");
        secondBackResult.Should().BeTrue("Second GoBack should succeed");

        mainViewModel.CurrentView.Should().BeOfType<PatientListViewModel>(
            "After going back twice from Settings, we should be at PatientList");
    }

    /// <summary>
    /// Integration test: GoBack returns false when navigation history is empty.
    /// SWR-COORD-020: Cannot go back when history is empty.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-020")]
    public void Navigation_EmptyHistory_GoBackReturnsFalse()
    {
        // Arrange
        var services = new ServiceCollection();
        SetupMinimalServices(services);
        var provider = services.BuildServiceProvider();

        var navService = provider.GetRequiredService<INavigationService>();

        // Act — try to go back without any navigation history
        var result = navService.GoBack();

        // Assert
        result.Should().BeFalse("GoBack must return false when navigation history is empty");
    }

    // ── Scenario 4: Login → MainViewModel Auth State ─────────────────────────

    /// <summary>
    /// Integration test: Authenticate via SecurityService, convert to AuthenticatedUser,
    /// call OnLoginSuccess, and verify MainViewModel reflects authenticated state.
    /// SWR-COORD-030: Authentication state propagates through MainViewModel.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-030")]
    public async Task Login_AuthenticateAndOnLoginSuccess_MainViewModelReflectsAuthenticatedState()
    {
        // Arrange — create shared ISecurityContext that both SecurityService and MainViewModel use
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var sharedSecContext = Substitute.For<ISecurityContext>();

        const string password = "TestPass1";
        var user = MakeUser(password: password, role: UserRole.Radiographer);

        userRepo.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        userRepo.UpdateFailedLoginCountAsync(user.UserId, 0, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        userRepo.GetByIdAsync(user.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string?>(ErrorCode.NotFound, "empty log"));
        auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Configure the mock secContext to reflect authenticated state after SetCurrentUser
        sharedSecContext.IsAuthenticated.Returns(false);
        sharedSecContext.When(x => x.SetCurrentUser(Arg.Any<AuthenticatedUser>()))
            .Do(ci =>
            {
                var user = ci.Arg<AuthenticatedUser>();
                sharedSecContext.IsAuthenticated.Returns(true);
                sharedSecContext.CurrentUsername.Returns(user.Username);
                sharedSecContext.CurrentRole.Returns(user.Role);
            });

        var secService = new SecurityService(
            userRepo, auditRepo, sharedSecContext, TestJwtOptions, TestAuditOptions,
            Substitute.For<ITokenDenylist>());

        var services = new ServiceCollection();
        SetupMinimalServices(services, overrideSecurityService: secService, overrideSecContext: sharedSecContext);
        var provider = services.BuildServiceProvider();

        var mainViewModel = provider.GetRequiredService<IMainViewModel>();

        // Act — authenticate (returns AuthenticationToken)
        var authResult = await secService.AuthenticateAsync(user.Username, password);
        authResult.IsSuccess.Should().BeTrue("Authentication should succeed");

        // Act — convert AuthenticationToken → AuthenticatedUser and call OnLoginSuccess
        var authUser = new AuthenticatedUser(
            authResult.Value.UserId,
            authResult.Value.Username,
            authResult.Value.Role,
            authResult.Value.Jti);
        mainViewModel.OnLoginSuccess(authUser);

        // Assert — MainViewModel reflects authenticated state
        mainViewModel.IsAuthenticated.Should().BeTrue("IsAuthenticated must be true after login");
        mainViewModel.CurrentUsername.Should().Be(user.Username, "CurrentUsername must match authenticated user");
    }

    /// <summary>
    /// Integration test: OnLoginSuccess transitions UI to main content and hides login.
    /// SWR-COORD-030: Login transitions shell to authenticated state.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-030")]
    public void Login_OnLoginSuccess_TransitionsToMainContentAndPatientList()
    {
        // Arrange
        var services = new ServiceCollection();
        SetupMinimalServices(services);
        var provider = services.BuildServiceProvider();

        var mainViewModel = provider.GetRequiredService<IMainViewModel>();
        var authUser = new AuthenticatedUser(
            UserId: "user-123",
            Username: "testuser",
            Role: UserRole.Admin);

        // Act — call OnLoginSuccess
        mainViewModel.OnLoginSuccess(authUser);

        // Assert
        mainViewModel.IsLoginVisible.Should().BeFalse("Login view must be hidden after login");
        mainViewModel.IsMainContentVisible.Should().BeTrue("Main content must be visible after login");
    }

    // ── Scenario 5: Workflow Start with Patient ──────────────────────────────

    /// <summary>
    /// Integration test: StartAsync on WorkflowEngine updates WorkflowViewModel state.
    /// SWR-COORD-040: Workflow state changes are reflected in WorkflowViewModel.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-040")]
    public async Task Workflow_StartAsyncWithPatient_UpdatesWorkflowState()
    {
        // Arrange — create real WorkflowEngine with GeneratorSimulator
        var doseService = Substitute.For<IDoseService>();
        var generator = new GeneratorSimulator { PrepareDelayMs = 0, ExposureDelayMs = 0 };
        var secContext = Substitute.For<ISecurityContext>();
        secContext.CurrentRole.Returns(UserRole.Radiographer);

        var engine = new WorkflowEngine(doseService, generator, secContext);
        var workflowViewModel = new WorkflowViewModel(engine, secContext);

        // Act — start workflow with patient
        var result = await engine.StartAsync("P-001", "1.2.3.4.5");

        // Assert
        result.IsSuccess.Should().BeTrue("StartAsync should succeed");
        engine.CurrentState.Should().Be(WorkflowState.PatientSelected,
            "Engine should be in PatientSelected state after StartAsync");

        // Assert — ViewModel reflects the state (via StateChanged event)
        workflowViewModel.CurrentState.Should().Be("PatientSelected",
            "WorkflowViewModel must reflect the engine's current state");
    }

    /// <summary>
    /// Integration test: Multiple state transitions update WorkflowViewModel.
    /// SWR-COORD-040: WorkflowViewModel tracks all state transitions.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-040")]
    public async Task Workflow_MultipleTransitions_ViewModelTracksAllStates()
    {
        // Arrange
        var doseService = Substitute.For<IDoseService>();
        var generator = new GeneratorSimulator { PrepareDelayMs = 0, ExposureDelayMs = 0 };
        var secContext = Substitute.For<ISecurityContext>();
        secContext.CurrentRole.Returns(UserRole.Radiographer);

        var engine = new WorkflowEngine(doseService, generator, secContext);
        var workflowViewModel = new WorkflowViewModel(engine, secContext);

        // Act — drive through multiple states
        await engine.StartAsync("P-002", "1.2.3.4.6");
        await engine.TransitionAsync(WorkflowState.ProtocolLoaded);
        await engine.TransitionAsync(WorkflowState.ReadyToExpose);

        // Assert
        workflowViewModel.CurrentState.Should().Be("ReadyToExpose",
            "ViewModel should reflect the final state after multiple transitions");
        workflowViewModel.IsExposureReady.Should().BeTrue("IsExposureReady should be true in ReadyToExpose state");
    }

    // ── Scenario 6: Dose Validation → DoseDisplayViewModel ───────────────────

    /// <summary>
    /// Integration test: DoseService validates elevated exposure, and
    /// DoseDisplayViewModel surfaces the warning through CurrentDoseDap.
    /// SWR-COORD-050: Dose validation results are exposed via DoseDisplayViewModel.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-050")]
    public async Task Dose_ValidateExposure_ElevatedDoseTriggersAlert()
    {
        // Arrange — create DoseService and DoseDisplayViewModel
        var doseRepo = Substitute.For<IDoseRepository>();
        var doseService = new DoseService(doseRepo);
        var doseDisplayViewModel = new DoseDisplayViewModel(doseService);

        // Arrange — elevated exposure parameters (Warn level: DAP between 10 and 20 for CHEST)
        var parameters = new ExposureParameters(
            BodyPart: "CHEST",
            Kvp: 250.0,
            Mas: 100.0,
            StudyInstanceUid: "1.2.3.4.5.001");

        // Act — validate exposure
        var result = await doseService.ValidateExposureAsync(parameters);

        // Assert
        result.IsSuccess.Should().BeTrue("Validation should succeed");
        result.Value.Level.Should().Be(DoseValidationLevel.Warn, "Should be Warn level for elevated exposure");
        result.Value.IsAllowed.Should().BeTrue("Warn level should still allow exposure");

        // Act — set elevated dose in ViewModel (above default DRL of 150.0)
        doseDisplayViewModel.CurrentDoseDap = 200.0;

        // Assert — ViewModel shows alert
        doseDisplayViewModel.IsDoseAlert.Should().BeTrue("Elevated dose should trigger alert");
        doseDisplayViewModel.DrlPercentage.Should().Be(100.0, "DRL percentage is capped at 100% when dose exceeds DRL");
    }

    /// <summary>
    /// Integration test: Normal exposure does not trigger dose alert.
    /// SWR-COORD-050: Normal dose levels do not trigger alerts.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-050")]
    public async Task Dose_ValidateExposure_NormalDoseDoesNotTriggerAlert()
    {
        // Arrange
        var doseRepo = Substitute.For<IDoseRepository>();
        var doseService = new DoseService(doseRepo);
        var doseDisplayViewModel = new DoseDisplayViewModel(doseService);

        // Arrange — normal exposure parameters (Allow level)
        var parameters = new ExposureParameters(
            BodyPart: "CHEST",
            Kvp: 60.0,
            Mas: 2.0,
            StudyInstanceUid: "1.2.3.4.5.002");

        // Act
        var result = await doseService.ValidateExposureAsync(parameters);

        // Assert
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
        result.Value.IsAllowed.Should().BeTrue("Normal exposure should be allowed");

        // Act — set normal dose in ViewModel (below default DRL of 150.0)
        doseDisplayViewModel.CurrentDoseDap = 50.0;

        // Assert
        doseDisplayViewModel.IsDoseAlert.Should().BeFalse("Normal dose should not trigger alert");
        doseDisplayViewModel.DrlPercentage.Should().BeLessThan(100, "DRL percentage should be below 100%");
    }

    // ── Scenario 7: Settings ViewModel Composition ───────────────────────────

    /// <summary>
    /// Integration test: SettingsViewModel exposes expected tabs for configuration.
    /// SWR-COORD-060: SettingsViewModel provides all required configuration tabs.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-060")]
    public void Settings_ViewModel_ContainsExpectedTabs()
    {
        // Arrange — create SettingsViewModel (parameterless constructor)
        var settingsViewModel = new SettingsViewModel();

        // Act — get tabs collection
        var tabs = settingsViewModel.Tabs;

        // Assert — verify expected tabs are present
        tabs.Should().NotBeEmpty("SettingsViewModel must have tabs");
        tabs.Should().Contain("System", "System tab must be present");
        tabs.Should().Contain("Account", "Account tab must be present");
        tabs.Should().Contain("Detector", "Detector tab must be present");
        tabs.Should().Contain("Generator", "Generator tab must be present");
        tabs.Should().Contain("Network", "Network tab must be present (merged PACS+Worklist+Print)");
        tabs.Should().Contain("Display", "Display tab must be present");
        tabs.Should().Contain("Option", "Option tab must be present");
        tabs.Should().Contain("Database", "Database tab must be present");
        tabs.Should().Contain("DicomSet", "DicomSet tab must be present");
        tabs.Should().Contain("RIS Code", "RIS Code tab must be present");
    }

    /// <summary>
    /// Integration test: SettingsViewModel exposes role options for account creation.
    /// SWR-COORD-060: Role options are available in SettingsViewModel.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-060")]
    public void Settings_ViewModel_ContainsRoleOptions()
    {
        // Arrange
        var settingsViewModel = new SettingsViewModel();

        // Act
        var availableRoles = settingsViewModel.AvailableRoles;

        // Assert
        availableRoles.Should().NotBeEmpty("AvailableRoles must not be empty");
        availableRoles.Should().Contain("Admin", "Admin role must be available");
        availableRoles.Should().Contain("Technician", "Technician role must be available");
        availableRoles.Should().Contain("Radiologist", "Radiologist role must be available");
    }

    // ── Scenario 8: StudylistViewModel Composition ────────────────────────────

    /// <summary>
    /// Integration test: StudylistViewModel resolves from DI and has expected default values.
    /// SWR-COORD-070: StudylistViewModel DI resolution and initial state.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-070")]
    public void Studylist_ResolveFromDI_HasExpectedDefaults()
    {
        // Arrange
        var services = new ServiceCollection();
        SetupMinimalServices(services);
        var provider = services.BuildServiceProvider();

        // Act
        var vm = provider.GetService<IStudylistViewModel>();

        // Assert — resolved successfully
        vm.Should().NotBeNull("StudylistViewModel must be resolvable from DI");

        // Assert — default values
        vm!.ActivePeriodFilter.Should().Be("All", "Default period filter should be 'All'");
        vm.PacsServers.Should().NotBeEmpty("PACS server list should not be empty");
        vm.PacsServers.Should().Contain("LOCAL", "LOCAL PACS server must be available");
        vm.Studies.Should().NotBeNull("Studies collection should be initialized");
        vm.Studies.Should().BeEmpty("Studies should be empty initially");
        vm.SearchQuery.Should().BeEmpty("Search query should be empty initially");
    }

    /// <summary>
    /// Integration test: StudylistViewModel property changes propagate correctly.
    /// SWR-COORD-070: StudylistViewModel property binding works for UI.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-070")]
    public void Studylist_PropertyChanges_PropagateCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        SetupMinimalServices(services);
        var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<IStudylistViewModel>();

        // Act — change period filter
        vm.ActivePeriodFilter = "Today";

        // Assert
        vm.ActivePeriodFilter.Should().Be("Today", "Period filter should update to 'Today'");

        // Act — change search query
        vm.SearchQuery = "CHEST";

        // Assert
        vm.SearchQuery.Should().Be("CHEST", "Search query should update");

        // Act — change PACS server selection
        vm.SelectedPacsServer = "PACS-01";

        // Assert
        vm.SelectedPacsServer.Should().Be("PACS-01", "PACS server selection should update");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Sets up a minimal DI container for integration testing.
    /// Registers repositories as NSubstitute substitutes and real service implementations.
    /// Complex services (SWUpdateService) are mocked at the interface level.
    /// </summary>
    private static void SetupMinimalServices(
        ServiceCollection services,
        ISecurityService? overrideSecurityService = null,
        ISecurityContext? overrideSecContext = null)
    {
        // Register repositories (NSubstitute mocks)
        services.AddSingleton(Substitute.For<IUserRepository>());
        services.AddSingleton(Substitute.For<IAuditRepository>());
        services.AddSingleton(Substitute.For<IPatientRepository>());
        services.AddSingleton(Substitute.For<IDoseRepository>());
        services.AddSingleton(Substitute.For<HnVue.Common.Abstractions.IStudyRepository>());
        services.AddSingleton(Substitute.For<IWorklistRepository>());
        services.AddSingleton(Substitute.For<ISystemSettingsRepository>());
        services.AddSingleton(Substitute.For<HnVue.CDBurning.IStudyRepository>());

        // Register infrastructure — shared secContext for SecurityService + MainViewModel
        var secContext = overrideSecContext ?? Substitute.For<ISecurityContext>();
        services.AddSingleton(secContext);

        var tokenDenylist = Substitute.For<ITokenDenylist>();
        services.AddSingleton(tokenDenylist);

        services.AddSingleton(TestJwtOptions);        // JwtOptions directly (SecurityService takes it directly)
        services.AddSingleton(TestAuditOptions);       // IOptions<AuditOptions>

        // Register services — real where possible, mocked for complex deps
        if (overrideSecurityService != null)
        {
            services.AddSingleton(overrideSecurityService);
        }
        else
        {
            services.AddSingleton<ISecurityService, SecurityService>();
        }

        services.AddSingleton<IPatientService, PatientService>();
        services.AddSingleton<IDoseService, DoseService>();
        services.AddSingleton<IWorklistService, WorklistService>();
        services.AddSingleton<ISystemAdminService, SystemAdminService>();

        // SWUpdateService needs IHttpClientFactory + IOptions<UpdateOptions> — mock at interface level
        services.AddSingleton(Substitute.For<ISWUpdateService>());

        // Register imaging services
        services.AddSingleton(Substitute.For<IImageProcessor>());

        // Register workflow services (real engine + simulator)
        var doseServiceForWorkflow = Substitute.For<IDoseService>();
        var generator = new GeneratorSimulator { PrepareDelayMs = 0, ExposureDelayMs = 0 };
        services.AddSingleton(doseServiceForWorkflow);
        services.AddSingleton<IGeneratorInterface>(generator);
        services.AddSingleton<IWorkflowEngine, WorkflowEngine>();

        // Register CD burning
        services.AddSingleton(Substitute.For<IBurnSession>());
        services.AddSingleton<ICDDVDBurnService, CDDVDBurnService>();

        // Register ViewModels
        services.AddSingleton<IStudylistViewModel, StudylistViewModel>();
        services.AddSingleton<IMergeViewModel, MergeViewModel>();
        services.AddSingleton<ISettingsViewModel, SettingsViewModel>();
        services.AddSingleton<IImageViewerViewModel, ImageViewerViewModel>();
        services.AddSingleton<IDoseViewModel, DoseViewModel>();
        services.AddSingleton<ILoginViewModel, LoginViewModel>();
        services.AddSingleton<IPatientListViewModel, PatientListViewModel>();
        services.AddSingleton<IWorkflowViewModel, WorkflowViewModel>();
        services.AddSingleton<IDoseDisplayViewModel, DoseDisplayViewModel>();
        services.AddSingleton<ICDBurnViewModel, CDBurnViewModel>();
        services.AddSingleton<ISystemAdminViewModel, SystemAdminViewModel>();
        services.AddSingleton<IQuickPinLockViewModel, QuickPinLockViewModel>();
        services.AddSingleton<IAddPatientProcedureViewModel, AddPatientProcedureViewModel>();
        services.AddSingleton<IMainViewModel, MainViewModel>();

        // Register NavigationService (inline implementation)
        services.AddSingleton<INavigationService, TestNavigationService>();
    }

    /// <summary>
    /// Creates a test user with a hashed password.
    /// </summary>
    private static UserRecord MakeUser(
        string? userId = null,
        string? username = null,
        string password = "Password1",
        UserRole role = UserRole.Radiographer,
        bool isLocked = false,
        int failedLoginCount = 0)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 4);
        return new UserRecord(
            UserId: userId ?? Guid.NewGuid().ToString(),
            Username: username ?? $"testuser_{Guid.NewGuid():N}",
            DisplayName: "Test User",
            PasswordHash: hash,
            Role: role,
            FailedLoginCount: failedLoginCount,
            IsLocked: isLocked,
            LastLoginAt: null);
    }

    /// <summary>
    /// Test implementation of INavigationService that delegates to IMainViewModel.
    /// Mirrors the real NavigationService from HnVue.App.Services without requiring
    /// a project reference to HnVue.App.
    /// </summary>
    private sealed class TestNavigationService : INavigationService
    {
        private readonly IMainViewModel _shell;

        public TestNavigationService(IMainViewModel shell)
        {
            ArgumentNullException.ThrowIfNull(shell);
            _shell = shell;
        }

        public bool CanGoBack => _shell.NavigationHistory.Count > 0;

        public event EventHandler<NavigationToken>? Navigated;

        public void NavigateTo(NavigationToken token) => NavigateTo(token, null);

        public void NavigateTo(NavigationToken token, object? parameter)
        {
            _shell.NavigateTo(token, parameter);
            Navigated?.Invoke(this, token);
        }

        public bool GoBack()
        {
            if (!CanGoBack) return false;
            _shell.NavigateBack();
            return true;
        }
    }
}
