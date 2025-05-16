using System.Windows;
using RestaurantOnline.Models;
using RestaurantOnline.Converters;
using System.Linq;

namespace RestaurantOnline.Views
{
    /// <summary>
    /// Logica de interactiune pentru MenuDetailsView.xaml
    /// </summary>
    public partial class MenuDetailsView : Window
    {
        private readonly ImagePathConverter _imagePathConverter = new ImagePathConverter();
        
        public MenuDetailsView()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Seteaza detaliile meniului in dialogul de afisare
        /// </summary>
        /// <param name="meniu">Meniul pentru care se afiseaza detaliile</param>
        public void SetMenuDetails(Menu meniu)
        {
            if (meniu == null) return;
            
            // Seteaza detaliile de baza ale meniului
            this.Title = $"Detalii - {meniu.Name}";
            NumeMeniu.Text = meniu.Name;
            CategorieMeniu.Text = meniu.Category?.Name ?? "Categorie necunoscută";
            
            // Seteaza imaginile preparatelor din meniu
            if (meniu.MenuDishes != null && meniu.MenuDishes.Count > 0)
            {
                PozePreparat.ItemsSource = meniu.MenuDishes;
                
                // Seteaza lista de preparate
                PreparateListControl.ItemsSource = meniu.MenuDishes;
                
                // Colectează și afișează toți alergenii distincti din meniu
                var totiAlergenii = meniu.MenuDishes
                    .Where(md => md.Dish?.Allergens != null)
                    .SelectMany(md => md.Dish.Allergens)
                    .GroupBy(a => a.AllergenId)
                    .Select(g => g.First())
                    .OrderBy(a => a.Name)
                    .ToList();
                
                ListaAlergeniControl.ItemsSource = totiAlergenii;
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