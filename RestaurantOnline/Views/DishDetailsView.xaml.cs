using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using RestaurantOnline.Models;
using RestaurantOnline.Converters;

namespace RestaurantOnline.Views
{
    /// <summary>
    /// Logica de interactiune pentru DetaliiPreparatDialog.xaml
    /// </summary>
    public partial class DishDetailsView : Window
    {
        private readonly ImagePathConverter _imagePathConverter = new ImagePathConverter();
        
        public DishDetailsView()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Seteaza detaliile preparatului in dialogul de afisare
        /// </summary>
        /// <param name="preparat">Preparatul pentru care se afiseaza detaliile</param>
        public void SetPreparatDetails(Dish preparat)
        {
            if (preparat == null) return;
            
            // Seteaza detaliile de baza
            this.Title = $"Detalii - {preparat.Denumire}";
            NumePreparat.Text = preparat.Denumire;
            CategoriePreparat.Text = preparat.Categorie?.Nume ?? "Categorie necunoscuta";
            PretPreparat.Text = $"{preparat.Pret:F2} lei";
            CantitatePortie.Text = $"{preparat.CantitatePortieGrame} g";
            CantitateDisponibila.Text = $"{preparat.CantitateTotalaGrame} g";
            
            // Seteaza imaginea
            if (preparat.Fotografii != null && preparat.Fotografii.Count > 0)
            {
                var fotografie = preparat.Fotografii[0];
                ImaginePreparatControl.Source = _imagePathConverter.Convert(
                    fotografie.Url, typeof(BitmapImage), null, null) as BitmapImage;
            }
            
            // Seteaza alergenii
            if (preparat.Alergeni != null)
            {
                var alergeniList = new List<Allergen>();
                foreach (var alergen in preparat.Alergeni)
                {
                    alergeniList.Add(alergen);
                }
                
                AlergeniListControl.ItemsSource = alergeniList;
            }
        }
        
        /// <summary>
        /// Handler pentru butonul de inchidere al dialogului
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
} 