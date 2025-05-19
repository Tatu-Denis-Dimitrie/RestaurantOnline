using System;
using System.Globalization;
using System.Windows.Data;

namespace RestaurantOnline.Converters
{
    /// <summary>
    /// Un convertor universal pentru text care poate gestiona diferite tipuri de valori
    /// și le poate converti în text cu opțiuni pentru diferite cazuri
    /// </summary>
    public class UniversalTextConverter : IValueConverter, IMultiValueConverter
    {
        // Implementare IValueConverter
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Formatare cu parametru
            if (parameter is string options)
            {
                var parts = options.Split('|');
                
                if (value == null && parts.Length >= 1)
                {
                    return parts[0]; // Textul pentru valoare null
                }
                
                if (value is bool boolValue && parts.Length >= 2)
                {
                    return boolValue ? parts[0] : parts[1]; // Texte pentru true/false
                }
                
                // Dacă avem o valoare non-null și cel puțin 2 părți, a doua parte este pentru non-null
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
            
            // Comportament implicit
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
        
        // Implementare IMultiValueConverter (păstrată pentru compatibilitate înapoi)
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Pentru alte tipuri de conversii multi-valoare
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