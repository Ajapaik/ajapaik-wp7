using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ajapaik.Helpers
{
    /// <summary>
    /// Converts a boolean value to a <see cref="Visibility"/> value, 
    /// and vice versa.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            string paramValue = (string)parameter;

            if (value == null || (bool)value)
            {
                return paramValue == "Collapsed"
                    ? Visibility.Collapsed : Visibility.Visible;
            }

            return paramValue == "Collapsed"
                ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            string paramValue = (string)parameter;
            if (value == null || (Visibility)value == Visibility.Visible)
            {
                return paramValue != "Collapsed";
            }

            return paramValue == "Collapsed";
        }
    }
}