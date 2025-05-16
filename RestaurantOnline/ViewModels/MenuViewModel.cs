using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using RestaurantOnline.Models;
using RestaurantOnline.Services;

namespace RestaurantOnline.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        private readonly IRestaurantS<Menu> _menuService;
        private readonly CategoryS _categorieService;
        private readonly Dispatcher _dispatcher;
        private readonly object _lockObject = new object();
        private bool _isLoading = false;
        private bool _isAngajat;
        
        private ObservableCollection<Menu> _meniuri;
        private ObservableCollection<Category> _categorii;
        private Category? _categorieSelectata;
        private string _searchTerm = string.Empty;
        private string _errorMessage = string.Empty;
        
        public MenuViewModel(IRestaurantS<Menu> menuService, CategoryS categorieService, bool isAngajat = false)
        {
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _categorieService = categorieService ?? throw new ArgumentNullException(nameof(categorieService));
            _dispatcher = Application.Current.Dispatcher;
            _isAngajat = isAngajat;
            
            _meniuri = new ObservableCollection<Menu>();
            _categorii = new ObservableCollection<Category>();
            
            SearchCommand = new RelayCommand(_ => LoadMeniuri());
            DetaliiCommand = new RelayCommand(m => ShowDetalii(m as Menu));
            AdaugaLaComandaCommand = new RelayCommand(m => AdaugaLaComanda(m as Menu));
            StergeMeniuCommand = new RelayCommand(m => StergeMeniu(m as Menu), m => _isAngajat);
            
            LoadCategorii();
            LoadMeniuri();
        }
        
        public ObservableCollection<Menu> Meniuri
        {
            get => _meniuri;
            set => SetProperty(ref _meniuri, value);
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
                    LoadMeniuri();
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
        
        public bool IsAngajat
        {
            get => _isAngajat;
            set => SetProperty(ref _isAngajat, value);
        }
        
        public ICommand SearchCommand { get; }
        public ICommand DetaliiCommand { get; }
        public ICommand AdaugaLaComandaCommand { get; }
        public ICommand StergeMeniuCommand { get; }
        
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
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la încărcarea categoriilor: {ex.Message}";
            }
        }
        
        private async void LoadMeniuri()
        {
            if (IsLoading) return;
            
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                ObservableCollection<Menu> rezultat = new ObservableCollection<Menu>();

                if (!string.IsNullOrWhiteSpace(_searchTerm))
                {
                    var allMenus = await _menuService.GetAllAsync();
                    var searchResults = allMenus.Where(m => m.Name.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                    rezultat = new ObservableCollection<Menu>(searchResults);
                }
                else if (_categorieSelectata != null && _categorieSelectata.CategoryId > 0)
                {
                    var allMenus = await _menuService.GetAllAsync();
                    var filteredMenus = allMenus.Where(m => m.CategoryId == _categorieSelectata.CategoryId).ToList();
                    rezultat = new ObservableCollection<Menu>(filteredMenus);
                }
                else
                {
                    rezultat = await _menuService.GetAllAsync();
                }

                Meniuri = rezultat;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la încărcarea meniurilor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private void ShowDetalii(Menu? meniu)
        {
            if (meniu == null) return;
            
            try
            {
                var detaliiDialog = new Views.MenuDetailsView();
                detaliiDialog.SetMenuDetails(meniu);
                detaliiDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea detaliilor: {ex.Message}", "Eroare", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void AdaugaLaComanda(Menu? meniu)
        {
            if (meniu == null) return;
            
            MessageBox.Show($"Meniul '{meniu.Name}' a fost adăugat la comandă", "Informație",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private async void StergeMeniu(Menu? meniu)
        {
            if (meniu == null) return;
            
            var result = MessageBox.Show(
                $"Ești sigur că dorești să ștergi meniul '{meniu.Name}'?", 
                "Confirmare ștergere",
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
                
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _menuService.DeleteAsync(meniu.MenuId);
                    
                    if (success)
                    {
                        MessageBox.Show("Meniul a fost șters cu succes.", "Succes", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                            
                        // Reîncărcăm lista de meniuri
                        LoadMeniuri();
                    }
                    else
                    {
                        MessageBox.Show("Nu s-a putut șterge meniul.", "Eroare", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la ștergerea meniului: {ex.Message}", "Eroare", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
} 