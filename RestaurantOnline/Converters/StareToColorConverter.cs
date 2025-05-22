using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RestaurantOnline.Converters
{
    public class StareToColorConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter as string == "stocklimit" && value is int totalQuantityGrams)
            {
                var app = App.Current as App;
                var stockThreshold = app?.AppSettings?.StockThreshold ?? 1000;
                
                if (totalQuantityGrams <= stockThreshold)
                {
                    return new SolidColorBrush(Colors.Red);
                }
                
                return new SolidColorBrush(Colors.Green);
            }
            
            if (value is string stare)
            {
                return stare switch
                {
                    "inregistrata" => new SolidColorBrush(Colors.Blue),
                    "se_pregateste" => new SolidColorBrush(Colors.Orange),
                    "a_plecat_la_client" => new SolidColorBrush(Colors.Purple),
                    "livrata" => new SolidColorBrush(Colors.Green),
                    "anulata" => new SolidColorBrush(Colors.Red),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            
            return new SolidColorBrush(Colors.Gray);
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
            return new SolidColorBrush(Colors.Gray);
        }
        
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 