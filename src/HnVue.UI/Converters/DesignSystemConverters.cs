using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HnVue.UI.Converters;

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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts enum to brush for status indicators.
/// </summary>
public class StatusToBrushConverter : IValueConverter
{
    private static readonly Dictionary<string, Brush> StatusBrushes = new()
    {
        { "Safe", new SolidColorBrush(Color.FromRgb(46, 213, 115)) },
        { "Warning", new SolidColorBrush(Color.FromRgb(255, 165, 2)) },
        { "Error", new SolidColorBrush(Color.FromRgb(255, 71, 87)) },
        { "Info", new SolidColorBrush(Color.FromRgb(30, 144, 255)) },
        { "Online", new SolidColorBrush(Color.FromRgb(46, 213, 115)) },
        { "Offline", new SolidColorBrush(Color.FromRgb(255, 71, 87)) },
        { "Busy", new SolidColorBrush(Color.FromRgb(30, 144, 255)) },
        { "Blocked", new SolidColorBrush(Color.FromRgb(255, 109, 0)) }
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue)
        {
            var key = enumValue.ToString();
            return StatusBrushes.TryGetValue(key, out var brush) ? brush : Brushes.Gray;
        }
        return Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }
}
