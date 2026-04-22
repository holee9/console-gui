using FluentAssertions;
using HnVue.CDBurning;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Detector;
using HnVue.Dicom;
using HnVue.Dose;
using HnVue.Imaging;
using HnVue.PatientManagement;
using HnVue.Security;
using HnVue.SystemAdmin;
using HnVue.UI.Contracts.Models;
using HnVue.UI.Contracts.Navigation;
using HnVue.UI.Contracts.ViewModels;
using HnVue.UI.ViewModels;
using HnVue.UI.ViewModels.Models;
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
        var settingsViewModel = new SettingsViewModel(Substitute.For<ISystemAdminService>());

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
        var settingsViewModel = new SettingsViewModel(Substitute.For<ISystemAdminService>());

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

    // ── Scenario 9: Detector DI Conditional Registration ──────────────────────

    /// <summary>
    /// Integration test: DetectorSimulator resolves from DI container as IDetectorInterface.
    /// SWR-COORD-080: Detector conditional DI registration resolves correctly in dev mode.
    /// </summary>
    [Fact]
    public void Detector_DI_ResolvesDetectorSimulator_WhenNoSdkDllPresent()
    {
        // Arrange — setup DI with DetectorSimulator (dev/test mode, no SDK DLL)
        var services = new ServiceCollection();
        SetupMinimalServices(services);
        services.AddSingleton<IDetectorInterface, DetectorSimulator>();

        var provider = services.BuildServiceProvider();

        // Act — resolve IDetectorInterface
        var detector = provider.GetService<IDetectorInterface>();

        // Assert — DetectorSimulator resolved successfully
        detector.Should().NotBeNull("IDetectorInterface must be resolvable from DI container");
        detector.Should().BeOfType<DetectorSimulator>("DetectorSimulator is the default adapter when no SDK DLL is present");
    }

    /// <summary>
    /// Integration test: DetectorSimulator state transitions work correctly via DI.
    /// SWR-COORD-080: Detector lifecycle (Connect → Arm → Abort) works through DI-resolved instance.
    /// </summary>
    [Fact]
    public async Task Detector_DI_SimulatorLifecycle_WorksThroughDIResolution()
    {
        // Arrange
        var services = new ServiceCollection();
        SetupMinimalServices(services);
        var simulator = new DetectorSimulator { ArmDelayMs = 0, ReadoutDelayMs = 0 };
        services.AddSingleton<IDetectorInterface>(simulator);

        var provider = services.BuildServiceProvider();
        var detector = provider.GetRequiredService<IDetectorInterface>();

        // Act & Assert — Connect
        var connectResult = await detector.ConnectAsync();
        connectResult.IsSuccess.Should().BeTrue("Connect should succeed for simulator");

        // Act & Assert — Arm
        var armResult = await detector.ArmAsync(DetectorTriggerMode.Sync);
        armResult.IsSuccess.Should().BeTrue("Arm should succeed for simulator");

        // Act & Assert — Abort
        var abortResult = await detector.AbortAsync();
        abortResult.IsSuccess.Should().BeTrue("Abort should succeed for simulator");

        // Act & Assert — Disconnect
        var disconnectResult = await detector.DisconnectAsync();
        disconnectResult.IsSuccess.Should().BeTrue("Disconnect should succeed for simulator");
    }

    /// <summary>
    /// Integration test: WorkflowEngine receives IDetectorInterface through DI.
    /// SWR-COORD-080: WorkflowEngine correctly uses injected detector for ARM/Abort operations.
    /// </summary>
    [Fact]
    public async Task Detector_DI_WorkflowEngineReceivesDetector_ThroughDI()
    {
        // Arrange — setup DI with real WorkflowEngine and DetectorSimulator
        var services = new ServiceCollection();
        SetupMinimalServices(services);
        var simulator = new DetectorSimulator { ArmDelayMs = 0, ReadoutDelayMs = 0 };
        services.AddSingleton<IDetectorInterface>(simulator);

        var provider = services.BuildServiceProvider();

        // Act — resolve WorkflowEngine (constructor receives IDetectorInterface)
        var engine = provider.GetRequiredService<IWorkflowEngine>();

        // Start workflow with patient/study IDs
        var secContext = provider.GetRequiredService<ISecurityContext>();
        secContext.SetCurrentUser(new AuthenticatedUser(
            "test-user", "Test User", UserRole.Radiographer, "token"));

        var startResult = await engine.StartAsync("P-001", "ST-001");

        // Assert — WorkflowEngine resolves with detector and starts successfully
        engine.Should().NotBeNull("WorkflowEngine must resolve from DI with detector");
        startResult.IsSuccess.Should().BeTrue("WorkflowEngine should start successfully");
    }

    /// <summary>
    /// Integration test: RegisterDetectorService conditional pattern registers correct type.
    /// SWR-COORD-080: Conditional registration logic selects Simulator when no SDK DLL exists.
    /// Tests the same conditional logic used in App.xaml.cs RegisterDetectorService.
    /// </summary>
    [Fact]
    public void Detector_DI_ConditionalRegistration_SelectsSimulatorInDevMode()
    {
        // Arrange — simulate App.xaml.cs RegisterDetectorService logic
        var services = new ServiceCollection();

        // In test/dev environment, neither AbyzSdk.dll nor libxd2.dll exist
        // This mirrors the "else" branch of RegisterDetectorService
        var sdkPath = System.IO.Path.Combine(AppContext.BaseDirectory, "AbyzSdk.dll");
        var hmePath = System.IO.Path.Combine(AppContext.BaseDirectory, "libxd2.dll");

        if (!System.IO.File.Exists(sdkPath) && !System.IO.File.Exists(hmePath))
        {
            // Dev/test mode — register Simulator (same as App.xaml.cs line 307)
            services.AddSingleton<IDetectorInterface, DetectorSimulator>();
        }

        var provider = services.BuildServiceProvider();
        var detector = provider.GetService<IDetectorInterface>();

        // Assert
        detector.Should().NotBeNull("Conditional registration must register a detector in dev mode");
        detector.Should().BeOfType<DetectorSimulator>(
            "Without SDK DLLs, DetectorSimulator is the correct fallback");
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

    // ── Scenario 10: WorkflowViewModel Full Acquisition Flow ────────────────────

    /// <summary>
    /// Integration test: Full acquisition workflow from patient selection through protocol loading
    /// to exposure-ready state, verifying ViewModel tracks every transition.
    /// SWR-COORD-090: WorkflowViewModel tracks patient → protocol → exposure flow.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-090")]
    public async Task Workflow_FullAcquisitionFlow_ViewModelTracksAllTransitions()
    {
        // Arrange — create real WorkflowEngine with simulators
        var doseService = Substitute.For<IDoseService>();
        var generator = new GeneratorSimulator { PrepareDelayMs = 0, ExposureDelayMs = 0 };
        var secContext = Substitute.For<ISecurityContext>();
        secContext.CurrentRole.Returns(UserRole.Radiographer);
        secContext.HasRole(UserRole.Radiographer).Returns(true);

        var engine = new WorkflowEngine(doseService, generator, secContext);
        using var workflowViewModel = new WorkflowViewModel(engine, secContext);

        // Act — Step 1: Select patient
        var startResult = await engine.StartAsync("P-100", "1.2.3.4.5.100");

        // Assert — Step 1
        startResult.IsSuccess.Should().BeTrue("StartAsync should succeed");
        workflowViewModel.CurrentState.Should().Be("PatientSelected");
        workflowViewModel.WorkflowState.Should().Be(WorkflowState.PatientSelected);
        workflowViewModel.StatusMessage.Should().Be("Patient selected. Load a protocol.");

        // Act — Step 2: Load protocol
        var protocolResult = await engine.TransitionAsync(WorkflowState.ProtocolLoaded);

        // Assert — Step 2
        protocolResult.IsSuccess.Should().BeTrue("Protocol transition should succeed");
        workflowViewModel.CurrentState.Should().Be("ProtocolLoaded");
        workflowViewModel.WorkflowState.Should().Be(WorkflowState.ProtocolLoaded);
        workflowViewModel.IsExposureReady.Should().BeFalse("Not ready to expose yet");

        // Act — Step 3: Prepare for exposure
        var prepareResult = await engine.TransitionAsync(WorkflowState.ReadyToExpose);

        // Assert — Step 3
        prepareResult.IsSuccess.Should().BeTrue("PrepareExposure transition should succeed");
        workflowViewModel.CurrentState.Should().Be("ReadyToExpose");
        workflowViewModel.WorkflowState.Should().Be(WorkflowState.ReadyToExpose);
        workflowViewModel.IsExposureReady.Should().BeTrue("Should be ready to expose");
        workflowViewModel.StatusMessage.Should().Be("Ready to expose. Trigger when clear.");
    }

    /// <summary>
    /// Integration test: WorkflowViewModel SelectedPatient property works for patient info panel.
    /// SWR-COORD-090: WorkflowViewModel binds patient selection correctly.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-090")]
    public void Workflow_SelectedPatient_UpdatesCorrectly()
    {
        // Arrange
        var doseService = Substitute.For<IDoseService>();
        var generator = new GeneratorSimulator { PrepareDelayMs = 0, ExposureDelayMs = 0 };
        var secContext = Substitute.For<ISecurityContext>();
        var engine = new WorkflowEngine(doseService, generator, secContext);
        using var vm = new WorkflowViewModel(engine, secContext);

        var patient = new PatientRecord(
            "P-200", "홍길동", new DateOnly(1985, 3, 15), "M", false,
            DateTimeOffset.UtcNow, "admin-001");

        // Act
        vm.SelectedPatient = patient;

        // Assert
        vm.SelectedPatient.Should().NotBeNull();
        vm.SelectedPatient!.PatientId.Should().Be("P-200");
        vm.SelectedPatient.Name.Should().Be("홍길동");
    }

    /// <summary>
    /// Integration test: WorkflowViewModel ThumbnailList can hold StudyItem instances.
    /// SWR-COORD-090: ThumbnailList ObservableCollection works with IStudyItem.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-090")]
    public void Workflow_ThumbnailList_AcceptsStudyItems()
    {
        // Arrange
        var doseService = Substitute.For<IDoseService>();
        var generator = new GeneratorSimulator { PrepareDelayMs = 0, ExposureDelayMs = 0 };
        var secContext = Substitute.For<ISecurityContext>();
        var engine = new WorkflowEngine(doseService, generator, secContext);
        using var vm = new WorkflowViewModel(engine, secContext);

        var study1 = new StudyItem(new StudyRecord("1.2.3.4.5", "P-001", DateTimeOffset.UtcNow, "CHEST PA", "ACC-001", "CHEST"));
        var study2 = new StudyItem(new StudyRecord("1.2.3.4.6", "P-001", DateTimeOffset.UtcNow, "CHEST LAT", "ACC-002", "CHEST"));

        // Act
        vm.ThumbnailList.Add(study1);
        vm.ThumbnailList.Add(study2);

        // Assert
        vm.ThumbnailList.Should().HaveCount(2);
        vm.ThumbnailList[0].Study.StudyInstanceUid.Should().Be("1.2.3.4.5");
        vm.ThumbnailList[1].Study.StudyInstanceUid.Should().Be("1.2.3.4.6");
    }

    /// <summary>
    /// Integration test: IWorkflowEngine interface contract — invalid transition returns failure.
    /// SWR-COORD-090: Invalid state transitions are rejected with proper error codes.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-090")]
    public async Task Workflow_InvalidTransition_ReturnsFailure()
    {
        // Arrange — engine starts in Idle state
        var doseService = Substitute.For<IDoseService>();
        var generator = new GeneratorSimulator { PrepareDelayMs = 0, ExposureDelayMs = 0 };
        var secContext = Substitute.For<ISecurityContext>();
        var engine = new WorkflowEngine(doseService, generator, secContext);
        using var vm = new WorkflowViewModel(engine, secContext);

        // Act — try to go directly to Exposing from Idle (invalid)
        var result = await engine.TransitionAsync(WorkflowState.Exposing);

        // Assert
        result.IsFailure.Should().BeTrue("Direct Idle→Exposing transition should be rejected");
        engine.CurrentState.Should().Be(WorkflowState.Idle, "State should remain Idle");
        vm.CurrentState.Should().Be("Idle", "ViewModel should still show Idle");
    }

    /// <summary>
    /// Integration test: Workflow abort from any state transitions to Error.
    /// SWR-COORD-090: Abort always available and updates ViewModel.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-090")]
    public async Task Workflow_AbortFromPatientSelected_TransitionsToError()
    {
        // Arrange
        var doseService = Substitute.For<IDoseService>();
        var generator = new GeneratorSimulator { PrepareDelayMs = 0, ExposureDelayMs = 0 };
        var secContext = Substitute.For<ISecurityContext>();
        var engine = new WorkflowEngine(doseService, generator, secContext);
        using var vm = new WorkflowViewModel(engine, secContext);

        await engine.StartAsync("P-300", "1.2.3.4.5.300");
        vm.CurrentState.Should().Be("PatientSelected");

        // Act — abort
        var abortResult = await engine.AbortAsync("Test abort");

        // Assert
        abortResult.IsSuccess.Should().BeTrue("Abort should always succeed");
        engine.CurrentState.Should().Be(WorkflowState.Error);
        vm.CurrentState.Should().Be("Error");
        vm.StatusMessage.Should().Be("Test abort", "OnWorkflowStateChanged sets StatusMessage to the abort reason");
    }

    /// <summary>
    /// Integration test: WorkflowViewModel safe state display updates on state change.
    /// SWR-COORD-090: SafeState label reflects workflow engine's CurrentSafeState.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-090")]
    public void Workflow_SafeStateLabel_ReflectsEngineState()
    {
        // Arrange
        var doseService = Substitute.For<IDoseService>();
        var generator = new GeneratorSimulator { PrepareDelayMs = 0, ExposureDelayMs = 0 };
        var secContext = Substitute.For<ISecurityContext>();
        var engine = new WorkflowEngine(doseService, generator, secContext);
        using var vm = new WorkflowViewModel(engine, secContext);

        // Assert — initial safe state
        vm.CurrentSafeState.Should().Be(SafeState.Idle);
        vm.SafeStateLabel.Should().Be("IDLE");
    }

    /// <summary>
    /// Integration test: WorkflowViewModel DI resolution via full DI container.
    /// SWR-COORD-090: WorkflowViewModel resolves from DI and reflects engine state.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-090")]
    public void Workflow_DIResolution_ReflectsEngineInitialState()
    {
        // Arrange — resolve from full DI container
        var services = new ServiceCollection();
        SetupMinimalServices(services);
        var provider = services.BuildServiceProvider();

        // Act
        var vm = provider.GetService<IWorkflowViewModel>();

        // Assert
        vm.Should().NotBeNull("WorkflowViewModel must resolve from DI");
        vm!.CurrentState.Should().Be("Idle", "Initial state should be Idle");
        vm.IsExposureReady.Should().BeFalse("Should not be ready to expose initially");
    }

    // ── Scenario 11: MergeViewModel Integration Tests ───────────────────────────

    /// <summary>
    /// Integration test: MergeViewModel resolves from DI and has expected defaults.
    /// SWR-COORD-100: MergeViewModel DI resolution and initial state.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-100")]
    public void Merge_DIResolution_HasExpectedDefaults()
    {
        // Arrange
        var services = new ServiceCollection();
        SetupMinimalServices(services);
        var provider = services.BuildServiceProvider();

        // Act
        var vm = provider.GetService<IMergeViewModel>();

        // Assert — resolved successfully
        vm.Should().NotBeNull("MergeViewModel must resolve from DI");

        // Assert — default values
        vm!.SearchQueryA.Should().BeEmpty("SearchQueryA should be empty initially");
        vm.SearchQueryB.Should().BeEmpty("SearchQueryB should be empty initially");
        vm.PatientsA.Should().BeEmpty("PatientsA should be empty initially");
        vm.PatientsB.Should().BeEmpty("PatientsB should be empty initially");
        vm.SelectedPatientA.Should().BeNull("No patient selected initially on side A");
        vm.SelectedPatientB.Should().BeNull("No patient selected initially on side B");
        vm.PreviewStudiesA.Should().BeEmpty("PreviewStudiesA should be empty initially");
        vm.PreviewStudiesB.Should().BeEmpty("PreviewStudiesB should be empty initially");
        vm.SelectedStudies.Should().BeEmpty("SelectedStudies should be empty initially");
        vm.SelectedPreviewStudy.Should().BeNull("No preview study selected initially");
    }

    /// <summary>
    /// Integration test: MergeViewModel search populates patient lists via IPatientService.
    /// SWR-COORD-100: MergeViewModel SearchA populates PatientsA from IPatientService.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-100")]
    public async Task Merge_SearchA_PopulatesPatientsA()
    {
        // Arrange — setup patient service mock with test data
        var patientService = Substitute.For<IPatientService>();
        var testPatients = new List<PatientRecord>
        {
            new("P-001", "김환자", new DateOnly(1990, 5, 10), "M", false, DateTimeOffset.UtcNow, "admin"),
            new("P-002", "이환자", new DateOnly(1985, 8, 20), "F", false, DateTimeOffset.UtcNow, "admin"),
        };
        patientService.SearchAsync("김", Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<PatientRecord>>(testPatients));

        var vm = new MergeViewModel(patientService);
        vm.SearchQueryA = "김";

        // Act
        await vm.SearchACommand.ExecuteAsync(null);

        // Assert
        vm.PatientsA.Should().HaveCount(2);
        vm.PatientsA[0].Name.Should().Be("김환자");
        vm.PatientsA[1].Name.Should().Be("이환자");
        vm.IsLoading.Should().BeFalse();
    }

    /// <summary>
    /// Integration test: MergeViewModel SearchB populates PatientsB independently.
    /// SWR-COORD-100: MergeViewModel SearchB populates PatientsB independently from SearchA.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-100")]
    public async Task Merge_SearchB_PopulatesPatientsBIndependently()
    {
        // Arrange
        var patientService = Substitute.For<IPatientService>();
        var sideBPatients = new List<PatientRecord>
        {
            new("P-010", "박환자", new DateOnly(2000, 1, 1), "F", false, DateTimeOffset.UtcNow, "admin"),
        };
        patientService.SearchAsync("박", Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<PatientRecord>>(sideBPatients));

        var vm = new MergeViewModel(patientService);
        vm.SearchQueryB = "박";

        // Act
        await vm.SearchBCommand.ExecuteAsync(null);

        // Assert
        vm.PatientsB.Should().HaveCount(1);
        vm.PatientsB[0].Name.Should().Be("박환자");
        vm.PatientsA.Should().BeEmpty("SearchA should not be affected by SearchB");
    }

    /// <summary>
    /// Integration test: MergeViewModel handles search failure gracefully.
    /// SWR-COORD-100: MergeViewModel displays error message on search failure.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-100")]
    public async Task Merge_SearchFailure_DisplaysErrorMessage()
    {
        // Arrange
        var patientService = Substitute.For<IPatientService>();
        patientService.SearchAsync("없는사람", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<PatientRecord>>(ErrorCode.NotFound, "No patients found"));

        var vm = new MergeViewModel(patientService);
        vm.SearchQueryA = "없는사람";

        // Act
        await vm.SearchACommand.ExecuteAsync(null);

        // Assert
        vm.PatientsA.Should().BeEmpty("PatientsA should be empty on failure");
        vm.ErrorMessage.Should().Be("No patients found");
    }

    /// <summary>
    /// Integration test: IStudyItem contract — StudyItem wraps StudyRecord and tracks selection.
    /// SWR-COORD-100: IStudyItem interface contract verification for merge operations.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-100")]
    public void Merge_StudyItem_ContractHoldsStudyAndSelection()
    {
        // Arrange — create StudyItem wrapping a real StudyRecord
        var study = new StudyRecord(
            "1.2.3.4.5.999", "P-050", DateTimeOffset.UtcNow, "CHEST PA", "ACC-100", "CHEST");
        IStudyItem item = new StudyItem(study);

        // Assert — interface contract
        item.Study.Should().Be(study);
        item.Study.StudyInstanceUid.Should().Be("1.2.3.4.5.999");
        item.Study.BodyPart.Should().Be("CHEST");
        item.IsSelected.Should().BeFalse("Default selection state is false");

        // Act — toggle selection
        item.IsSelected = true;

        // Assert
        item.IsSelected.Should().BeTrue("Selection state should update");
    }

    /// <summary>
    /// Integration test: MergeViewModel SelectedStudies collection accepts IStudyItem.
    /// SWR-COORD-100: SelectedStudies ObservableCollection works with IStudyItem for merge.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-100")]
    public void Merge_SelectedStudies_AcceptsStudyItemsForMerge()
    {
        // Arrange
        var patientService = Substitute.For<IPatientService>();
        var vm = new MergeViewModel(patientService);

        var study1 = new StudyItem(new StudyRecord("1.2.3.1", "P-A", DateTimeOffset.UtcNow, "CHEST", "A001", "CHEST"));
        var study2 = new StudyItem(new StudyRecord("1.2.3.2", "P-B", DateTimeOffset.UtcNow, "HAND", "A002", "HAND"));
        study2.IsSelected = true;

        // Act — add studies to selected collection (simulates checkbox selection)
        vm.SelectedStudies.Add(study1);
        vm.SelectedStudies.Add(study2);

        // Assert
        vm.SelectedStudies.Should().HaveCount(2);
        vm.SelectedStudies.Count(s => s.IsSelected).Should().Be(1, "Only study2 is selected");
        vm.SelectedStudies[1].Study.BodyPart.Should().Be("HAND");
    }

    /// <summary>
    /// Integration test: MergeViewModel MergeCompleted event fires on successful merge.
    /// SWR-COORD-100: MergeCompleted event contract for UI notification.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-100")]
    public async Task Merge_MergeAsync_RaisesMergeCompletedEvent()
    {
        // Arrange
        var patientService = Substitute.For<IPatientService>();
        var vm = new MergeViewModel(patientService);

        // Set required selections
        vm.SelectedPatientA = new PatientRecord("P-A", "환자A", null, "M", false, DateTimeOffset.UtcNow, "admin");
        vm.SelectedPatientB = new PatientRecord("P-B", "환자B", null, "F", false, DateTimeOffset.UtcNow, "admin");

        var eventRaised = false;
        vm.MergeCompleted += (_, _) => eventRaised = true;

        // Act
        await vm.MergeCommand.ExecuteAsync(null);

        // Assert
        eventRaised.Should().BeTrue("MergeCompleted event should be raised on successful merge");
    }

    /// <summary>
    /// Integration test: MergeViewModel Cancel raises Cancelled event.
    /// SWR-COORD-100: Cancelled event contract for dialog close.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-100")]
    public void Merge_Cancel_RaisesCancelledEvent()
    {
        // Arrange
        var patientService = Substitute.For<IPatientService>();
        var vm = new MergeViewModel(patientService);

        var eventRaised = false;
        vm.Cancelled += (_, _) => eventRaised = true;

        // Act
        vm.CancelCommand.Execute(null);

        // Assert
        eventRaised.Should().BeTrue("Cancelled event should be raised on cancel");
    }

    /// <summary>
    /// Integration test: MergeViewModel merge without both patients shows error.
    /// SWR-COORD-100: Merge requires both patients to be selected.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-100")]
    public async Task Merge_WithoutBothPatients_ShowsErrorMessage()
    {
        // Arrange
        var patientService = Substitute.For<IPatientService>();
        var vm = new MergeViewModel(patientService);

        // Only select patient A, not B
        vm.SelectedPatientA = new PatientRecord("P-A", "환자A", null, "M", false, DateTimeOffset.UtcNow, "admin");

        // Act
        await vm.MergeCommand.ExecuteAsync(null);

        // Assert
        vm.ErrorMessage.Should().NotBeNull("Error message should be shown when patients not selected on both sides");
        vm.ErrorMessage.Should().Contain("select a patient on both sides");
    }

    /// <summary>
    /// Integration test: MergeViewModel PreviewStudies accepts IStudyItem for preview panel.
    /// SWR-COORD-100: PreviewStudiesA/B ObservableCollection holds IStudyItem.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-100")]
    public void Merge_PreviewStudies_AcceptsStudyItems()
    {
        // Arrange
        var patientService = Substitute.For<IPatientService>();
        var vm = new MergeViewModel(patientService);

        var study = new StudyItem(new StudyRecord("1.2.3.4.5", "P-001", DateTimeOffset.UtcNow, "ABD", "ACC-999", "ABDOMEN"));

        // Act
        vm.PreviewStudiesA.Add(study);

        // Assert
        vm.PreviewStudiesA.Should().HaveCount(1);
        vm.PreviewStudiesA[0].Study.Description.Should().Be("ABD");
    }

    // ── Scenario 12: SettingsViewModel Tab Selection ───────────────────────────

    /// <summary>
    /// Integration test: SettingsViewModel tab selection updates ActiveTab property.
    /// SWR-COORD-110: SettingsViewModel tab navigation works through command.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-110")]
    public void Settings_TabSelection_UpdatesActiveTabProperty()
    {
        // Arrange
        var settingsViewModel = new SettingsViewModel(Substitute.For<ISystemAdminService>());

        // Act — select Account tab
        settingsViewModel.ActiveTab = "Account";

        // Assert
        settingsViewModel.ActiveTab.Should().Be("Account", "ActiveTab should update to Account");

        // Act — select Network tab
        settingsViewModel.ActiveTab = "Network";

        // Assert
        settingsViewModel.ActiveTab.Should().Be("Network", "ActiveTab should update to Network");
    }

    /// <summary>
    /// Integration test: SettingsViewModel SaveCommand raises SaveCompleted event.
    /// SWR-COORD-110: SettingsViewModel save notification works through event.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-110")]
    public async Task Settings_SaveCommand_RaisesSaveCompletedEvent()
    {
        // Arrange
        var settingsService = Substitute.For<ISystemAdminService>();
        settingsService.UpdateSettingsAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var settingsViewModel = new SettingsViewModel(settingsService);
        var eventRaised = false;
        settingsViewModel.SaveCompleted += (_, _) => eventRaised = true;

        // Act
        await settingsViewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        eventRaised.Should().BeTrue("SaveCompleted event should be raised when SaveCommand executes");
        settingsViewModel.IsLoading.Should().BeFalse("IsLoading should be false after save completes");
    }

    /// <summary>
    /// Integration test: SettingsViewModel CancelCommand raises Cancelled event.
    /// SWR-COORD-110: SettingsViewModel cancel notification works through event.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-110")]
    public void Settings_CancelCommand_RaisesCancelledEvent()
    {
        // Arrange
        var settingsViewModel = new SettingsViewModel(Substitute.For<ISystemAdminService>());
        var eventRaised = false;
        settingsViewModel.Cancelled += (_, _) => eventRaised = true;

        // Act
        settingsViewModel.CancelCommand.Execute(null);

        // Assert
        eventRaised.Should().BeTrue("Cancelled event should be raised when CancelCommand executes");
    }

    /// <summary>
    /// Integration test: SettingsViewModel properties have expected default values.
    /// SWR-COORD-110: SettingsViewModel initializes with correct defaults.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-110")]
    public void Settings_Properties_HaveExpectedDefaults()
    {
        // Arrange & Act
        var settingsViewModel = new SettingsViewModel(Substitute.For<ISystemAdminService>());

        // Assert — default tab
        settingsViewModel.ActiveTab.Should().Be("System", "Default active tab should be System");

        // Assert — account defaults
        settingsViewModel.NewAccountId.Should().BeEmpty("NewAccountId should be empty initially");
        settingsViewModel.NewAccountRole.Should().Be("Technician", "Default role should be Technician");

        // Assert — network defaults
        settingsViewModel.PacsServerAddress.Should().BeEmpty("PACS server address should be empty initially");
        settingsViewModel.PacsServerPort.Should().Be(104, "Default PACS port should be 104");
        settingsViewModel.WorklistServerAddress.Should().BeEmpty("Worklist server address should be empty initially");
        settingsViewModel.WorklistServerPort.Should().Be(4006, "Default Worklist port should be 4006");

        // Assert — RIS tab defaults
        settingsViewModel.ActiveRisTab.Should().Be("Matching", "Default RIS tab should be Matching");

        // Assert — state defaults
        settingsViewModel.IsLoading.Should().BeFalse("IsLoading should be false initially");
        settingsViewModel.ErrorMessage.Should().BeNull("ErrorMessage should be null initially");
    }

    /// <summary>
    /// Integration test: SettingsViewModel DI registration as ISettingsViewModel.
    /// SWR-COORD-110: SettingsViewModel implements ISettingsViewModel contract.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-110")]
    public void Settings_DI_Registration_ISettingsViewModel()
    {
        // Arrange
        var services = new ServiceCollection();
        SetupMinimalServices(services);
        var provider = services.BuildServiceProvider();

        // Act
        var settingsViewModel = provider.GetService<ISettingsViewModel>();

        // Assert — resolved successfully
        settingsViewModel.Should().NotBeNull("ISettingsViewModel must resolve from DI container");

        // Assert — interface contract
        settingsViewModel!.Tabs.Should().NotBeEmpty("Tabs collection should not be empty");
        settingsViewModel.AvailableRoles.Should().NotBeEmpty("AvailableRoles should not be empty");
        settingsViewModel.SaveCommand.Should().NotBeNull("SaveCommand should be available");
        settingsViewModel.CancelCommand.Should().NotBeNull("CancelCommand should be available");
        settingsViewModel.SelectTabCommand.Should().NotBeNull("SelectTabCommand should be available");
    }

    /// <summary>
    /// Integration test: SettingsViewModel PACS and Worklist properties work with data binding.
    /// SWR-COORD-110: Network settings properties support two-way binding.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-110")]
    public void Settings_NetworkProperties_DataBindingWorksCorrectly()
    {
        // Arrange
        var settingsViewModel = new SettingsViewModel(Substitute.For<ISystemAdminService>());

        // Act — set PACS server properties
        settingsViewModel.PacsServerAddress = "192.168.1.100";
        settingsViewModel.PacsServerPort = 11112;

        // Assert
        settingsViewModel.PacsServerAddress.Should().Be("192.168.1.100", "PACS server address should update");
        settingsViewModel.PacsServerPort.Should().Be(11112, "PACS server port should update");

        // Act — set Worklist server properties
        settingsViewModel.WorklistServerAddress = "192.168.1.200";
        settingsViewModel.WorklistServerPort = 4006;

        // Assert
        settingsViewModel.WorklistServerAddress.Should().Be("192.168.1.200", "Worklist server address should update");
        settingsViewModel.WorklistServerPort.Should().Be(4006, "Worklist server port should update");
    }
}
