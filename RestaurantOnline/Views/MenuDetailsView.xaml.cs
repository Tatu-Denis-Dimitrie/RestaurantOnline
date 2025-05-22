using System.Windows;
using RestaurantOnline.Models;
using RestaurantOnline.Converters;
using System.Linq;

namespace RestaurantOnline.Views
{
    public partial class MenuDetailsView : Window
    {
        private readonly ImagePathConverter _imagePathConverter = new ImagePathConverter();
        
        public MenuDetailsView()
        {
            InitializeComponent();
        }
        
        public void SetMenuDetails(Menu meniu)
        {
            if (meniu == null) return;
            
            this.Title = $"Detalii - {meniu.Name}";
            NumeMeniu.Text = meniu.Name;
            CategorieMeniu.Text = meniu.Category?.Name ?? "Categorie necunoscută";
            
            if (meniu.MenuDishes != null && meniu.MenuDishes.Count > 0)
            {
                PozePreparat.ItemsSource = meniu.MenuDishes;
                
                PreparateListControl.ItemsSource = meniu.MenuDishes;
                
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
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
} 