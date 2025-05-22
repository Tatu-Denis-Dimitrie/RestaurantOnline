using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace RestaurantOnline.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string imagePath && !string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    string fullPath = imagePath;
                    if (!Path.IsPathRooted(imagePath))
                    {
                        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                        fullPath = Path.Combine(baseDir, imagePath);
                    }

                    if (File.Exists(fullPath))
            {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.UriSource = new Uri(fullPath);
                        image.EndInit();
                        return image;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 