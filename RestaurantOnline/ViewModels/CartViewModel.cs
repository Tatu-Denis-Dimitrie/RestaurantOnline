using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using RestaurantOnline.Models;
using RestaurantOnline.Services;
using System.Collections.Generic;

namespace RestaurantOnline.ViewModels
{
    public class CartViewModel : ViewModelBase
    {
        private readonly OrderS _orderService;
        private readonly DishS _dishService;
        private readonly MainViewModel _mainViewModel;
        private ObservableCollection<CartItem> _cartItems;
        private decimal _totalAmount;
        private string _errorMessage;
        
        public CartViewModel(OrderS orderService, DishS dishService, MainViewModel mainViewModel)
        {
            _orderService = orderService;
            _dishService = dishService;
            _mainViewModel = mainViewModel;
            _cartItems = new ObservableCollection<CartItem>();
            
            RemoveFromCartCommand = new RelayCommand(RemoveFromCart);
            PlaceOrderCommand = new RelayCommand(_ => PlaceOrder(), _ => CanPlaceOrder());
            ContinueShoppingCommand = new RelayCommand(_ => _mainViewModel.NavigateToDishes());
            
            LoadCartFromSession();
        }
        
        public ObservableCollection<CartItem> CartItems
        {
            get => _cartItems;
            set
            {
                SetProperty(ref _cartItems, value);
                CalculateTotalAmount();
            }
        }
        
