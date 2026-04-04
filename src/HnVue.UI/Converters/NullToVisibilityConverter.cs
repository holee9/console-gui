using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HnVue.UI.Converters;

/// <summary>
/// Converts a <see langword="null"/> reference to a <see cref="Visibility"/> value.
/// Returns <see cref="Visibility.Collapsed"/> when the value is <see langword="null"/>;
/// otherwise returns <see cref="Visibility.Visible"/>.
/// </summary>
[ValueConversion(typeof(object), typeof(Visibility))]
public sealed class NullToVisibilityConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is null ? Visibility.Collapsed : Visibility.Visible;

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(NullToVisibilityConverter)} does not support two-way binding.");
}
