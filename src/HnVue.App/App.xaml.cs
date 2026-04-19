using System.Windows;
using HnVue.CDBurning;
using HnVue.Common.Abstractions;
using HnVue.Common.Extensions;
using HnVue.Common.Models;
using HnVue.Data;
using HnVue.Data.Extensions;
using HnVue.Dicom;
using HnVue.Dose;
using HnVue.Incident;
using HnVue.PatientManagement;
using HnVue.Security;
using HnVue.Security.Extensions;
using HnVue.SystemAdmin;
using HnVue.UI.Contracts.Navigation;
using HnVue.UI.Contracts.ViewModels;
using HnVue.UI.ViewModels;
using HnVue.App.Services;
using HnVue.Update;
using HnVue.Detector;
using HnVue.Detector.OwnDetector;
using HnVue.Detector.ThirdParty.Hme;
using HnVue.Workflow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HnVue.App;

/// <summary>
/// WPF application entry point.
/// Builds the Microsoft.Extensions.Hosting <see cref="IHost"/> with full DI for all 13 modules,
/// starts the host on application startup, and stops it cleanly on exit.
/// IEC 62304 §5.1.1 — software architecture: centralised composition root.
/// </summary>
public partial class App : Application
{
    // ── Safe development defaults ─────────────────────────────────────────────

    /// <summary>
    /// SQLite connection string used for Phase 1d development.
    /// Production deployments should supply an encrypted connection string via
    /// configuration (<c>appsettings.json</c> or environment variable).
    /// </summary>
    // @MX:WARN DefaultConnectionString - @MX:REASON: Hardcoded SQLite connection string; production requires encrypted config
    private const string DefaultConnectionString = "Data Source=hnvue.db";

    private IHost? _host;

    // ── Lifetime hooks ────────────────────────────────────────────────────────

    /// <summary>
    /// Builds the DI host and opens the main window.
    /// Invoked by WPF before the first window is displayed.
    /// </summary>
    // @MX:WARN OnStartup - @MX:REASON: async void WPF event handler; exceptions on unobserved task crash app
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = BuildHost();
        await _host.StartAsync().ConfigureAwait(false);

        // Development: ensure DB exists and seed default admin account.
        await EnsureDatabaseSeededAsync(_host.Services).ConfigureAwait(false);

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    /// <summary>
    /// Creates the SQLite schema (if absent) and seeds a default admin user for development.
    /// This runs only when no users exist in the database.
    /// </summary>
    private static async Task EnsureDatabaseSeededAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HnVueDbContext>();
        await db.Database.EnsureCreatedAsync().ConfigureAwait(false);

        if (!db.Users.Any())
        {
            db.Users.Add(new Data.Entities.UserEntity
            {
                UserId       = Guid.NewGuid().ToString(),
                Username     = "admin",
                DisplayName  = "Administrator",
                PasswordHash = HnVue.Security.PasswordHasher.HashPassword("Admin1234!"),
                RoleValue    = (int)Common.Enums.UserRole.Admin,
            });
            await db.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Stops the DI host and releases all managed resources.
    /// Invoked after the last window closes.
    /// </summary>
    // @MX:WARN OnExit - @MX:REASON: async void WPF event handler; exceptions on unobserved task during app shutdown
    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync().ConfigureAwait(false);
            _host.Dispose();
        }

