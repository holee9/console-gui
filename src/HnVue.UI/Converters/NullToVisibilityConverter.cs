using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HnVue.UI.Converters;

/// <summary>
/// Converts a null value to <see cref="Visibility.Collapsed"/> and a non-null value to <see cref="Visibility.Visible"/>.
/// Used to show/hide UI elements based on whether a binding value is null.
/// </summary>
[ValueConversion(typeof(object), typeof(Visibility))]
public sealed class NullToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a value: returns <see cref="Visibility.Visible"/> when the value is non-null,
    /// <see cref="Visibility.Collapsed"/> when null.
    /// </summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is null ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>Not supported. Throws <see cref="NotSupportedException"/>.</summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(NullToVisibilityConverter)} does not support ConvertBack.");
}
