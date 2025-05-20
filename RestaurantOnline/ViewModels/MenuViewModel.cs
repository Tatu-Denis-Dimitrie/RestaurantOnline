using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using RestaurantOnline.Models;
using RestaurantOnline.Services;
using Microsoft.Extensions.DependencyInjection;

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
            StergeMeniuCommand = new RelayCommand(m => StergeMeniu(m as Menu), m => _isAngajat);
            AdaugaLaComandaCommand = new RelayCommand(m => AdaugaLaComanda(m as Menu));
            
            LoadCategoriiAndMeniuri();
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
            set
            {
                if (SetProperty(ref _searchTerm, value))
                {
                    LoadMeniuri();
                }
            }
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
        public ICommand StergeMeniuCommand { get; }
        public ICommand AdaugaLaComandaCommand { get; }
        
        private async void LoadCategoriiAndMeniuri()
        {
            try
            {
                IsLoading = true;
                var categorii = await _categorieService.GetAllAsync();
                await _dispatcher.InvokeAsync(() =>
                {
                    Categorii.Clear();
                    foreach (var categorie in categorii)
                    {
                        Categorii.Add(categorie);
                    }
                });

                var meniuri = await _menuService.GetAllAsync();
                await _dispatcher.InvokeAsync(() =>
                {
                    Meniuri.Clear();
                    foreach (var meniu in meniuri)
                    {
                        Meniuri.Add(meniu);
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la încărcarea datelor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async void LoadMeniuri()
        {
            if (IsLoading) return;
            
            lock (_lockObject)
            {
                if (IsLoading) return;
                IsLoading = true;
            }
            
            try
            {
                var meniuri = await _menuService.GetAllAsync();
                
                await _dispatcher.InvokeAsync(() =>
                {
                    Meniuri.Clear();
                    
                    var meniuriFiltrate = meniuri.AsQueryable();
                    
                    if (CategorieSelectata != null)
                    {
                        meniuriFiltrate = meniuriFiltrate.Where(m => m.CategoryId == CategorieSelectata.CategoryId);
                    }
                    
                    if (!string.IsNullOrWhiteSpace(SearchTerm))
                    {
                        var searchTermLower = SearchTerm.ToLower();
                        meniuriFiltrate = meniuriFiltrate.Where(m => 
                            m.Name.ToLower().Contains(searchTermLower) ||
                            m.Category.Name.ToLower().Contains(searchTermLower));
                    }
                    
                    foreach (var meniu in meniuriFiltrate)
                    {
                        Meniuri.Add(meniu);
                    }
                });
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
        
        private async void StergeMeniu(Menu? meniu)
        {
            if (meniu == null) return;
            
            var result = MessageBox.Show(
                $"Sigur doriți să ștergeți meniul '{meniu.Name}'?",
                "Confirmare ștergere",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = await _menuService.DeleteAsync(meniu.MenuId);
                    
                    if (success)
                    {
                    Meniuri.Remove(meniu);
                        MessageBox.Show("Meniul a fost șters cu succes.", "Succes", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Meniul nu poate fi șters deoarece există comenzi care îl conțin.", "Operație nepermisă", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la ștergerea meniului: {ex.Message}", "Eroare", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void AdaugaLaComanda(Menu? meniu)
        {
            if (meniu == null) return;
            
            try
            {
                var orderService = ((App)Application.Current).ServiceProvider.GetService(typeof(OrderS)) as OrderS;
                var dishService = ((App)Application.Current).ServiceProvider.GetService(typeof(DishS)) as DishS;
                var mainViewModel = ((App)Application.Current).ServiceProvider.GetService(typeof(MainViewModel)) as MainViewModel;
                
                if (orderService != null && dishService != null && mainViewModel != null)
                {
                    CartViewModel cartViewModel;
                    
                    if (App.Current.Properties.Contains("CartItems") && 
                        App.Current.Properties["CartItems"] is ObservableCollection<CartItem> savedCart)
                    {
                        cartViewModel = new CartViewModel(orderService, dishService, mainViewModel);
                        cartViewModel.AddMenuToCart(meniu, 1, true);
                        
                        App.Current.Properties["CartItems"] = cartViewModel.CartItems;
                    }
                    else
                    {
                        cartViewModel = new CartViewModel(orderService, dishService, mainViewModel);
                        cartViewModel.AddMenuToCart(meniu, 1, true);
                        
                        App.Current.Properties["CartItems"] = cartViewModel.CartItems;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la adăugarea în coș: {ex.Message}", "Eroare", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 