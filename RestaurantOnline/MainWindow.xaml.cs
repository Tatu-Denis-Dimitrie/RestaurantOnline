using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using RestaurantOnline.Services;
using RestaurantOnline.ViewModels;

namespace RestaurantOnline
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PreparatService _preparatService;
        private readonly CategorieService _categorieService;
        private readonly UtilizatorService _utilizatorService;
        private readonly ComandaService _comandaService;

        public MainWindow(
            PreparatService preparatService,
            CategorieService categorieService,
            UtilizatorService utilizatorService,
            ComandaService comandaService)
        {
            _preparatService = preparatService;
            _categorieService = categorieService;
            _utilizatorService = utilizatorService;
            _comandaService = comandaService;

            InitializeComponent();
            DataContext = new MainViewModel(
                _preparatService,
                _categorieService,
                _utilizatorService,
                _comandaService);
        }
    }
}