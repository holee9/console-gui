using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HnVue.UI.Converters;

/// <summary>
/// Design token resource keys for dynamic theme-aware color resolution.
/// References HnVue.Semantic.Status.* SolidColorBrush resources defined in Themes/tokens/SemanticTokens.xaml.
/// </summary>
internal static class DesignTokenResources
{
    /// <summary>Maps status enum strings to semantic brush resource keys.</summary>
    private static readonly Dictionary<string, string> StatusToResourceKey = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Safe", "HnVue.Semantic.Status.Safe" },
        { "Warning", "HnVue.Semantic.Status.Warning" },
        { "Error", "HnVue.Semantic.Status.Emergency" },
        { "Emergency", "HnVue.Semantic.Status.Emergency" },
        { "Info", "HnVue.Semantic.Brand.Accent" },
        { "Online", "HnVue.Semantic.Status.Safe" },
        { "Offline", "HnVue.Semantic.Status.Emergency" },
        { "Busy", "HnVue.Semantic.Brand.Accent" },
        { "Blocked", "HnVue.Semantic.Status.Blocked" }
    };

    /// <summary>
    /// Resolves a status string to its theme-aware brush using design token resources.
    /// Falls back to gray brush if resource not found or Application.Current is null (DesignTime/Test environment).
    /// </summary>
    public static Brush? ResolveStatusBrush(string statusKey)
    {
        if (StatusToResourceKey.TryGetValue(statusKey, out var resourceKey))
        {
            var app = Application.Current;
            if (app == null) return Brushes.Gray; // DesignTime/Test fallback
            if (app.TryFindResource(resourceKey) is Brush brush)
                return brush;
        }
        return Brushes.Gray;
    }
}

// BoolToVisibilityConverter and NullToVisibilityConverter are defined in
// their own files (BoolToVisibilityConverter.cs, NullToVisibilityConverter.cs).
// They are intentionally NOT redefined here to avoid CS0101 duplicate type errors.

/// <summary>
/// Inverted boolean to Visibility converter.
/// True => Collapsed, False => Visible
/// </summary>
public class InvertedBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is false ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility.Collapsed;
    }
}

/// <summary>
/// Converts null values to Visibility (inverted).
/// Null => Visible, Not null => Collapsed
/// </summary>
public class NullToCollapsedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // One-way converter: Collapsed => null, Visible => non-null sentinel
        return value is Visibility.Collapsed ? null : string.Empty;
    }
}

/// <summary>
/// Converts integer count to Visibility.
/// Count > 0 => Visible, Count = 0 => Collapsed
/// </summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
            return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // One-way converter: Visible => 1, Collapsed => 0
        return value is Visibility.Visible ? 1 : 0;
    }
}

/// <summary>
/// Converts empty string to Visibility.
/// Not empty => Visible, Empty => Collapsed
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is string str && !string.IsNullOrWhiteSpace(str)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // One-way converter: Visible => non-empty string, Collapsed => empty
        return value is Visibility.Visible ? "visible" : string.Empty;
    }
}

/// <summary>
/// Converts enum to brush for status indicators.
/// Uses design token resources for theme-aware color resolution.
/// Supports Light/Dark/High Contrast themes via DynamicResource lookup.
/// </summary>
public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue)
        {
            var key = enumValue.ToString();
            return DesignTokenResources.ResolveStatusBrush(key) ?? Brushes.Gray;
        }
        return Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // One-way converter: Brush to string conversion not supported
        // Status should be set directly, not derived from brush color
        throw new NotSupportedException($"{nameof(StatusToBrushConverter)} does not support two-way binding.");
    }
}

/// <summary>
/// Converts a string value to Visibility by comparing it against the converter parameter.
/// Used for top-tab and sub-tab content panels in SettingsView.
/// Returns Visible when value equals parameter (case-insensitive), Collapsed otherwise.
/// </summary>
public class ActiveTabToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string activeTab && parameter is string tabKey)
            return string.Equals(activeTab, tabKey, StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(ActiveTabToVisibilityConverter)} does not support two-way binding.");
}

/// <summary>
/// Converts a string value to bool by comparing it against the converter parameter.
/// Used for ToggleButton IsChecked binding in tab rows.
/// Returns true when value equals parameter (case-insensitive), false otherwise.
/// </summary>
public class StringEqualityToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string current && parameter is string expected)
            return string.Equals(current, expected, StringComparison.OrdinalIgnoreCase);
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(StringEqualityToBoolConverter)} does not support two-way binding.");
}

/// <summary>
/// Multi-converter for boolean AND logic.
/// All values must be true to return true.
/// </summary>
public class MultiBoolAndConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return values.OfType<bool>().All(b => b);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        // One-way converter: boolean to array conversion not supported
        // Input values should be set directly via individual bindings
        throw new NotSupportedException($"{nameof(MultiBoolAndConverter)} does not support two-way binding.");
    }
}

/// <summary>
/// Multi-converter for boolean OR logic.
/// At least one value must be true to return true.
/// </summary>
public class MultiBoolOrConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return values.OfType<bool>().Any(b => b);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        // One-way converter: boolean to array conversion not supported
        // Input values should be set directly via individual bindings
        throw new NotSupportedException($"{nameof(MultiBoolOrConverter)} does not support two-way binding.");
    }
}
