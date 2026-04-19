using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Configuration;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.CDBurning;
using HnVue.Detector;
using HnVue.Dicom;
using HnVue.Dose;
using HnVue.Imaging;
using HnVue.Incident;
using HnVue.Update;
using HnVue.PatientManagement;
using HnVue.Security;
using HnVue.Security.Extensions;
using HnVue.SystemAdmin;
using HnVue.UI.Contracts.Navigation;
using HnVue.UI.Contracts.ViewModels;
using HnVue.UI.ViewModels;
using HnVue.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HnVue.IntegrationTests;

/// <summary>
/// Integration tests verifying DI container registration completeness for all 13 modules.
/// Ensures every service and ViewModel resolves correctly from the composition root.
/// SWR-COORD-120: All module services resolve from DI container.
/// SWR-COORD-130: S13-R1 STRIDE security services resolve from DI container.
/// </summary>
public sealed class DiRegistrationIntegrationTests
{
    private static readonly JwtOptions TestJwtOptions = new()
    {
        SecretKey = "DiRegistrationTestKey-32CharMin!",
        ExpiryMinutes = 15,
        Issuer = "HnVue",
        Audience = "HnVue",
    };

    private static readonly IOptions<AuditOptions> TestAuditOptions =
        Options.Create(new AuditOptions { HmacKey = "DiRegTestHmacKey-32CharMin!" });

    /// <summary>
    /// Integration test: All 14 ViewModel interfaces resolve from the full DI container.
    /// SWR-COORD-120: Complete ViewModel DI registration verification.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-120")]
    public void DI_AllViewModels_ResolveSuccessfully()
    {
        var provider = BuildProvider();

        provider.GetService<IMainViewModel>().Should().NotBeNull();
        provider.GetService<ILoginViewModel>().Should().NotBeNull();
        provider.GetService<IPatientListViewModel>().Should().NotBeNull();
        provider.GetService<IStudylistViewModel>().Should().NotBeNull();
        provider.GetService<IWorkflowViewModel>().Should().NotBeNull();
        provider.GetService<IImageViewerViewModel>().Should().NotBeNull();
        provider.GetService<IDoseDisplayViewModel>().Should().NotBeNull();
        provider.GetService<IDoseViewModel>().Should().NotBeNull();
        provider.GetService<ICDBurnViewModel>().Should().NotBeNull();
        provider.GetService<ISystemAdminViewModel>().Should().NotBeNull();
        provider.GetService<IMergeViewModel>().Should().NotBeNull();
        provider.GetService<ISettingsViewModel>().Should().NotBeNull();
        provider.GetService<IQuickPinLockViewModel>().Should().NotBeNull();
        provider.GetService<IAddPatientProcedureViewModel>().Should().NotBeNull();
    }

    /// <summary>
    /// Integration test: STRIDE security services resolve from DI container.
    /// SWR-COORD-130: TLS, PHI masking, rate limiting services registered.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-130")]
    public void DI_StrideSecurityServices_ResolveSuccessfully()
    {
        var provider = BuildProvider();

        provider.GetService<ITlsConnectionService>().Should().NotBeNull("ITlsConnectionService must be registered (STRIDE S/T)");
        provider.GetService<IPhiMaskingService>().Should().NotBeNull("IPhiMaskingService must be registered (STRIDE I)");
        provider.GetService<IRateLimitingService>().Should().NotBeNull("IRateLimitingService must be registered (STRIDE D)");
    }

    /// <summary>
    /// Integration test: SecurityService and AuditService resolve from DI.
    /// SWR-COORD-130: Core security services available through DI.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-130")]
    public void DI_CoreSecurityServices_ResolveSuccessfully()
    {
        var provider = BuildProvider();

        provider.GetService<ISecurityService>().Should().NotBeNull();
        provider.GetService<IAuditService>().Should().NotBeNull();
        provider.GetService<ITokenDenylist>().Should().NotBeNull();
    }

    /// <summary>
    /// Integration test: All domain services resolve from DI.
    /// SWR-COORD-120: Domain service registration completeness.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-120")]
    public void DI_DomainServices_ResolveSuccessfully()
    {
        var provider = BuildProvider();

        provider.GetService<IPatientService>().Should().NotBeNull();
        provider.GetService<IDoseService>().Should().NotBeNull();
        provider.GetService<IWorklistService>().Should().NotBeNull();
        provider.GetService<ISystemAdminService>().Should().NotBeNull();
        provider.GetService<IWorkflowEngine>().Should().NotBeNull();
        provider.GetService<IDetectorInterface>().Should().NotBeNull();
        provider.GetService<IGeneratorInterface>().Should().NotBeNull();
        provider.GetService<ICDDVDBurnService>().Should().NotBeNull();
        provider.GetService<IImageProcessor>().Should().NotBeNull();
        provider.GetService<IDicomNetworkConfig>().Should().NotBeNull();
        provider.GetService<IDicomService>().Should().NotBeNull("IDicomService must be registered (Print SCU)");
        provider.GetService<DicomStoreScu>().Should().NotBeNull();
    }

