using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RestaurantOnline.Converters
{
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            if (value is int count && count > 0)
                return Visibility.Visible;

            if (value is System.Collections.ICollection collection && collection.Count > 0)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 