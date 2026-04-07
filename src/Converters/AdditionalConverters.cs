using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HnVue.UI.Converters
{
    /// <summary>
    /// Converts enum values to boolean for radio button group binding.
    /// </summary>
    [ValueConversion(typeof(Enum), typeof(bool))]
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.Equals(parameter) ?? false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : Binding.DoNothing;
        }
    }

    /// <summary>
    /// Converts enum values to display strings using Description attribute or ToString().
    /// </summary>
    [ValueConversion(typeof(Enum), typeof(string))]
    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            Type enumType = value.GetType();
            if (enumType.IsEnum)
            {
                // For Korean UI, we could use Description attributes
                // For now, return the enum name
                return value.ToString() ?? string.Empty;
            }
            return value.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts string to uppercase for consistent display.
    /// </summary>
    [ValueConversion(typeof(string), typeof(string))]
    public class StringToUpperConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString()?.ToUpper(CultureInfo.CurrentCulture) ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts null or empty strings to a placeholder value.
    /// </summary>
    [ValueConversion(typeof(string), typeof(string))]
    public class NullToEmptyConverter : IValueConverter
    {
        public string Placeholder { get; set; } = "-";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? Placeholder : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    /// <summary>
    /// Converts boolean to inverted boolean.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue && !boolValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue && !boolValue;
        }
    }

    /// <summary>
    /// Converts PatientSex enum to Korean display string.
    /// </summary>
    [ValueConversion(typeof(PatientSex), typeof(string))]
    public class SexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PatientSex sex)
            {
                return sex switch
                {
                    PatientSex.Male => "남성",
                    PatientSex.Female => "여성",
                    PatientSex.Other => "기타",
                    _ => "미상"
                };
            }
            return "미상";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts ToastType to appropriate color brush.
    /// </summary>
    [ValueConversion(typeof(ToastType), typeof(Brush))]
    public class ToastTypeToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 4 && values[0] is Brush success &&
                values[1] is Brush warning && values[2] is Brush error &&
                values[3] is Brush info)
            {
                // This would be used with actual ToastType binding
                return info;
            }
            return Brushes.Gray;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts boolean values with multiple parameters for complex scenarios.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BoolToBrushConverter : IValueConverter
    {
        public Brush TrueBrush { get; set; } = Brushes.Green;
        public Brush FalseBrush { get; set; } = Brushes.Gray;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue && boolValue ? TrueBrush : FalseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts DateTime to relative time string (e.g., "5 minutes ago").
    /// </summary>
    [ValueConversion(typeof(DateTime), typeof(string))]
    public class RelativeTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                TimeSpan diff = DateTime.Now - dateTime;

                return diff.TotalSeconds < 60 ? "방금 전" :
                       diff.TotalMinutes < 60 ? $"{(int)diff.TotalMinutes}분 전" :
                       diff.TotalHours < 24 ? $"{(int)diff.TotalHours}시간 전" :
                       diff.TotalDays < 7 ? $"{(int)diff.TotalDays}일 전" :
                       dateTime.ToString("yyyy-MM-dd", culture);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

/// <summary>
/// Patient sex enumeration for converter use.
/// </summary>
public enum PatientSex
{
    Unknown,
    Male,
    Female,
    Other
}

/// <summary>
/// Toast type enumeration for converter use.
/// </summary>
public enum ToastType
{
    Success,
    Warning,
    Error,
    Info
}
