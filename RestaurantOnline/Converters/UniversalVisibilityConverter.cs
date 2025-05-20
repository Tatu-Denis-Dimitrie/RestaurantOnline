using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RestaurantOnline.Converters
{
    public class UniversalVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = false;
            bool invert = parameter is string paramStr && paramStr.ToLower() == "invert";
            
            // Procesează în funcție de tipul de valoare
            if (value == null)
            {
                isVisible = false;
            }
            else if (value is bool boolValue)
            {
                isVisible = boolValue;
            }
            else if (value is string strValue)
            {
                isVisible = !string.IsNullOrEmpty(strValue);
            }
            else if (value is int intValue)
            {
                isVisible = intValue > 0;
            }
            else if (value is ICollection collection)
            {
                isVisible = collection.Count > 0;
            }
            else
            {
                isVisible = true; // Alte tipuri de obiecte non-null considerăm că sunt vizibile
            }
            
            // Aplică inversa dacă este cerut
            if (invert)
            {
                isVisible = !isVisible;
            }
            
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool isVisible = visibility == Visibility.Visible;
                bool invert = parameter is string paramStr && paramStr.ToLower() == "invert";
                
                if (invert)
                {
                    isVisible = !isVisible;
                }
                
                if (targetType == typeof(bool))
                {
                    return isVisible;
                }
            }
            
            throw new NotImplementedException();
        }
    }
} 