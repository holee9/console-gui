using System.Globalization;
using System.Windows.Data;

namespace HnVue.UI.Converters;

/// <summary>
/// Converts a <see cref="bool"/> value to its inverse.
/// <c>true</c> maps to <c>false</c> and vice versa.
/// Intended for use with <c>IsEnabled</c> bindings where the bound property
/// represents a "loading" or "busy" state (e.g., IsLoading → IsEnabled = !IsLoading).
/// </summary>
[ValueConversion(typeof(bool), typeof(bool))]
public sealed class InverseBoolConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? !b : true;

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? !b : true;
}
