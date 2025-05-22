using System.Windows.Input;
using RestaurantOnline.Services;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantOnline.ViewModels
{
    public class AdminViewModel : ViewModelBase
    {
        private readonly DishS _dishService;
        private readonly CategoryS _categoryService;
        private readonly AllergenS _allergenService;
        private readonly UserS _userService;
        private readonly OrderS _orderService;
        private readonly MenuService _menuService;
        private readonly MainViewModel _mainViewModel;
        private ViewModelBase _currentTabViewModel;
        private int _selectedTabIndex;

        public AdminViewModel(
            DishS dishService,
            CategoryS categoryService,
            AllergenS allergenService,
            UserS userService,
            OrderS orderService,
            MainViewModel mainViewModel)
        {
            _dishService = dishService;
            _categoryService = categoryService;
            _allergenService = allergenService;
            _userService = userService;
            _orderService = orderService;
            _mainViewModel = mainViewModel;
            _menuService = ((App)System.Windows.Application.Current).ServiceProvider.GetRequiredService<MenuService>();

            AdaugaPreparatCommand = new RelayCommand(_ => ShowAdaugaPreparat());
            EditarePreparatCommand = new RelayCommand(_ => ShowEditarePreparat());
            AdaugaMeniuCommand = new RelayCommand(_ => ShowAdaugaMeniu());
            ComenziCommand = new RelayCommand(_ => ShowComenzi());
            UtilizatoriCommand = new RelayCommand(_ => ShowUtilizatori());
            CategoriiCommand = new RelayCommand(_ => ShowCategorii());
            AlergeniCommand = new RelayCommand(_ => ShowAlergeni());
            StockCommand = new RelayCommand(_ => ShowStock());
            InapoiCommand = new RelayCommand(_ => _mainViewModel.NavigateToHome());

            _selectedTabIndex = 0;
            ShowAdaugaPreparat();
        }

        public ViewModelBase CurrentTabViewModel
        {
            get => _currentTabViewModel;
            set => SetProperty(ref _currentTabViewModel, value);
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (SetProperty(ref _selectedTabIndex, value))
                {
                    UpdateTabContent();
                }
            }
        }

        public ICommand AdaugaPreparatCommand { get; }
        public ICommand EditarePreparatCommand { get; }
        public ICommand AdaugaMeniuCommand { get; }
        public ICommand ComenziCommand { get; }
        public ICommand UtilizatoriCommand { get; }
        public ICommand CategoriiCommand { get; }
        public ICommand AlergeniCommand { get; }
        public ICommand StockCommand { get; }
        public ICommand InapoiCommand { get; }

        private void UpdateTabContent()
        {
            switch (_selectedTabIndex)
            {
                case 0:
                    ShowAdaugaPreparat();
                    break;
                case 1:
                    ShowEditarePreparat();
                    break;
                case 2:
                    ShowAdaugaMeniu();
                    break;
                case 3:
                    ShowComenzi();
                    break;
                case 4:
                    ShowUtilizatori();
                    break;
                case 5:
                    ShowCategorii();
                    break;
                case 6:
                    ShowAlergeni();
                    break;
                case 7:
                    ShowStock();
                    break;
                default:
                    ShowAdaugaPreparat();
                    break;
            }
        }

        private void ShowAdaugaPreparat()
        {
            CurrentTabViewModel = new AddDishViewModel(_dishService, _categoryService, _allergenService, _mainViewModel);
        }

        private void ShowEditarePreparat()
        {
            CurrentTabViewModel = new EditDishViewModel(_dishService, _categoryService, _allergenService, _mainViewModel);
        }

        private void ShowAdaugaMeniu()
        {
            CurrentTabViewModel = new AddMenuViewModel(_menuService, _dishService, _categoryService, _mainViewModel);
        }

        private void ShowComenzi()
        {
            CurrentTabViewModel = new ComenziViewModel(_orderService, _dishService, _userService);
        }

        private void ShowUtilizatori()
        {
            CurrentTabViewModel = new UtilizatoriViewModel(_userService);
        }

        private void ShowCategorii()
        {
            CurrentTabViewModel = new CategoryViewModel(_categoryService);
        }

        private void ShowAlergeni()
        {
            CurrentTabViewModel = new AllergenViewModel(_allergenService);
        }

        private void ShowStock()
        {
            CurrentTabViewModel = new StockViewModel(_dishService);
        }
    }
}