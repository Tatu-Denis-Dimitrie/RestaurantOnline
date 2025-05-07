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
                    // Verifica daca calea este absoluta sau relativa
                    string fullPath = imagePath;
                    if (!Path.IsPathRooted(imagePath))
                    {
                        // Calea este relativa, adauga directorul aplicatiei
                        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                        fullPath = Path.Combine(baseDir, imagePath);
                    }

                    // Verifica daca fisierul exista
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
                    System.Diagnostics.Debug.WriteLine($"Eroare la incarcarea imaginii: {ex.Message}");
                }
            }

            // Returneaza o imagine implicita sau null
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 