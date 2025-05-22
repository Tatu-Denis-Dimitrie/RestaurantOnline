using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using RestaurantOnline.Models;
using RestaurantOnline.Services;

namespace RestaurantOnline.ViewModels
{
    public class CategoryViewModel : ViewModelBase
    {
        private readonly CategoryS _categoryService;
        private ObservableCollection<Category> _categories;
        private Category _selectedCategory;
        private string _newCategoryName;
        private string _errorMessage;
        private bool _isLoading;

        public CategoryViewModel(CategoryS categoryService)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _categories = new ObservableCollection<Category>();
            _newCategoryName = string.Empty;
            _errorMessage = string.Empty;

            SaveCommand = new RelayCommand(_ => SaveCategory(), _ => CanSaveCategory());
            DeleteCommand = new RelayCommand(_ => DeleteCategory(), _ => CanDeleteCategory());
            RefreshCommand = new RelayCommand(_ => LoadCategories());

            LoadCategories();
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value) && value != null)
                {
                    NewCategoryName = value.Name;
                }
            }
        }

        public string NewCategoryName
        {
            get => _newCategoryName;
            set => SetProperty(ref _newCategoryName, value);
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
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        private async void LoadCategories()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var categories = await _categoryService.GetAllAsync();
                Categories = categories;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la încărcarea categoriilor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanSaveCategory()
        {
            return !string.IsNullOrWhiteSpace(NewCategoryName);
        }

        private async void SaveCategory()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                if (SelectedCategory == null)
                {
                    var newCategory = new Category { Name = NewCategoryName };
                    await _categoryService.AddAsync(newCategory);
                    MessageBox.Show("Categoria a fost adăugată cu succes.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    SelectedCategory.Name = NewCategoryName;
                    await _categoryService.UpdateAsync(SelectedCategory);
                    MessageBox.Show("Categoria a fost actualizată cu succes.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                NewCategoryName = string.Empty;
                SelectedCategory = null;
                LoadCategories();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la salvarea categoriei: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanDeleteCategory()
        {
            return SelectedCategory != null;
        }

        private async void DeleteCategory()
        {
            if (SelectedCategory == null) return;

            var result = MessageBox.Show(
                $"Sigur doriți să ștergeți categoria '{SelectedCategory.Name}'?",
                "Confirmare ștergere",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                await _categoryService.DeleteAsync(SelectedCategory.CategoryId);
                MessageBox.Show("Categoria a fost ștearsă cu succes.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                NewCategoryName = string.Empty;
                SelectedCategory = null;
                LoadCategories();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la ștergerea categoriei: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}