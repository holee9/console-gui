namespace HnVue.UI.Contracts.Theming;

/// <summary>
/// Describes an available UI theme.
/// </summary>
/// <param name="Id">Unique identifier for the theme.</param>
/// <param name="DisplayName">Human-readable theme name.</param>
/// <param name="IsDark">Whether this is a dark theme variant.</param>
public sealed record ThemeInfo(string Id, string DisplayName, bool IsDark);
