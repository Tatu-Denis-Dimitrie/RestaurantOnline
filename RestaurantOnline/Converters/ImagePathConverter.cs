using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace RestaurantOnline.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            string imagePath = value.ToString();

            if (string.IsNullOrEmpty(imagePath))
                return null;

            Debug.WriteLine($"Cale imagine originală: {imagePath}");

            if (imagePath.StartsWith("/"))
                imagePath = imagePath.Substring(1);

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(baseDirectory, imagePath);
            
            Debug.WriteLine($"Cale completă: {fullPath}");
            Debug.WriteLine($"Fișierul există: {File.Exists(fullPath)}");

            if (!File.Exists(fullPath))
            {
                string projectDir = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\..\"));
                string projectPath = Path.Combine(projectDir, imagePath);
                
                Debug.WriteLine($"Cale alternativă în directorul de proiect: {projectPath}");
                Debug.WriteLine($"Fișierul există: {File.Exists(projectPath)}");
                
                if (File.Exists(projectPath))
                    return projectPath;

                string altPath = Path.Combine(baseDirectory, "Imagini", Path.GetFileName(imagePath));
                Debug.WriteLine($"Cale alternativă în /Imagini: {altPath}");
                Debug.WriteLine($"Fișierul există: {File.Exists(altPath)}");
                
                if (File.Exists(altPath))
                    return altPath;
            }
            else
            {
                return fullPath;
            }

            return imagePath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 