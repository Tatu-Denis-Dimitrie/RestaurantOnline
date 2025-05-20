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
    public class EditDishViewModel : ViewModelBase
    {
        private readonly DishS _dishService;
        private readonly CategoryS _categoryService;
        private readonly AllergenS _allergenService;
        private readonly MainViewModel _mainViewModel;

        private int _dishId;
        private string _name = string.Empty;
        private decimal _price;
        private int _portionSizeGrams;
        private int _totalQuantityGrams;
        private Category? _selectedCategory;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<AllergenItemViewModel> _allAllergens;
        private ObservableCollection<Dish> _allDishes;
        private Dish? _selectedDish;
        private string _photoName = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;
        private SemaphoreSlim _loadingSemaphore = new SemaphoreSlim(1, 1);

        public EditDishViewModel(DishS dishService, CategoryS categoryService, AllergenS allergenService, MainViewModel mainViewModel)
        {
            _dishService = dishService;
            _categoryService = categoryService;
            _allergenService = allergenService;
            _mainViewModel = mainViewModel;

            _categories = new ObservableCollection<Category>();
            _allAllergens = new ObservableCollection<AllergenItemViewModel>();
            _allDishes = new ObservableCollection<Dish>();

            SaveCommand = new RelayCommand(_ => SaveDish(), _ => CanSaveDish());
            CancelCommand = new RelayCommand(_ => CancelEdit());
            LoadDishDetailsCommand = new RelayCommand(_ => LoadDishDetails(), _ => SelectedDish != null);

            // Încărcăm datele când se creează pagina
            _ = InitializeDataAsync();
        }

        // Proprietăți
        public int DishId
        {
            get => _dishId;
            set => SetProperty(ref _dishId, value);
        }

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

        public ObservableCollection<Dish> AllDishes
        {
            get => _allDishes;
            set => SetProperty(ref _allDishes, value);
        }

        public Dish? SelectedDish
        {
            get => _selectedDish;
            set
            {
                if (SetProperty(ref _selectedDish, value) && value != null)
                {
                    LoadDishDetails();
                }
            }
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
        public ICommand LoadDishDetailsCommand { get; }

        private async Task InitializeDataAsync()
        {
            await _loadingSemaphore.WaitAsync();
            try
            {
                IsLoading = true;
                await LoadCategoriesAsync();
                await LoadAllergensAsync();
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
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la încărcarea categoriilor: {ex.Message}";
            }
        }

        private async Task LoadAllergensAsync()
        {
            try
            {
                // Încărcăm alergenii din baza de date
                var allergens = await _allergenService.GetAllAsync();
                AllAllergens.Clear();

                foreach (var allergen in allergens)
                {
                    AllAllergens.Add(new AllergenItemViewModel { Allergen = allergen });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la încărcarea alergenilor: {ex.Message}";
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
                    AllDishes.Add(dish);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la încărcarea preparatelor: {ex.Message}";
            }
        }

        private async void LoadDishDetails()
        {
            if (SelectedDish == null) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Încărcăm detaliile complete ale preparatului
                var dishDetails = await _dishService.GetByIdAsync(SelectedDish.DishId);
                if (dishDetails == null)
                {
                    ErrorMessage = "Nu s-au putut încărca detaliile preparatului.";
                    return;
                }

                // Populăm câmpurile formularului cu datele preparatului
                DishId = dishDetails.DishId;
                Name = dishDetails.Name;
                Price = dishDetails.Price;
                PortionSizeGrams = dishDetails.PortionSizeGrams;
                TotalQuantityGrams = dishDetails.TotalQuantityGrams;

                // Setăm categoria selectată
                SelectedCategory = Categories.FirstOrDefault(c => c.CategoryId == dishDetails.CategoryId);

                // Marcăm alergenii selectați
                foreach (var allergenVM in AllAllergens)
                {
                    allergenVM.IsSelected = dishDetails.DishAllergens.Any(da => da.AllergenId == allergenVM.Allergen.AllergenId);
                }

                // Setăm numele imaginii (dacă există)
                if (dishDetails.Photos.Count > 0)
                {
                    var photoUrl = dishDetails.Photos[0].Url;
                    if (!string.IsNullOrEmpty(photoUrl) && photoUrl.StartsWith("Imagini/"))
                    {
                        PhotoName = photoUrl.Substring("Imagini/".Length);
                    }
                    else
                    {
                        PhotoName = photoUrl;
                    }
                }
                else
                {
                    PhotoName = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la încărcarea detaliilor preparatului: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanSaveDish()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   Price > 0 &&
                   PortionSizeGrams > 0 &&
                   TotalQuantityGrams >= 0 &&
                   SelectedCategory != null &&
                   DishId > 0 &&
                   !IsLoading;
        }

        private async void SaveDish()
        {
            if (IsLoading || DishId <= 0) return;

            if (string.IsNullOrWhiteSpace(Name) || Price <= 0 || PortionSizeGrams <= 0 || SelectedCategory == null)
            {
                ErrorMessage = "Toate câmpurile sunt obligatorii. Prețul și gramajul porției trebuie să fie mai mari decât 0.";
                return;
            }

            try
            {
                IsLoading = true;

                // Obținem preparatul existent
                var existingDish = await _dishService.GetByIdAsync(DishId);
                if (existingDish == null)
                {
                    ErrorMessage = "Preparatul nu a fost găsit în baza de date.";
                    return;
                }

                // Actualizăm proprietățile preparatului
                existingDish.Name = Name;
                existingDish.Price = Price;
                existingDish.PortionSizeGrams = PortionSizeGrams;
                existingDish.TotalQuantityGrams = TotalQuantityGrams;
                existingDish.CategoryId = SelectedCategory.CategoryId;
                existingDish.Category = SelectedCategory;

                // Actualizăm alergenii
                var selectedAllergens = AllAllergens.Where(a => a.IsSelected).ToList();

                // Curățăm colecția existentă
                existingDish.DishAllergens.Clear();

                // Adăugăm alergenii selectați
                foreach (var allergenVM in selectedAllergens)
                {
                    existingDish.DishAllergens.Add(new DishAllergen
                    {
                        DishId = DishId,
                        AllergenId = allergenVM.Allergen.AllergenId,
                        Allergen = allergenVM.Allergen
                    });
                }

                // Actualizăm imaginea dacă s-a schimbat
                if (!string.IsNullOrWhiteSpace(PhotoName))
                {
                    var photoUrl = $"Imagini/{PhotoName}";
                    if (existingDish.Photos.Count > 0)
                    {
                        // Actualizăm imaginea existentă
                        existingDish.Photos[0].Url = photoUrl;
                    }
                    else
                    {
                        // Adăugăm o imagine nouă
                        existingDish.Photos.Add(new DishImage
                        {
                            DishId = DishId,
                            Url = photoUrl
                        });
                    }
                }

                // Salvăm modificările în baza de date
                try
                {
                    await _dishService.UpdateWithAllergensAsync(existingDish);

                    // Afișăm mesaj de succes
                    MessageBox.Show("Preparatul a fost actualizat cu succes!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Reîncărcăm lista de preparate
                    await LoadDishesAsync();

                    // Resetăm formularul
                    ClearForm();
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

                    ErrorMessage = $"Eroare la salvarea preparatului: {errorDetails}";
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

        private void ClearForm()
        {
            DishId = 0;
            Name = string.Empty;
            Price = 0;
            PortionSizeGrams = 0;
            TotalQuantityGrams = 0;
            SelectedCategory = null;
            PhotoName = string.Empty;
            SelectedDish = null;

            // Resetăm alergenii
            foreach (var allergenVM in AllAllergens)
            {
                allergenVM.IsSelected = false;
            }
        }

        private void CancelEdit()
        {
            ClearForm();
            _mainViewModel.NavigateToHome();
        }
    }
}