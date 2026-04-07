namespace HnVue.UI.Contracts.Theming;

/// <summary>
/// Defines the contract for runtime theme switching.
/// </summary>
// @MX:NOTE Theme switching uses MahApps.Metro ThemeManager; supports Dark/Light/HighContrast themes
public interface IThemeService
{
    /// <summary>Gets the currently active theme.</summary>
    ThemeInfo CurrentTheme { get; }

    /// <summary>Gets all available themes.</summary>
    IReadOnlyList<ThemeInfo> AvailableThemes { get; }

    /// <summary>Applies the specified theme at runtime.</summary>
    void ApplyTheme(ThemeInfo theme);

    /// <summary>Raised when the active theme changes.</summary>
    event EventHandler<ThemeInfo>? ThemeChanged;
}
