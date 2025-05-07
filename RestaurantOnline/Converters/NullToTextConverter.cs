using System;
using System.Globalization;
using System.Windows.Data;

namespace RestaurantOnline.Converters
{
    public class NullToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string options)
            {
                var parts = options.Split('|');
                if (parts.Length == 2)
                {
                    return value == null ? parts[0] : parts[1];
                }
            }
            
            return value == null ? "Nu exista" : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 