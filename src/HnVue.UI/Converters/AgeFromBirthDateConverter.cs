using System.Globalization;
using System.Windows.Data;
using HnVue.Common.Models;

namespace HnVue.UI.Converters;

/// <summary>
/// Converts a <see cref="DateOnly"/>? value (patient date of birth) to an age string
/// formatted as "{n}Y" (e.g., "37Y"). Returns "-" when the value is null or not a DateOnly.
/// </summary>
[ValueConversion(typeof(DateOnly?), typeof(string))]
public sealed class AgeFromBirthDateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateOnly dob)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var years = today.Year - dob.Year;
            if (dob.AddYears(years) > today) years--;
            return $"{years}Y";
        }
        return "-";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => System.Windows.Data.Binding.DoNothing;
}

/// <summary>
/// Converts a <see cref="DateOnly"/>? to a display string formatted as "yyyy-MM-dd".
/// Returns "-" when the value is null.
/// </summary>
[ValueConversion(typeof(DateOnly?), typeof(string))]
public sealed class DateOnlyToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is DateOnly d ? d.ToString("yyyy-MM-dd", culture) : "-";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => System.Windows.Data.Binding.DoNothing;
}