        base.OnExit(e);
    }

    // ── DI composition root ───────────────────────────────────────────────────

    /// <summary>
    /// Constructs the application <see cref="IHost"/> and registers all 13 module services.
    /// </summary>
    /// <remarks>
    /// Registration order follows module dependency hierarchy.
    /// Repositories without EF implementations in Phase 1d use lightweight no-op stubs
    /// (inner sealed classes in this file) marked with <c>// STUB</c>.
    /// </remarks>
    // @MX:ANCHOR BuildHost - @MX:REASON: Composition root for entire 13-module DI graph; all services registered here
    private static IHost BuildHost()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                // ── HnVue.Common ─────────────────────────────────────────────
                // Registers ISecurityContext (ThreadLocalSecurityContext singleton).
                services.AddHnVueCommon();

                // ── HnVue.Data ───────────────────────────────────────────────
                // Registers EF Core DbContext + IUserRepository, IAuditRepository,
                // IPatientRepository, IStudyRepository (backed by SQLite).
                services.AddHnVueData(DefaultConnectionString);

                // ── HnVue.Security ───────────────────────────────────────────
                // Registers ISecurityService, IAuditService, JwtTokenService.
                // Secrets loaded from configuration with environment variable fallback.
                var jwtSecret = configuration["Security:JwtSecretKey"]
                    ?? Environment.GetEnvironmentVariable("HNVUE_JWT_SECRET");
                if (string.IsNullOrWhiteSpace(jwtSecret))
                    throw new InvalidOperationException(
                        "JWT secret key is not configured. " +
                        "Set 'Security:JwtSecretKey' in appsettings.json or the HNVUE_JWT_SECRET environment variable.");

                var auditHmacKey = configuration["Security:AuditHmacKey"]
                    ?? Environment.GetEnvironmentVariable("HNVUE_AUDIT_HMAC_KEY");
                if (string.IsNullOrWhiteSpace(auditHmacKey))
                    throw new InvalidOperationException(
                        "Audit HMAC key is not configured. " +
                        "Set 'Security:AuditHmacKey' in appsettings.json or the HNVUE_AUDIT_HMAC_KEY environment variable.");

                services.AddHnVueSecurity(
                    new JwtOptions
                    {
                        SecretKey = jwtSecret,
                        Issuer = "HnVue",
                        Audience = "HnVue",
                        ExpiryMinutes = 60,
                    },
                    new AuditOptions
                    {
                        HmacKey = auditHmacKey,
                    });

                // ── HnVue.Workflow ───────────────────────────────────────────
                // WorkflowEngine depends on IDoseService, IGeneratorInterface, and IDetectorInterface.
                // IGeneratorInterface: GeneratorSimulator (safe, no real hardware).
                // IDetectorInterface: conditional registration based on SDK DLL availability.
                services.AddSingleton<IGeneratorInterface, GeneratorSimulator>();
                RegisterDetectorService(services, configuration);
                services.AddScoped<IWorkflowEngine, WorkflowEngine>();

                // ── HnVue.Dose ───────────────────────────────────────────────
                // IDoseRepository: EF Core implementation — replaced from NullDoseRepository (SPEC-COORDINATOR-001).
                services.AddScoped<IDoseRepository, EfDoseRepository>();
                services.AddScoped<IDoseService, DoseService>();

                // ── HnVue.PatientManagement ──────────────────────────────────
                // IWorklistRepository: EF Core implementation — replaced from NullWorklistRepository (SPEC-COORDINATOR-001).
                services.AddScoped<IWorklistRepository, EfWorklistRepository>();
                services.AddScoped<IPatientService, PatientService>();
                services.AddScoped<IWorklistService, WorklistService>();

                // ── HnVue.Incident ───────────────────────────────────────────
                // IIncidentRepository: EF Core implementation — replaced from NullIncidentRepository (SPEC-COORDINATOR-001).
                services.AddScoped<IIncidentRepository, EfIncidentRepository>();
                services.AddScoped<IncidentResponseService>();

                // ── HnVue.Update ─────────────────────────────────────────────
                // IUpdateRepository: EF Core implementation — replaced from NullUpdateRepository (SPEC-COORDINATOR-001).
                services.AddScoped<IUpdateRepository, EfUpdateRepository>();
                services.AddSingleton(new BackupService(
                    applicationDirectory: AppContext.BaseDirectory,
                    backupBaseDirectory: System.IO.Path.Combine(AppContext.BaseDirectory, "Backups")));
                services.AddScoped<ISWUpdateService, SWUpdateService>();

                // ── HnVue.SystemAdmin ────────────────────────────────────────
                // ISystemSettingsRepository: EF Core implementation — replaced from NullSystemSettingsRepository (SPEC-COORDINATOR-001).
                services.AddScoped<ISystemSettingsRepository, EfSystemSettingsRepository>();
                services.AddScoped<ISystemAdminService, SystemAdminService>();

                // ── HnVue.CDBurning ──────────────────────────────────────────
                // IMAPIComWrapper implements IBurnSession (simulated for Phase 1d).
                // IStudyRepository (CDBurning-specific): EF Core implementation — replaced from NullCdStudyRepository (SPEC-COORDINATOR-001).
                services.AddSingleton<IBurnSession, IMAPIComWrapper>();
                services.AddScoped<HnVue.CDBurning.IStudyRepository, StudyRepository>();
                services.AddScoped<ICDDVDBurnService, CDDVDBurnService>();

                // ── HnVue.Dicom ──────────────────────────────────────────────
                // IDicomNetworkConfig: inline record with Phase 1d localhost defaults.
                services.AddSingleton<IDicomNetworkConfig>(new DicomNetworkConfig(
                    PacsHost: "localhost",
                    PacsPort: 11112,
                    PacsAeTitle: "DCMRCV",
                    LocalAeTitle: "HNVUE_SCU",
                    MwlHost: "localhost",
                    MwlPort: 11113));
                services.AddSingleton<IDicomService, DicomService>();
                services.AddSingleton<DicomStoreScu>();
                services.AddSingleton<DicomFileIO>();

                // ── HnVue.Imaging ────────────────────────────────────────────
                // ImageProcessor registered as IImageProcessor (interface-based DI).
                services.AddSingleton<IImageProcessor, HnVue.Imaging.ImageProcessor>();

                // ── HnVue.UI ViewModels ───────────────────────────────────────
                // Each concrete ViewModel is registered against its interface contract so
                // the DI container resolves the interface wherever declared as a dependency.
                // Concrete registrations are also provided for types resolved directly
                // (e.g. MainWindow constructor, MainViewModel sub-ViewModel parameters).
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
                // MainViewModel registered as Singleton: NavigationService depends on IMainViewModel,
                // and MainWindow is Singleton — both require the same shell instance for the app lifetime.
                // Single registration via interface; concrete type forwarded to avoid two separate instances.
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<IMainViewModel>(sp => sp.GetRequiredService<MainViewModel>());
                // Concrete-type registrations required where the DI container must resolve
                // the concrete class (MainViewModel constructor args, MainWindow).
                services.AddTransient<CDBurnViewModel>();
                services.AddTransient<SystemAdminViewModel>();

                // ── HnVue.App Navigation ─────────────────────────────────────
                // NavigationService is Singleton: delegates to IMainViewModel (also Singleton).
                services.AddSingleton<INavigationService, NavigationService>();

                // ── WPF main window ──────────────────────────────────────────
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    // ── Nested helper types ───────────────────────────────────────────────────

    /// <summary>
    /// Registers the appropriate <see cref="IDetectorInterface"/> implementation
    /// based on which SDK DLL is available at runtime.
    /// <list type="bullet">
    ///   <item>AbyzSdk.dll present → <see cref="OwnDetectorAdapter"/> (자사 CsI FPD)</item>
    ///   <item>libxd2.dll present → <see cref="DetectorSimulator"/> (HME adapter pending SDK integration)</item>
    ///   <item>No SDK DLL → <see cref="DetectorSimulator"/> (development/test)</item>
    /// </list>
    /// </summary>
    // @MX:NOTE Conditional detector DI registration — SDK DLL presence determines adapter at runtime
    private static void RegisterDetectorService(IServiceCollection services, IConfiguration configuration)
    {
        var sdkPath = System.IO.Path.Combine(AppContext.BaseDirectory, "AbyzSdk.dll");
        var hmePath = System.IO.Path.Combine(AppContext.BaseDirectory, "libxd2.dll");

        if (System.IO.File.Exists(sdkPath))
        {
            // 자사 AbyzSdk 어댑터 (.NET managed, IL Only)
            // 연결: TCP/IP (IP + Port, 단일 소켓)
            services.AddSingleton<IDetectorInterface>(sp =>
                new OwnDetectorAdapter(new OwnDetectorConfig(
                    Host: configuration["Detector:Host"] ?? "192.168.1.100",
                    Port: int.Parse(configuration["Detector:Port"] ?? "8888", System.Globalization.CultureInfo.InvariantCulture),
                    CalibrationPath: configuration["Detector:CalibrationPath"] ?? @"C:\HnVue\Calibration\")));
        }
        else if (System.IO.File.Exists(hmePath))
        {
            // HME 어댑터 (Native C, 5-소켓 연결)
            // 연결: TCP 5-소켓 (Control:25000, Data:25001, Trigger:25002, Status:25003, SAlign:25004)
            // Team B HmeDetectorAdapter 구현 완료 후 아래 코드 활성화:
            // services.AddSingleton<IDetectorInterface>(sp =>
            //     new HmeDetectorAdapter(new HmeDetectorConfig(
            //         Host: configuration["Detector:Host"] ?? "192.168.197.80",
            //         ParamFilePath: configuration["Detector:ParamPath"] ?? @"C:\HnVue\HME\param\")));
            services.AddSingleton<IDetectorInterface, DetectorSimulator>(); // 임시 — HME SDK 통합 대기
        }
        else
        {
            // 개발/테스트용 시뮬레이터
            services.AddSingleton<IDetectorInterface, DetectorSimulator>();
        }
    }

    /// <summary>
    /// Value-object implementation of <see cref="IDicomNetworkConfig"/> that supplies
    /// Phase 1d localhost defaults without requiring appsettings.json.
    /// </summary>
    private sealed record DicomNetworkConfig(
        string PacsHost,
        int PacsPort,
        string PacsAeTitle,
        string LocalAeTitle,
        string MwlHost,
        int MwlPort) : IDicomNetworkConfig;
}
