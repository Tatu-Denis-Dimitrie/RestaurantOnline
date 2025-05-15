using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RestaurantOnline.Converters
{
    /// <summary>
    /// Converteste o valoare in Visibility.Visible cand este egala cu parametrul specificat si Visibility.Collapsed cand nu este
    /// </summary>
    public class EqualityToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;
                
            bool isEqual = value.ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase);
            return isEqual ? Visibility.Visible : Visibility.Collapsed;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 