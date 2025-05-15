using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RestaurantOnline.Converters
{
    /// <summary>
    /// Converteste o valoare boolean in Visibility.Visible cand este true si Visibility.Collapsed cand este false
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            
            return Visibility.Collapsed;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            
            return false;
        }
    }
} 