using System;
using System.Threading;
using System.Windows;
using Xunit;

namespace HnVue.UI.Tests;

/// <summary>
/// xUnit collection marker that forces tests depending on the shared WPF Application
/// to run serially. Required because only one <see cref="Application"/> instance
/// may exist per AppDomain and StaticResource resolution is not reentrancy-safe.
/// </summary>
[CollectionDefinition("WpfApplication", DisableParallelization = true)]
public sealed class WpfApplicationCollection
{
}

/// <summary>
/// Shared fixture that initializes a single WPF <see cref="Application"/> instance
/// with all HnVue theme resource dictionaries loaded. This allows tests that
/// instantiate Views (which reference StaticResource theme keys) to succeed
/// under STA threads.
/// </summary>
/// <remarks>
/// WPF requires exactly one Application per AppDomain. We create it once on
/// an STA thread during the first call to <see cref="EnsureInitialized"/> and
/// keep it alive for the test-run lifetime.
/// </remarks>
internal static class WpfApplicationFixture
{
    private static readonly object _lock = new();
    private static bool _initialized;

    /// <summary>
    /// Ensures that <see cref="Application.Current"/> exists and the HnVue
    /// theme resource dictionary is merged into its resources.
    /// Safe to call from any thread. Idempotent.
    /// </summary>
    public static void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        lock (_lock)
        {
            if (_initialized)
            {
                return;
            }

            Exception? caught = null;
            var started = new ManualResetEventSlim(false);

            var thread = new Thread(() =>
            {
                try
                {
                    if (Application.Current is null)
                    {
                        var app = new Application
                        {
                            ShutdownMode = ShutdownMode.OnExplicitShutdown,
                        };

                        // Ensure pack:// scheme is registered by touching it first.
                        _ = new Uri("pack://application:,,,/");

                        // Merge dictionaries in the same order as HnVue.App/App.xaml so that
                        // Views constructed in tests resolve the same resource keys at runtime.
                        AddDictionary(app, "pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml");
                        AddDictionary(app, "pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml");
                        AddDictionary(app, "pack://application:,,,/MahApps.Metro;component/Styles/Themes/Dark.Steel.xaml");
                        AddDictionary(app, "pack://application:,,,/HnVue.UI;component/Themes/HnVueTheme.xaml");

                        // Register application-wide converter instances. Mirrors App.xaml.
                        app.Resources.Add("BooleanToVisibilityConverter", new System.Windows.Controls.BooleanToVisibilityConverter());
                        app.Resources.Add("BoolToVisibilityConverter", new HnVue.UI.Converters.BoolToVisibilityConverter());
                        app.Resources.Add("NullToVisibilityConverter", new HnVue.UI.Converters.NullToVisibilityConverter());
                        app.Resources.Add("InverseBoolConverter", new HnVue.UI.Converters.InverseBoolConverter());
                        app.Resources.Add("ActiveTabToVisibilityConverter", new HnVue.UI.Converters.ActiveTabToVisibilityConverter());
                        app.Resources.Add("StringEqualityToBoolConverter", new HnVue.UI.Converters.StringEqualityToBoolConverter());
                    }

                    _initialized = true;
                    started.Set();

                    // Pump messages so WPF dispatcher stays alive
                    System.Windows.Threading.Dispatcher.Run();
                }
                catch (Exception ex)
                {
                    caught = ex;
                    started.Set();
                }
            })
            {
                IsBackground = true,
                Name = "HnVue.Test.WpfApplicationThread",
            };

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            started.Wait(TimeSpan.FromSeconds(10));

            if (caught is not null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(caught).Throw();
            }
        }
    }

    private static void AddDictionary(Application app, string packUri)
    {
        var dict = new ResourceDictionary { Source = new Uri(packUri, UriKind.Absolute) };
        app.Resources.MergedDictionaries.Add(dict);
    }

    /// <summary>
    /// Executes <paramref name="action"/> on the Application's dispatcher thread.
    /// Ensures the Application and theme are initialized before invoking.
    /// </summary>
    public static void InvokeOnUiThread(Action action)
    {
        EnsureInitialized();

        var app = Application.Current
            ?? throw new InvalidOperationException("WPF Application was not initialized.");

        app.Dispatcher.Invoke(action);
    }
}
