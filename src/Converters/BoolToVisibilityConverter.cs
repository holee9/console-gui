using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HnVue.UI.Converters
{
    /// <summary>
    /// Converts boolean values to Visibility with optional inversion.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets whether to invert the boolean logic.
        /// </summary>
        public bool Invert { get; set; }

        /// <summary>
        /// Gets or sets the visibility to use when false (default is Collapsed).
        /// </summary>
        public Visibility FalseVisibility { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                bool result = Invert ? !boolValue : boolValue;
                return result ? Visibility.Visible : FalseVisibility;
            }
            return FalseVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool result = visibility == Visibility.Visible;
                return Invert ? !result : result;
            }
            return false;
        }
    }

    /// <summary>
    /// Converts null values to Visibility (Collapses when null).
    /// </summary>
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NullToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }
        public Visibility FalseVisibility { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = value == null;
            bool result = Invert ? isNull : !isNull;
            return result ? Visibility.Visible : FalseVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
