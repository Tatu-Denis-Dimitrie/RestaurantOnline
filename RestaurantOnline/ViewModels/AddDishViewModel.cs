using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using RestaurantOnline.Models;
using RestaurantOnline.Services;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace RestaurantOnline.ViewModels
{
    public class AddDishViewModel : ViewModelBase
    {
        private readonly DishS _dishService;
        private readonly CategoryS _categoryService;
        private readonly AllergenS _allergenService;
        private readonly MainViewModel _mainViewModel;

        private string _name = string.Empty;
        private decimal _price;
        private int _portionSizeGrams;
        private int _totalQuantityGrams;
        private Category? _selectedCategory;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<AllergenItemViewModel> _allAllergens;
        private string _photoName = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;
        private SemaphoreSlim _loadingSemaphore = new SemaphoreSlim(1, 1);

        public AddDishViewModel(DishS dishService, CategoryS categoryService, AllergenS allergenService, MainViewModel mainViewModel)
        {
            _dishService = dishService;
            _categoryService = categoryService;
            _allergenService = allergenService;
            _mainViewModel = mainViewModel;

            _categories = new ObservableCollection<Category>();
            _allAllergens = new ObservableCollection<AllergenItemViewModel>();

            SaveCommand = new RelayCommand(_ => SaveDish());
            CancelCommand = new RelayCommand(_ => CancelAdd());

            // incarcam datele cand se creeaza pagina
            _ = InitializeDataAsync();
        }

        // Proprietati
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public decimal Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        public int PortionSizeGrams
        {
            get => _portionSizeGrams;
            set => SetProperty(ref _portionSizeGrams, value);
        }

        public int TotalQuantityGrams
        {
            get => _totalQuantityGrams;
            set => SetProperty(ref _totalQuantityGrams, value);
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

        public ObservableCollection<AllergenItemViewModel> AllAllergens
        {
            get => _allAllergens;
            set => SetProperty(ref _allAllergens, value);
        }

        public string PhotoName
        {
            get => _photoName;
            set => SetProperty(ref _photoName, value);
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

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private async Task InitializeDataAsync()
        {
            await _loadingSemaphore.WaitAsync();
            try
            {
                IsLoading = true;
                await LoadCategoriesAsync();
                await LoadAllergensAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error initializing data: {ex.Message}";
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
                ErrorMessage = $"Error loading categories: {ex.Message}";
            }
        }

        private async Task LoadAllergensAsync()
        {
            try
            {
                // incarcam alergenii din baza de date
                var allergens = await _allergenService.GetAllAsync();
                AllAllergens.Clear();

                foreach (var allergen in allergens)
                {
                    AllAllergens.Add(new AllergenItemViewModel { Allergen = allergen });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading allergens: {ex.Message}";
            }
        }

        private async void SaveDish()
        {
            if (IsLoading) return;

            if (string.IsNullOrWhiteSpace(Name) || Price <= 0 || PortionSizeGrams <= 0 || TotalQuantityGrams <= 0 || SelectedCategory == null)
            {
                ErrorMessage = "All fields are required. Price, portion size and quantity must be greater than 0.";
                return;
            }

            try
            {
                IsLoading = true;

                var dish = new Dish
                {
                    Name = Name,
                    Price = Price,
                    PortionSizeGrams = PortionSizeGrams,
                    TotalQuantityGrams = TotalQuantityGrams,
                    CategoryId = SelectedCategory.CategoryId
                };

                // Adaugam alergenii selectati
                var selectedAllergens = AllAllergens.Where(a => a.IsSelected).ToList();
                if (selectedAllergens.Any())
                {
                    foreach (var allergenVM in selectedAllergens)
                    {
                        dish.DishAllergens.Add(new DishAllergen
                        {
                            AllergenId = allergenVM.Allergen.AllergenId
                        });
                    }
                }

                // Adaugam imaginea daca s-a introdus un nume
                if (!string.IsNullOrWhiteSpace(PhotoName))
                {
                    dish.Photos.Add(new DishImage
                    {
                        Url = $"Imagini/{PhotoName}"
                    });
                }

                // Salvam preparatul in baza de date cu tratarea erorilor detaliate
                try
                {
                    await _dishService.AddAsync(dish);

                    // Afisam mesaj de succes
                    MessageBox.Show("The dish was added successfully to the database!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Navigheaza inapoi la lista de preparate si reincarca datele
                    _mainViewModel.NavigateToHome();
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

                    ErrorMessage = $"Error saving dish: {errorDetails}";
                    MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error preparing data: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelAdd()
        {
            _mainViewModel.NavigateToHome();
        }
    }

    public class AllergenItemViewModel : ViewModelBase
    {
        private bool _isSelected;
        private Allergen _allergen;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public Allergen Allergen
        {
            get => _allergen;
            set => SetProperty(ref _allergen, value);
        }

        public string Name => Allergen?.Name ?? string.Empty;
    }
}