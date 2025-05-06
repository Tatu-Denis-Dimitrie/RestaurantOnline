using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace RestaurantOnline.Views
{
    /// <summary>
    /// Interaction logic for PreparateView.xaml
    /// </summary>
    public partial class PreparateView : UserControl
    {
        public PreparateView()
        {
            InitializeComponent();
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Image img = sender as Image;
            Debug.WriteLine($"Eroare la încărcarea imaginii: {e.ErrorException.Message}");
            if (img != null && img.Source is BitmapImage bitmapImage)
            {
                Debug.WriteLine($"Sursa imaginii: {bitmapImage.UriSource}");
            }
        }
    }
} 