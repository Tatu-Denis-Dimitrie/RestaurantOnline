using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using RestaurantOnline.Models;
using RestaurantOnline.Services;
using RestaurantOnline.Views;

namespace RestaurantOnline.ViewModels
{
    public class DishViewModel : ViewModelBase
    {
        private readonly DishS _preparatService;
        private readonly CategoryS _categorieService;
        private readonly Dispatcher _dispatcher;
        private readonly object _lockObject = new object();
        private bool _isLoading = false;
        
        private ObservableCollection<Dish> _preparate;
        private ObservableCollection<Category> _categorii;
        private Category? _categorieSelectata;
        private string _searchTerm = string.Empty;
        private string _errorMessage = string.Empty;
        
        public DishViewModel(DishS preparatService, CategoryS categorieService)
        {
            _preparatService = preparatService ?? throw new ArgumentNullException(nameof(preparatService));
            _categorieService = categorieService ?? throw new ArgumentNullException(nameof(categorieService));
            _dispatcher = Application.Current.Dispatcher;
            
            _preparate = new ObservableCollection<Dish>();
            _categorii = new ObservableCollection<Category>();
            
            SearchCommand = new RelayCommand(_ => LoadPreparate());
            DetaliiCommand = new RelayCommand(p => ShowDetalii(p as Dish));
            AdaugaLaComandaCommand = new RelayCommand(p => AdaugaLaComanda(p as Dish));
            
            LoadCategorii();
            LoadPreparate();
        }
        
        public ObservableCollection<Dish> Preparate
        {
            get => _preparate;
            set => SetProperty(ref _preparate, value);
        }
        
        public ObservableCollection<Category> Categorii
        {
            get => _categorii;
            set => SetProperty(ref _categorii, value);
        }
        
        public Category? CategorieSelectata
        {
            get => _categorieSelectata;
            set
            {
                if (SetProperty(ref _categorieSelectata, value))
                {
                    LoadPreparate();
                }
            }
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
        
        public ICommand SearchCommand { get; }
        public ICommand DetaliiCommand { get; }
        public ICommand AdaugaLaComandaCommand { get; }
        
        private async void LoadCategorii()
        {
            try
            {
                var categorii = await _categorieService.GetAllAsync();
                
                var toateCategoriile = new Category { Name = "Toate categoriile" };
                Categorii.Clear();
                Categorii.Add(toateCategoriile);
                
                foreach (var categorie in categorii)
                {
                    Categorii.Add(categorie);
                }
                
                CategorieSelectata = toateCategoriile;
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Eroare la incarcarea categoriilor: {ex.Message}";
            }
        }
        
        private async void LoadPreparate()
        {
            if (IsLoading) return;
            
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                ObservableCollection<Dish> rezultat;

                if (!string.IsNullOrWhiteSpace(_searchTerm))
                {
                    var searchResults = await _preparatService.SearchPreparat(_searchTerm);
                    rezultat = new ObservableCollection<Dish>(searchResults);
                }
                else if (_categorieSelectata != null && _categorieSelectata.CategoryId > 0)
                {
                    rezultat = await _preparatService.GetByCategorie(_categorieSelectata.CategoryId);
                }
                else
                {
                    rezultat = await _preparatService.GetAllAsync();
                }

                Preparate = rezultat;
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Eroare la incarcarea preparatelor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async void ShowDetalii(Dish? preparat)
        {
            if (preparat == null) return;
            
            try
            {
                var preparatDetaliat = await _preparatService.GetDetaliiPreparat(preparat.DishId);
                
                if (preparatDetaliat != null)
                {
                    var detaliiDialog = new DishDetailsView();
                    detaliiDialog.SetPreparatDetails(preparatDetaliat);
                    detaliiDialog.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Nu s-au putut incarca detaliile preparatului.", "Eroare", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la incarcarea detaliilor: {ex.Message}", "Eroare", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void AdaugaLaComanda(Dish? preparat)
        {
            if (preparat == null) return;
            
        }
    }
} 