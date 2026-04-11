using System;
using System.Linq;
using System.Windows;

namespace HnVue.UI.Services;

/// <summary>
/// Provides theme rollback capability for usability-driven UI change management.
/// When SUS score drops below threshold or critical usability failures are detected,
/// this service reverts the active WPF ResourceDictionary to the previous approved version.
///
/// Usage pattern (SPEC-UI-001 §UE-005, §UE-006):
///   - Current theme:  Themes/HnVueTheme.xaml       (actively loaded)
///   - Previous theme: Themes/HnVueTheme.previous.xaml (kept as rollback target)
///
/// Rollback trigger conditions (CHANGE_MANAGEMENT_PROCESS.md §6):
///   - SUS score drops below 78 (baseline: 82.3)
///   - Critical usability error occurs
///   - Task completion time exceeds baseline + 20%
/// </summary>
public class ThemeRollbackService
{
    private const string CurrentThemeFileName = "HnVueTheme.xaml";
    private const string PreviousThemeFileName = "HnVueTheme.previous.xaml";
    private const string PreviousThemePath = "/HnVue.UI;component/Themes/HnVueTheme.previous.xaml";

    /// <summary>
    /// Checks whether a previous theme snapshot is available for rollback.
    /// </summary>
    public static bool CanRollback
    {
        get
        {
            try
            {
                var dict = new ResourceDictionary { Source = new Uri(PreviousThemePath, UriKind.Relative) };
                return dict.Count > 0 || dict.MergedDictionaries.Count > 0;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Rolls back the active application theme to the previous approved version.
    /// Safe to call from any thread — marshals to the UI thread automatically.
    /// </summary>
    /// <returns>True if rollback succeeded; false if no previous theme is available.</returns>
    public static bool RollbackToPrevious()
    {
        var application = Application.Current;

        if (!CanRollback || application is null)
            return false;

        application.Dispatcher.Invoke(() =>
        {
            var mergedDicts = application.Resources.MergedDictionaries;

            // Find and remove the current active theme
            var currentTheme = mergedDicts.FirstOrDefault(d => HasThemeSource(d, CurrentThemeFileName));

            if (currentTheme != null)
            {
                mergedDicts.Remove(currentTheme);
            }

            var previousTheme = mergedDicts.FirstOrDefault(d => HasThemeSource(d, PreviousThemeFileName));
            if (previousTheme != null)
            {
                mergedDicts.Remove(previousTheme);
            }

            // Load and apply the previous approved theme
            mergedDicts.Add(new ResourceDictionary
            {
                Source = new Uri(PreviousThemePath, UriKind.Relative)
            });
        });

        return true;
    }

    /// <summary>
    /// Saves the current theme as a rollback snapshot (call before deploying a new theme version).
    /// </summary>
    public static void SaveCurrentAsSnapshot()
    {
        // Implementation note: copy HnVueTheme.xaml → HnVueTheme.previous.xaml
        // In production this is handled at deployment time, not at runtime.
        // This method is a placeholder for the deployment pipeline integration.
        //
        // Deployment pipeline step:
        //   cp src/HnVue.UI/Themes/HnVueTheme.xaml
        //      src/HnVue.UI/Themes/HnVueTheme.previous.xaml
        //   (before replacing HnVueTheme.xaml with new version)
    }

    private static bool HasThemeSource(ResourceDictionary dictionary, string fileName)
    {
        var source = dictionary.Source?.OriginalString;
        if (string.IsNullOrWhiteSpace(source))
        {
            return false;
        }

        return source.EndsWith($"/Themes/{fileName}", StringComparison.OrdinalIgnoreCase) ||
               source.EndsWith($@"\Themes\{fileName}", StringComparison.OrdinalIgnoreCase) ||
               source.EndsWith($"Themes/{fileName}", StringComparison.OrdinalIgnoreCase) ||
               source.EndsWith(fileName, StringComparison.OrdinalIgnoreCase);
    }
}
