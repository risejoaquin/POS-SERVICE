using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PosBuilder
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter?.ToString() == "Invert")
            {
                if (value is bool b)
                    return b ? Visibility.Collapsed : Visibility.Visible;
            }
            if (value is bool val)
                return val ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
