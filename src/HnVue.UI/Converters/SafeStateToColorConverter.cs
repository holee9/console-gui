using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HnVue.Common.Enums;

namespace HnVue.UI.Converters;

// @MX:NOTE Safety-critical color coding for equipment state (SWR-NF-SC-041); visual feedback for operators
/// <summary>
/// Converts a <see cref="SafeState"/> value to a <see cref="SolidColorBrush"/>
/// for the safety state indicator bar.
/// Colour scheme: Idle=Green, Warning=Yellow, Degraded=Orange, Blocked=DarkOrange, Emergency=Red.
/// SWR-NF-SC-041 / Issue #31.
/// </summary>
[ValueConversion(typeof(SafeState), typeof(SolidColorBrush))]
public sealed class SafeStateToColorConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not SafeState state)
            return Brushes.Gray;

        return state switch
        {
            SafeState.Idle => new SolidColorBrush(Color.FromRgb(0x00, 0xC8, 0x53)),     // #00C853 Green
            SafeState.Warning => new SolidColorBrush(Color.FromRgb(0xFF, 0xD6, 0x00)),   // #FFD600 Yellow
            SafeState.Degraded => new SolidColorBrush(Color.FromRgb(0xFF, 0x6D, 0x00)),  // #FF6D00 Deep Orange
            SafeState.Blocked => new SolidColorBrush(Color.FromRgb(0xE6, 0x5C, 0x00)),   // #E65C00 Dark Orange
            SafeState.Emergency => new SolidColorBrush(Color.FromRgb(0xD5, 0x00, 0x00)), // #D50000 Red
            _ => Brushes.Gray
        };
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException("SafeStateToColorConverter does not support ConvertBack.");
}
