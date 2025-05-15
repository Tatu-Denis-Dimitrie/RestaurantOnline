using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RestaurantOnline.Converters
{
    /// <summary>
    /// Converteste o valoare boolean in Visibility.Collapsed cand este true si Visibility.Visible cand este false
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            
            return Visibility.Visible;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Collapsed;
            }
            
            return false;
        }
    }
} 