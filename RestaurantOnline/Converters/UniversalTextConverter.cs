using System;
using System.Globalization;
using System.Windows.Data;

namespace RestaurantOnline.Converters
{
    public class UniversalTextConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string options)
            {
                var parts = options.Split('|');
                
                if (value == null && parts.Length >= 1)
                {
                    return parts[0]; 
                }
                
                if (value is bool boolValue && parts.Length >= 2)
                {
                    return boolValue ? parts[0] : parts[1]; 
                }
                
                if (value != null && parts.Length >= 2)
                {
                    string format = parts[1];
                    if (format.Contains("{0}"))
                    {
                        return string.Format(format, value);
                    }
                    return format;
                }
            }
            
            if (value == null)
            {
                return "Lipsă";
            }
            
            return value.ToString();
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length > 0 && values[0] != null)
            {
                return Convert(values[0], targetType, parameter, culture);
            }
            
            return "N/A";
        }
        
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 