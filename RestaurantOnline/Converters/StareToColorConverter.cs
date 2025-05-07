using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RestaurantOnline.Converters
{
    public class StareToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
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
    }
} 