using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using RestaurantOnline.Models;
using RestaurantOnline.Services;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace RestaurantOnline.ViewModels
{
    public class AddMenuViewModel : ViewModelBase
    {
        private readonly IRestaurantS<Menu> _menuService;
        private readonly DishS _dishService;
        private readonly CategoryS _categoryService;
        private readonly MainViewModel _mainViewModel;

        private string _name = string.Empty;
        private Category? _selectedCategory;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<DishItemViewModel> _allDishes;
        private ObservableCollection<DishItemViewModel> _selectedDishes;
        private decimal _discountPercent = 0;
        private string _searchTerm = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;
        private SemaphoreSlim _loadingSemaphore = new SemaphoreSlim(1, 1);

        public AddMenuViewModel(IRestaurantS<Menu> menuService, DishS dishService, CategoryS categoryService, MainViewModel mainViewModel)
        {
            _menuService = menuService;
            _dishService = dishService;
            _categoryService = categoryService;
            _mainViewModel = mainViewModel;

            _categories = new ObservableCollection<Category>();
            _allDishes = new ObservableCollection<DishItemViewModel>();
            _selectedDishes = new ObservableCollection<DishItemViewModel>();

            SaveCommand = new RelayCommand(_ => SaveMenu(), _ => CanSaveMenu());
            CancelCommand = new RelayCommand(_ => CancelAdd());
            AddDishCommand = new RelayCommand(d => AddDishToMenu(d as DishItemViewModel));
            RemoveDishCommand = new RelayCommand(d => RemoveDishFromMenu(d as DishItemViewModel));
            SearchCommand = new RelayCommand(_ => SearchDishes());

            // Încărcăm datele când se creează pagina
            _ = InitializeDataAsync();
        }

        // Proprietăți
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public decimal DiscountPercent
        {
            get => _discountPercent;
            set => SetProperty(ref _discountPercent, value);
        }

        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public ObservableCollection<DishItemViewModel> AllDishes
        {
            get => _allDishes;
            set => SetProperty(ref _allDishes, value);
        }

        public ObservableCollection<DishItemViewModel> SelectedDishes
        {
            get => _selectedDishes;
            set => SetProperty(ref _selectedDishes, value);
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public decimal TotalPrice => SelectedDishes.Sum(d => d.Dish?.Price ?? 0);

        public decimal DiscountedPrice => TotalPrice * (1 - DiscountPercent / 100);

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddDishCommand { get; }
        public ICommand RemoveDishCommand { get; }
        public ICommand SearchCommand { get; }

        private async Task InitializeDataAsync()
        {
            await _loadingSemaphore.WaitAsync();
            try
            {
                IsLoading = true;
                await LoadCategoriesAsync();
                await LoadDishesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la inițializarea datelor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                _loadingSemaphore.Release();
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                Categories.Clear();

                foreach (var category in categories)
                {
                    Categories.Add(category);
                }

                if (Categories.Count > 0)
                {
                    SelectedCategory = Categories.First();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la încărcarea categoriilor: {ex.Message}";
            }
        }

        private async Task LoadDishesAsync()
        {
            try
            {
                var dishes = await _dishService.GetAllAsync();
                AllDishes.Clear();

                foreach (var dish in dishes)
                {
                    AllDishes.Add(new DishItemViewModel { Dish = dish });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la încărcarea preparatelor: {ex.Message}";
            }
        }

        private void SearchDishes()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                // Dacă termenul de căutare este gol, afișăm toate preparatele
                foreach (var dishVM in AllDishes)
                {
                    dishVM.IsVisible = true;
                }
                return;
            }

            var searchTermLower = SearchTerm.ToLower();

            // Filtrăm preparatele după numele care conține termenul de căutare
            foreach (var dishVM in AllDishes)
            {
                dishVM.IsVisible = dishVM.Dish?.Name?.ToLower().Contains(searchTermLower) ?? false;
            }
        }

        private void AddDishToMenu(DishItemViewModel dishVM)
        {
            if (dishVM == null || dishVM.Dish == null) return;

            // Verificăm dacă preparatul este deja adăugat în meniu
            var existingDish = SelectedDishes.FirstOrDefault(d => d.Dish?.DishId == dishVM.Dish.DishId);
            if (existingDish == null)
            {
                var newDishVM = new DishItemViewModel
                {
                    Dish = dishVM.Dish,
                    Quantity = 1
                };
                SelectedDishes.Add(newDishVM);

                // Notificăm modificarea prețului total
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(DiscountedPrice));
            }
        }

        private void RemoveDishFromMenu(DishItemViewModel dishVM)
        {
            if (dishVM == null || dishVM.Dish == null) return;

            SelectedDishes.Remove(dishVM);

            // Notificăm modificarea prețului total
            OnPropertyChanged(nameof(TotalPrice));
            OnPropertyChanged(nameof(DiscountedPrice));
        }

        private bool CanSaveMenu()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   SelectedCategory != null &&
                   SelectedDishes.Count > 0 &&
                   !IsLoading;
        }

        private async void SaveMenu()
        {
            if (IsLoading) return;

            if (string.IsNullOrWhiteSpace(Name) || SelectedCategory == null || SelectedDishes.Count == 0)
            {
                ErrorMessage = "Toate câmpurile sunt obligatorii și trebuie să selectați cel puțin un preparat.";
                return;
            }

            try
            {
                IsLoading = true;

                var menu = new Menu { Name = Name, CategoryId = SelectedCategory.CategoryId, DiscountPercent = 0 };

                // Adăugăm preparatele selectate în meniu
                foreach (var dishVM in SelectedDishes)
                {
                    if (dishVM.Dish != null)
                    {
                        menu.MenuDishes.Add(new MenuDish
                        {
                            DishId = dishVM.Dish.DishId,
                            QuantityGrams = dishVM.Quantity
                        });
                    }
                }

                // Salvăm meniul în baza de date
                try
                {
                    await _menuService.AddAsync(menu);

                    // Afișăm mesaj de succes
                    MessageBox.Show("Meniul a fost adăugat cu succes în baza de date!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Navigăm înapoi la lista de meniuri
                    _mainViewModel.NavigateToMenus();
                }
                catch (Exception ex)
                {
                    var innerException = ex.InnerException;
                    string errorDetails = ex.Message;

                    while (innerException != null)
                    {
                        errorDetails += $"\n{innerException.Message}";
                        innerException = innerException.InnerException;
                    }

                    ErrorMessage = $"Eroare la salvarea meniului: {errorDetails}";
                    MessageBox.Show(ErrorMessage, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la pregătirea datelor: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelAdd()
        {
            _mainViewModel.NavigateToMenus();
        }
    }

    public class DishItemViewModel : ViewModelBase
    {
        private Dish _dish;
        private bool _isVisible = true;
        private int _quantity = 1;

        public Dish Dish
        {
            get => _dish;
            set => SetProperty(ref _dish, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }
    }
}