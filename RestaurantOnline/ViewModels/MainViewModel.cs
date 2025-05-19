using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RestaurantOnline.Models;
using RestaurantOnline.Services;

namespace RestaurantOnline.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase? _currentViewModel;
        private readonly DishS _dishService;
        private readonly CategoryS _categoryService;
        private readonly UserS _userService;
        private readonly OrderS _orderService;
        private readonly AllergenS _allergenService;
        private readonly MenuService _menuService;
        private User? _currentUser;

        public MainViewModel(
            DishS dishService,
            CategoryS categoryService,
            UserS userService,
            OrderS orderService,
            AllergenS allergenService)
        {
            _dishService = dishService;
            _categoryService = categoryService;
            _userService = userService;
            _orderService = orderService;
            _allergenService = allergenService;
            _menuService = ((App)Application.Current).ServiceProvider.GetRequiredService<MenuService>();

            // Activate navigation commands
            NavigateToDishesCommand = new RelayCommand(_ => NavigateToDishes());
            NavigateToUsersCommand = new RelayCommand(_ => NavigateToUsers());
            NavigateToOrdersCommand = new RelayCommand(_ => NavigateToOrders());
            NavigateToLoginCommand = new RelayCommand(_ => NavigateToLogin());
            NavigateToRegisterCommand = new RelayCommand(_ => NavigateToRegister());
            NavigateToMyAccountCommand = new RelayCommand(_ => NavigateToMyAccount());
            NavigateToAddDishCommand = new RelayCommand(_ => NavigateToAddDish());
            NavigateToAdminCommand = new RelayCommand(_ => NavigateToAdmin());
            NavigateToMenusCommand = new RelayCommand(_ => NavigateToMenus());
            NavigateToCartCommand = new RelayCommand(_ => NavigateToCart());
            LogoutCommand = new RelayCommand(_ => Logout());
            
            // Load dishes page by default
            NavigateToDishes();
        }

        public ViewModelBase? CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public User? CurrentUser
        {
            get => _currentUser;
            set 
            { 
                SetProperty(ref _currentUser, value);
                OnPropertyChanged(nameof(IsUserLoggedIn));
                OnPropertyChanged(nameof(IsClientLoggedIn));
                OnPropertyChanged(nameof(IsEmployeeLoggedIn));
                OnPropertyChanged(nameof(UserDisplayName));
                OnPropertyChanged(nameof(ShowUsersButton));
                OnPropertyChanged(nameof(ShowOrdersButton));
            }
        }

        // Properties for button visibility management
        public bool IsUserLoggedIn => CurrentUser != null;
        
        public bool IsClientLoggedIn => CurrentUser != null && CurrentUser.Role == "Client";
        
        public bool IsEmployeeLoggedIn => CurrentUser != null && CurrentUser.Role == "Angajat";
        
        // Properties for controlling specific button visibility
        public bool ShowUsersButton => IsEmployeeLoggedIn;
        
        public bool ShowOrdersButton => IsEmployeeLoggedIn;
        
        public string UserDisplayName => CurrentUser != null 
            ? $"Bun venit, {CurrentUser.NumeComplet}!" 
            : string.Empty;

        // Navigation commands
        public ICommand NavigateToDishesCommand { get; }
        public ICommand NavigateToUsersCommand { get; }
        public ICommand NavigateToOrdersCommand { get; }
        public ICommand NavigateToLoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand NavigateToMyAccountCommand { get; }
        public ICommand NavigateToAddDishCommand { get; }
        public ICommand NavigateToAdminCommand { get; }
        public ICommand NavigateToMenusCommand { get; }
        public ICommand NavigateToCartCommand { get; }
        public ICommand LogoutCommand { get; }

        public void NavigateToHome()
        {
            NavigateToDishes();
        }
        
        public void NavigateToDishes()
        {
            CurrentViewModel = new DishViewModel(_dishService, _categoryService, IsEmployeeLoggedIn);
        }
        
        private void NavigateToMenus()
        {
            CurrentViewModel = new MenuViewModel(_menuService, _categoryService, IsEmployeeLoggedIn);
        }

        private void NavigateToUsers()
        {
            CurrentViewModel = new UtilizatoriViewModel(_userService);
        }

        private void NavigateToOrders()
        {
            CurrentViewModel = new ComenziViewModel(_orderService, _dishService, _userService);
        }

        private void NavigateToLogin()
        {
            CurrentViewModel = new AuthVM(_userService, this);
        }

        private void NavigateToRegister()
        {
            CurrentViewModel = new AuthVM(_userService, this, isRegisterMode: true);
        }
        
        private void NavigateToMyAccount()
        {
            if (CurrentUser == null) return;
            
            CurrentViewModel = new MyAccountViewModel(_orderService, _dishService, CurrentUser);
        }
        
        private void NavigateToAddDish()
        {
            CurrentViewModel = new AddDishViewModel(_dishService, _categoryService, _allergenService, this);
        }
        
        private void NavigateToAdmin()
        {
            CurrentViewModel = new AdminViewModel(_dishService, _categoryService, _allergenService, _userService, _orderService, this);
        }
        
        private async void NavigateToCart()
        {
            var cartViewModel = new CartViewModel(_orderService, _dishService, this);
            
            try
            {
                // Forțăm reîncărcarea conținutului coșului
                cartViewModel.RefreshCart();
                
                // Setăm view-ul curent
                CurrentViewModel = cartViewModel;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea coșului: {ex.Message}", "Eroare", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void Logout()
        {
            CurrentUser = null;
            NavigateToDishes();
        }
    }
} 