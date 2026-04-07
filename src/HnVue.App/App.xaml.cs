using System.Windows;
using HnVue.CDBurning;
using HnVue.Common.Abstractions;
using HnVue.Common.Extensions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data;
using HnVue.Data.Extensions;
using HnVue.Dicom;
using HnVue.Dose;
using HnVue.Incident;
using HnVue.Incident.Models;
using HnVue.PatientManagement;
using HnVue.Security;
using HnVue.Security.Extensions;
using HnVue.SystemAdmin;
using HnVue.UI.Contracts.ViewModels;
using HnVue.UI.ViewModels;
using HnVue.Update;
using HnVue.Detector;
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
                // IDetectorInterface: DetectorSimulator (safe, no real hardware).
                // Production: replace DetectorSimulator with OwnDetectorAdapter — see sdk/own-detector/README.md
                services.AddSingleton<IGeneratorInterface, GeneratorSimulator>();
                services.AddSingleton<IDetectorInterface, DetectorSimulator>();
                services.AddScoped<IWorkflowEngine, WorkflowEngine>();

                // ── HnVue.Dose ───────────────────────────────────────────────
                // IDoseRepository: no EF implementation in Phase 1d — no-op stub.   // STUB
                services.AddSingleton<IDoseRepository, NullDoseRepository>();
                services.AddScoped<IDoseService, DoseService>();

                // ── HnVue.PatientManagement ──────────────────────────────────
                // IWorklistRepository: no EF implementation in Phase 1d — stub.     // STUB
                services.AddSingleton<IWorklistRepository, NullWorklistRepository>();
                services.AddScoped<IPatientService, PatientService>();
                services.AddScoped<IWorklistService, WorklistService>();

                // ── HnVue.Incident ───────────────────────────────────────────
                // IIncidentRepository: no EF implementation in Phase 1d — stub.     // STUB
                services.AddSingleton<IIncidentRepository, NullIncidentRepository>();
                services.AddScoped<IncidentResponseService>();

                // ── HnVue.Update ─────────────────────────────────────────────
                // IUpdateRepository: no EF implementation in Phase 1d — stub.       // STUB
                services.AddSingleton<IUpdateRepository, NullUpdateRepository>();
                services.AddSingleton(new BackupService(
                    applicationDirectory: AppContext.BaseDirectory,
                    backupBaseDirectory: System.IO.Path.Combine(AppContext.BaseDirectory, "Backups")));
                services.AddScoped<ISWUpdateService, SWUpdateService>();

                // ── HnVue.SystemAdmin ────────────────────────────────────────
                // ISystemSettingsRepository: no EF implementation in Phase 1d.      // STUB
                services.AddSingleton<ISystemSettingsRepository, NullSystemSettingsRepository>();
                services.AddScoped<ISystemAdminService, SystemAdminService>();

                // ── HnVue.CDBurning ──────────────────────────────────────────
                // IMAPIComWrapper implements IBurnSession (simulated for Phase 1d).
                // IStudyRepository (CDBurning-specific): no EF implementation.      // STUB
                services.AddSingleton<IBurnSession, IMAPIComWrapper>();
                services.AddSingleton<HnVue.CDBurning.IStudyRepository, NullCdStudyRepository>();
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
                services.AddSingleton<DicomStoreScu>();
                // DicomFindScu removed — Issue #24: WorklistRepository now uses IDicomService as single entry point.
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
                services.AddTransient<IPatientListViewModel, PatientListViewModel>();
                services.AddTransient<IImageViewerViewModel, ImageViewerViewModel>();
                services.AddTransient<IWorkflowViewModel, WorkflowViewModel>();
                services.AddTransient<IDoseDisplayViewModel, DoseDisplayViewModel>();
                services.AddTransient<IDoseViewModel, DoseViewModel>();
                services.AddTransient<ICDBurnViewModel, CDBurnViewModel>();
                services.AddTransient<ISystemAdminViewModel, SystemAdminViewModel>();
                services.AddTransient<IQuickPinLockViewModel, QuickPinLockViewModel>();
                services.AddTransient<IMainViewModel, MainViewModel>();
                // Concrete-type registrations required where the DI container must resolve
                // the concrete class (MainViewModel constructor args, MainWindow).
                services.AddTransient<CDBurnViewModel>();
                services.AddTransient<SystemAdminViewModel>();
                services.AddTransient<MainViewModel>();

                // ── WPF main window ──────────────────────────────────────────
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    // ── Phase 1d no-op stub repositories ─────────────────────────────────────
    // These stubs satisfy the DI container until EF Core implementations are added.
    // They return "not found" or "not supported" failures so that higher-level code
    // handles absence gracefully rather than throwing NullReferenceExceptions.

    /// <summary>No-op stub for <see cref="IDoseRepository"/>.</summary>
    private sealed class NullDoseRepository : IDoseRepository
    {
        private const string NoStorageMessage = "NullDoseRepository: no storage configured.";

        /// <inheritdoc/>
        public Task<Result> SaveAsync(DoseRecord dose, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Failure(ErrorCode.NotFound, NoStorageMessage));

        /// <inheritdoc/>
        public Task<Result<DoseRecord?>> GetByStudyAsync(string studyInstanceUid, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success<DoseRecord?>(null));

        /// <inheritdoc/>
        public Task<Result<IReadOnlyList<DoseRecord>>> GetByPatientAsync(
            string patientId,
            DateTimeOffset? from,
            DateTimeOffset? until,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success<IReadOnlyList<DoseRecord>>(
                new List<DoseRecord>().AsReadOnly()));
    }

    /// <summary>No-op stub for <see cref="IWorklistRepository"/>.</summary>
    private sealed class NullWorklistRepository : IWorklistRepository
    {
        /// <inheritdoc/>
        public Task<Result<IReadOnlyList<WorklistItem>>> QueryTodayAsync(CancellationToken ct = default)
            => Task.FromResult(Result.Success<IReadOnlyList<WorklistItem>>(Array.Empty<WorklistItem>()));
    }

    /// <summary>No-op stub for <see cref="IIncidentRepository"/>.</summary>
    private sealed class NullIncidentRepository : IIncidentRepository
    {
        /// <inheritdoc/>
        public Task<Result> SaveAsync(IncidentRecord record, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        /// <inheritdoc/>
        public Task<Result<IReadOnlyList<IncidentRecord>>> GetBySeverityAsync(
            HnVue.Common.Enums.IncidentSeverity severity, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success<IReadOnlyList<IncidentRecord>>(Array.Empty<IncidentRecord>()));

        /// <inheritdoc/>
        public Task<Result> ResolveAsync(string incidentId, string resolution, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }

    /// <summary>No-op stub for <see cref="IUpdateRepository"/>.</summary>
    private sealed class NullUpdateRepository : IUpdateRepository
    {
        /// <inheritdoc/>
        public Task<Result<UpdateInfo?>> CheckForUpdateAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success<UpdateInfo?>(null));

        /// <inheritdoc/>
        public Task<Result<UpdateInfo>> GetPackageInfoAsync(string packagePath, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Failure<UpdateInfo>(ErrorCode.NotFound, "NullUpdateRepository: no update source configured."));

        /// <inheritdoc/>
        public Task<Result> ApplyPackageAsync(string packagePath, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Failure(ErrorCode.NotFound, "NullUpdateRepository: no update source configured."));
    }

    /// <summary>No-op stub for <see cref="ISystemSettingsRepository"/>.</summary>
    private sealed class NullSystemSettingsRepository : ISystemSettingsRepository
    {
        private SystemSettings _settings = new();

        /// <inheritdoc/>
        public Task<Result<SystemSettings>> GetAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(_settings));

        /// <inheritdoc/>
        public Task<Result> SaveAsync(SystemSettings settings, CancellationToken cancellationToken = default)
        {
            _settings = settings;
            return Task.FromResult(Result.Success());
        }
    }

    /// <summary>No-op stub for <see cref="HnVue.CDBurning.IStudyRepository"/>.</summary>
    private sealed class NullCdStudyRepository : HnVue.CDBurning.IStudyRepository
    {
        /// <inheritdoc/>
        public Task<Result<IReadOnlyList<string>>> GetFilesForStudyAsync(
            string studyInstanceUid, CancellationToken ct = default)
            => Task.FromResult(Result.Success<IReadOnlyList<string>>(Array.Empty<string>()));
    }

    // ── Nested helper types ───────────────────────────────────────────────────

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
