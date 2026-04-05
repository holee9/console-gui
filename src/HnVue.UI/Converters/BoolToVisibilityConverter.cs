using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HnVue.UI.Converters;

/// <summary>
/// Converts a <see cref="bool"/> value to a <see cref="Visibility"/> value.
/// <c>true</c> maps to <see cref="Visibility.Visible"/>; <c>false</c> maps to <see cref="Visibility.Collapsed"/>.
/// Set <see cref="IsInverted"/> to <c>true</c> to reverse the mapping.
/// </summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public sealed class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>Gets or sets a value indicating whether the conversion should be inverted.</summary>
    public bool IsInverted { get; set; }

    /// <inheritdoc/>
    /// <remarks>Pass <c>ConverterParameter="Invert"</c> to reverse the mapping at the binding site.</remarks>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValue = value is bool b && b;
        var invert = IsInverted || (parameter is string p && p.Equals("Invert", StringComparison.OrdinalIgnoreCase));
        if (invert) boolValue = !boolValue;
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(BoolToVisibilityConverter)} does not support two-way binding.");
}