    /// <summary>
    /// Integration test: NavigationService resolves from DI.
    /// SWR-COORD-120: Navigation service composition verified.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-120")]
    public void DI_NavigationService_ResolvesSuccessfully()
    {
        var provider = BuildProvider();

        var navService = provider.GetService<INavigationService>();
        navService.Should().NotBeNull("INavigationService must be resolvable");
    }

    /// <summary>
    /// Integration test: AddPatientProcedureViewModel correctly receives ISecurityContext.
    /// SWR-COORD-120: Constructor injection verified for new ISecurityContext dependency.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-COORD-120")]
    public void DI_AddPatientProcedureViewModel_ReceivesSecurityContext()
    {
        var provider = BuildProvider();

        var vm = provider.GetService<IAddPatientProcedureViewModel>();
        vm.Should().NotBeNull("AddPatientProcedureViewModel must resolve with ISecurityContext injection");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        // Repositories (mocked)
        services.AddSingleton(Substitute.For<IUserRepository>());
        services.AddSingleton(Substitute.For<IAuditRepository>());
        services.AddSingleton(Substitute.For<IPatientRepository>());
        services.AddSingleton(Substitute.For<IDoseRepository>());
        services.AddSingleton(Substitute.For<HnVue.Common.Abstractions.IStudyRepository>());
        services.AddSingleton(Substitute.For<IWorklistRepository>());
        services.AddSingleton(Substitute.For<ISystemSettingsRepository>());
        services.AddSingleton(Substitute.For<HnVue.CDBurning.IStudyRepository>());
        services.AddSingleton(Substitute.For<IIncidentRepository>());
        services.AddSingleton(Substitute.For<IUpdateRepository>());

        // Infrastructure
        services.AddSingleton(Substitute.For<ISecurityContext>());
        services.AddSingleton(Substitute.For<ITokenDenylist>());
        services.AddSingleton(TestJwtOptions);
        services.AddSingleton(TestAuditOptions);

        // Security (real — includes STRIDE controls)
        services.AddHnVueSecurity(TestJwtOptions, TestAuditOptions.Value);

        // Domain services
        services.AddSingleton<IPatientService, PatientService>();
        services.AddSingleton<IDoseService, DoseService>();
        services.AddSingleton<IWorklistService, WorklistService>();
        services.AddSingleton<ISystemAdminService, SystemAdminService>();
        services.AddSingleton(Substitute.For<ISWUpdateService>());

        // Imaging
        services.AddSingleton(Substitute.For<IImageProcessor>());

        // Workflow
        var doseServiceForWorkflow = Substitute.For<IDoseService>();
        var generator = new GeneratorSimulator { PrepareDelayMs = 0, ExposureDelayMs = 0 };
        services.AddSingleton(doseServiceForWorkflow);
        services.AddSingleton<IGeneratorInterface>(generator);
        services.AddSingleton<IDetectorInterface, DetectorSimulator>();
        services.AddSingleton<IWorkflowEngine, WorkflowEngine>();

        // CD burning
        services.AddSingleton(Substitute.For<IBurnSession>());
        services.AddSingleton<ICDDVDBurnService, CDDVDBurnService>();

        // DICOM
        services.AddSingleton<IDicomNetworkConfig>(new TestDiDicomNetworkConfig());
        services.AddSingleton<IDicomService, DicomService>();
        services.AddSingleton<DicomStoreScu>();
        services.AddSingleton<DicomFileIO>();

        // ViewModels (same registration pattern as App.xaml.cs)
        services.AddTransient<ILoginViewModel, LoginViewModel>();
        services.AddTransient<IStudylistViewModel, StudylistViewModel>();
        services.AddTransient<IPatientListViewModel, PatientListViewModel>();
        services.AddTransient<IImageViewerViewModel, ImageViewerViewModel>();
        services.AddTransient<IWorkflowViewModel, WorkflowViewModel>();
        services.AddTransient<IDoseDisplayViewModel, DoseDisplayViewModel>();
        services.AddTransient<IDoseViewModel, DoseViewModel>();
        services.AddTransient<ICDBurnViewModel, CDBurnViewModel>();
        services.AddTransient<ISystemAdminViewModel, SystemAdminViewModel>();
        services.AddTransient<IQuickPinLockViewModel, QuickPinLockViewModel>();
        services.AddTransient<IMergeViewModel, MergeViewModel>();
        services.AddTransient<ISettingsViewModel>(sp => new SettingsViewModel(sp.GetRequiredService<ISystemAdminService>()));
        services.AddTransient<IAddPatientProcedureViewModel, AddPatientProcedureViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<IMainViewModel>(sp => sp.GetRequiredService<MainViewModel>());

        // Navigation
        services.AddSingleton<INavigationService, TestDiNavigationService>();

        return services.BuildServiceProvider();
    }

    private sealed class TestDiDicomNetworkConfig : IDicomNetworkConfig
    {
        public string PacsHost => "localhost";
        public int PacsPort => 11112;
        public string PacsAeTitle => "TEST_PACS";
        public string LocalAeTitle => "HNVUE_SCU";
        public string MwlHost => "localhost";
        public int MwlPort => 11113;
    }

    private sealed class TestDiNavigationService : INavigationService
    {
        private readonly IMainViewModel _shell;

        public TestDiNavigationService(IMainViewModel shell)
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
