using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using RestaurantOnline.Models;

namespace RestaurantOnline.Converters
{
    public class StareToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StareComanda stare)
            {
                return stare switch
                {
                    StareComanda.inregistrata => new SolidColorBrush(Color.FromRgb(52, 152, 219)),    // Albastru
                    StareComanda.se_pregateste => new SolidColorBrush(Color.FromRgb(243, 156, 18)),   // Portocaliu
                    StareComanda.a_plecat_la_client => new SolidColorBrush(Color.FromRgb(155, 89, 182)), // Mov
                    StareComanda.livrata => new SolidColorBrush(Color.FromRgb(46, 204, 113)),         // Verde
                    StareComanda.anulata => new SolidColorBrush(Color.FromRgb(231, 76, 60)),          // Roșu
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