        public decimal TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value);
        }
        
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        
        public ICommand RemoveFromCartCommand { get; }
        public ICommand PlaceOrderCommand { get; }
        public ICommand ContinueShoppingCommand { get; }
        
        public void AddToCart(Dish dish, int quantity = 1, bool showMessage = true)
        {
            if (dish == null) return;
            
            var existingItem = CartItems.FirstOrDefault(item => item.Dish != null && item.Dish.DishId == dish.DishId && item.IsMenuDish == false);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                CartItems.Add(new CartItem { Dish = dish, Quantity = quantity, IsMenuDish = false });
            }
            
            SaveCartToSession();
            CalculateTotalAmount();
            
            if (showMessage)
            {
                MessageBox.Show($"Produsul '{dish.Name}' a fost adăugat în coș.", "Coș cumpărături", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        public async void AddMenuToCart(Menu menu, int quantity = 1, bool showMessage = true)
        {
            if (menu == null) return;
            
            try
            {
                // Mai întâi încărcăm meniul complet cu toate relațiile sale
                var menuService = ((App)Application.Current).ServiceProvider.GetService(typeof(IRestaurantS<Menu>)) as IRestaurantS<Menu>;
                if (menuService != null)
                {
                    var completeMenu = await menuService.GetByIdAsync(menu.MenuId);
                    if (completeMenu != null)
                    {
                        // Găsim dacă meniul există deja în coș
                        var existingItem = CartItems.FirstOrDefault(item => 
                            item.IsMenuDish == true && 
                            item.Menu != null && 
                            item.Menu.MenuId == completeMenu.MenuId);
                            
                        if (existingItem != null)
                        {
                            // Dacă meniul există deja, incrementăm cantitatea
                            existingItem.Quantity += quantity;
                            
                            // Afișăm mesaj cu cantitatea actualizată
                            if (showMessage)
                            {
                                MessageBox.Show($"Cantitatea pentru meniul '{completeMenu.Name}' a fost actualizată la {existingItem.Quantity}.", 
                                    "Coș cumpărături", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        else
                        {
                            // Dacă meniul nu există, îl adăugăm
                            CartItems.Add(new CartItem { Menu = completeMenu, Quantity = quantity, IsMenuDish = true });
                            
                            // Afișăm mesaj de confirmare pentru adăugare
                            if (showMessage)
                            {
                                MessageBox.Show($"Meniul '{completeMenu.Name}' a fost adăugat în coș.", 
                                    "Coș cumpărături", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        
                        SaveCartToSession();
                        CalculateTotalAmount();
                    }
                }
            }
            catch (Exception ex)
            {
                if (showMessage)
                {
                    MessageBox.Show($"Eroare la adăugarea în coș: {ex.Message}", "Eroare", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void RemoveFromCart(object parameter)
        {
            if (parameter is CartItem item)
            {
                CartItems.Remove(item);
                SaveCartToSession();
                CalculateTotalAmount();
            }
        }
        
        private void CalculateTotalAmount()
        {
            decimal productTotal = CartItems.Sum(item => 
                item.IsMenuDish 
                    ? (item.Menu?.TotalPrice ?? 0) * item.Quantity 
                    : (item.Dish?.Price ?? 0) * item.Quantity);
            decimal deliveryFee = CartItems.Count > 0 ? 10.00m : 0;
            TotalAmount = productTotal + deliveryFee;
        }
        
        private bool CanPlaceOrder()
        {
            return _mainViewModel.IsUserLoggedIn && CartItems.Count > 0;
        }
        
        private async void PlaceOrder()
        {
            if (!CanPlaceOrder())
            {
                if (!_mainViewModel.IsUserLoggedIn)
                {
                    MessageBox.Show("Trebuie să fiți autentificat pentru a plasa o comandă.", 
                        "Autentificare necesară", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (CartItems.Count == 0)
                {
                    MessageBox.Show("Coșul dumneavoastră este gol.", 
                        "Coș gol", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            
            try
            {
                // Verificare stoc pentru preparate individuale
                foreach (var item in CartItems.Where(i => !i.IsMenuDish && i.Dish != null))
                {
                    var dish = await _dishService.GetByIdAsync(item.Dish.DishId);
                    
                    if (dish == null)
                    {
                        MessageBox.Show($"Preparatul '{item.Dish.Name}' nu mai este disponibil.", 
                            "Produs indisponibil", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    int cantitateNecesara = item.Quantity * dish.PortionSizeGrams;
                    
                    if (dish.TotalQuantityGrams < cantitateNecesara)
                    {
                        int portiiDisponibile = dish.TotalQuantityGrams / dish.PortionSizeGrams;
                        MessageBox.Show($"Ne pare rău, nu avem suficientă cantitate pentru '{dish.Name}'.\nCantitate disponibilă: {portiiDisponibile} porții.", 
                            "Stoc insuficient", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                
                // Verificare stoc pentru preparate din meniuri
                foreach (var item in CartItems.Where(i => i.IsMenuDish && i.Menu != null))
                {
                    foreach (var menuDish in item.Menu.MenuDishes)
                    {
                        var dish = await _dishService.GetByIdAsync(menuDish.Dish.DishId);
                        
                        if (dish == null)
                        {
                            MessageBox.Show($"Preparatul '{menuDish.Dish.Name}' din meniul '{item.Menu.Name}' nu mai este disponibil.", 
                                "Produs indisponibil", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        
                        int cantitateNecesara = item.Quantity * dish.PortionSizeGrams;
                        
                        if (dish.TotalQuantityGrams < cantitateNecesara)
                        {
                            int portiiDisponibile = dish.TotalQuantityGrams / dish.PortionSizeGrams;
                            MessageBox.Show($"Ne pare rău, nu avem suficientă cantitate pentru '{dish.Name}' din meniul '{item.Menu.Name}'.\nCantitate disponibilă: {portiiDisponibile} porții.", 
                                "Stoc insuficient", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                }
                
                decimal productTotal = CartItems.Sum(item => 
                    item.IsMenuDish 
                        ? (item.Menu?.TotalPrice ?? 0) * item.Quantity 
                        : (item.Dish?.Price ?? 0) * item.Quantity);
                decimal deliveryFee = 10.00m;
                decimal finalAmount = productTotal + deliveryFee;
                
                var order = new Order
                {
                    UserId = _mainViewModel.CurrentUser.UserId,
                    OrderDate = DateTime.Now,
                    Status = "inregistrata",
                    DeliveryFee = deliveryFee,
                    FinalAmount = finalAmount,
                    // Nu inițializăm OrderDishes aici pentru a evita probleme de tracking
                };

                try 
                {
                    // Salvăm comanda pentru a obține un OrderId valid
                    var savedOrder = await _orderService.AddAsync(order);
                    
                    if (savedOrder == null || savedOrder.OrderId <= 0)
                    {
                        throw new Exception("Nu s-a putut salva comanda principală.");
                    }
                    
                    int orderId = savedOrder.OrderId;
                    
                    // Adăugăm preparatele individuale
                    foreach (var item in CartItems.Where(i => !i.IsMenuDish && i.Dish != null))
                    {
                        var orderDish = new OrderDish
                        {
                            OrderId = orderId,
                            DishId = item.Dish.DishId,
                            Quantity = item.Quantity,
                            MenuId = null
                        };
                        
                        await _orderService.AddOrderDishAsync(orderDish);
                        
                        // Actualizăm stocul
                        var dish = await _dishService.GetByIdAsync(item.Dish.DishId);
                        if (dish != null)
                        {
                            int cantitateConsumata = item.Quantity * dish.PortionSizeGrams;
                            dish.TotalQuantityGrams -= cantitateConsumata;
                            await _dishService.UpdateAsync(dish);
                        }
                    }
                    
                    // Adăugăm preparatele din meniuri
                    foreach (var item in CartItems.Where(i => i.IsMenuDish && i.Menu != null))
                    {
                        foreach (var menuDish in item.Menu.MenuDishes)
                        {
                            var orderDish = new OrderDish
                            {
                                OrderId = orderId,
                                DishId = menuDish.Dish.DishId,
                                Quantity = item.Quantity,
                                MenuId = item.Menu.MenuId
                            };
                            
                            await _orderService.AddOrderDishAsync(orderDish);
                            
                            // Actualizăm stocul
                            var dish = await _dishService.GetByIdAsync(menuDish.Dish.DishId);
                            if (dish != null)
                            {
                                int cantitateConsumata = item.Quantity * dish.PortionSizeGrams;
                                dish.TotalQuantityGrams -= cantitateConsumata;
                                await _dishService.UpdateAsync(dish);
                            }
                        }
                    }
                    
                    // Afișăm mesaj de succes
                    MessageBox.Show($"Comanda dumneavoastră a fost înregistrată cu succes.\nNumăr comandă: {orderId}\nTotal: {finalAmount:F2} lei (inclusiv taxa de transport: {deliveryFee:F2} lei)", 
                        "Comandă plasată", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Curățăm coșul
                    CartItems.Clear();
                    SaveCartToSession();
                    CalculateTotalAmount();
                    
                    // Navigăm înapoi la preparate
                    _mainViewModel.NavigateToDishes();
                }
                catch (Exception ex)
                {
                    // Propagăm excepția pentru a fi gestionată în blocul catch exterior
                    throw new Exception("Eroare la salvarea comenzii sau a produselor", ex);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la plasarea comenzii: {ex.Message}";
                
                // Obținem mai multe detalii despre excepție
                string detaliiEroare = ex.Message;
                
                if (ex.InnerException != null)
                {
                    detaliiEroare += $"\n\nDetalii suplimentare: {ex.InnerException.Message}";
                }
                
                // Afișăm detalii despre stiva de apel pentru debugging
                detaliiEroare += $"\n\nStack trace: {ex.StackTrace}";
                
                MessageBox.Show($"A apărut o eroare la plasarea comenzii:\n{detaliiEroare}", 
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Scriem și în consolă pentru debugging
                System.Diagnostics.Debug.WriteLine($"Eroare la plasarea comenzii: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
        
        private void SaveCartToSession()
        {
            App.Current.Properties["CartItems"] = CartItems;
        }
        
        private void LoadCartFromSession()
        {
            try
            {
                if (App.Current.Properties.Contains("CartItems") && 
                    App.Current.Properties["CartItems"] is ObservableCollection<CartItem> savedCart)
                {
                    var validItems = savedCart.Where(item => item?.Dish != null || item?.Menu != null).ToList();
                    CartItems = new ObservableCollection<CartItem>(validItems);
                }
            }
            catch (Exception ex)
            {
                CartItems = new ObservableCollection<CartItem>();
                ErrorMessage = $"Eroare la încărcarea coșului: {ex.Message}";
            }
        }
        
        public async void RefreshCart()
        {
            try
            {
                // Încărcăm coșul din sesiune
                LoadCartFromSession();
                
                // Pentru fiecare element din coș, reîncărcăm datele complete
                var updatedItems = new ObservableCollection<CartItem>();
                
                foreach (var item in CartItems)
                {
                    if (item.IsMenuDish && item.Menu != null)
                    {
                        // Reîncărcăm meniul cu toate detaliile sale
                        var menuService = ((App)Application.Current).ServiceProvider.GetService(typeof(IRestaurantS<Menu>)) as IRestaurantS<Menu>;
                        if (menuService != null)
                        {
                            var completeMenu = await menuService.GetByIdAsync(item.Menu.MenuId);
                            if (completeMenu != null)
                            {
                                updatedItems.Add(new CartItem
                                {
                                    Menu = completeMenu,
                                    Quantity = item.Quantity,
                                    IsMenuDish = true
                                });
                            }
                        }
                    }
                    else if (!item.IsMenuDish && item.Dish != null)
                    {
                        // Reîncărcăm preparatul cu toate detaliile sale
                        var completeDish = await _dishService.GetByIdAsync(item.Dish.DishId);
                        if (completeDish != null)
                        {
                            updatedItems.Add(new CartItem
                            {
                                Dish = completeDish,
                                Quantity = item.Quantity,
                                IsMenuDish = false
                            });
                        }
                    }
                }
                
                // Actualizăm coșul curent și cel din sesiune
                CartItems = updatedItems;
                SaveCartToSession();
                CalculateTotalAmount();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la reîmprospătarea coșului: {ex.Message}";
                CartItems = new ObservableCollection<CartItem>();
            }
        }
    }
    
    public class CartItem : ViewModelBase
    {
        private Dish _dish;
        private Menu _menu;
        private int _quantity = 1;
        private bool _isMenuDish;
        
        public Dish Dish
        {
            get => _dish;
            set => SetProperty(ref _dish, value);
        }
        
        public Menu Menu
        {
            get => _menu;
            set => SetProperty(ref _menu, value);
        }
        
        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }
        
        public bool IsMenuDish
        {
            get => _isMenuDish;
            set => SetProperty(ref _isMenuDish, value);
        }
        
        public decimal LineTotal => IsMenuDish 
            ? (Menu?.TotalPrice ?? 0) * Quantity 
            : (Dish?.Price ?? 0) * Quantity;
        
        public string Name => IsMenuDish 
            ? Menu?.Name ?? "Meniu necunoscut" 
            : Dish?.Name ?? "Preparat necunoscut";
        
        public decimal UnitPrice => IsMenuDish 
            ? Menu?.TotalPrice ?? 0 
            : Dish?.Price ?? 0;
    }
} 