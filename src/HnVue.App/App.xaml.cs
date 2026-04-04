using HnVue.App.Stubs;
using HnVue.Common.Abstractions;
using HnVue.Common.Extensions;
using HnVue.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace HnVue.App;

/// <summary>
/// Application entry point. Bootstraps the DI container and launches <see cref="MainWindow"/>.
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    /// <inheritdoc/>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // ── Common (ISecurityContext) ──────────────────────────────────────────
                services.AddHnVueCommon();

                // ── Wave 3+ Stub Implementations ──────────────────────────────────────
                // These will be replaced by real implementations when the corresponding
                // modules (HnVue.Workflow, HnVue.Imaging, etc.) are integrated.
                services.AddSingleton<IWorkflowEngine, StubWorkflowEngine>();
                services.AddSingleton<IImageProcessor, StubImageProcessor>();
                services.AddSingleton<IDoseService, StubDoseService>();
                services.AddSingleton<IPatientService, StubPatientService>();
                services.AddSingleton<ICDDVDBurnService, StubCDDVDBurnService>();
                services.AddSingleton<ISystemAdminService, StubSystemAdminService>();

                // ── ViewModels ────────────────────────────────────────────────────────
                services.AddTransient<LoginViewModel>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<PatientListViewModel>();
                services.AddTransient<WorkflowViewModel>();
                services.AddTransient<ImageViewerViewModel>();
                services.AddTransient<DoseDisplayViewModel>();
                services.AddSingleton<SystemAdminViewModel>();
                services.AddTransient<CDBurnViewModel>();

                // ── Main Window ───────────────────────────────────────────────────────
                services.AddSingleton<MainWindow>();
            })
            .Build();

        _host.Start();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    /// <inheritdoc/>
    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
