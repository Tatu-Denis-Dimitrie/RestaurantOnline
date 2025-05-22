using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using RestaurantOnline.Models;
using RestaurantOnline.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Globalization; // Pentru culture info

namespace RestaurantOnline.ViewModels
{
    public class CartViewModel : ViewModelBase
    {
        private readonly OrderS _orderService;
        private readonly DishS _dishService;
        private readonly MainViewModel _mainViewModel;
        private ObservableCollection<CartItem> _cartItems;
        private decimal _totalAmount;
        private decimal _originalAmount;
        private decimal _discountAmount;
        private decimal _orderValueDiscountAmount;
        private decimal _deliveryFee;
        private string _errorMessage;
        private bool _discountApplied;
        private bool _orderValueDiscountApplied;
        private int _minimumOrdersForDiscount = 5;
        private int _discountPercent = 10;
        private decimal _minimumOrderValue = 150;
        private int _orderValueDiscountPercent = 10;
        private decimal _standardDeliveryFee = 15.00m;
        private decimal _freeDeliveryThreshold = 50.00m;
        
        public CartViewModel(OrderS orderService, DishS dishService, MainViewModel mainViewModel)
        {
            _orderService = orderService;
            _dishService = dishService;
            _mainViewModel = mainViewModel;
            _cartItems = new ObservableCollection<CartItem>();
            
            RemoveFromCartCommand = new RelayCommand(RemoveFromCart);
            PlaceOrderCommand = new RelayCommand(_ => PlaceOrder(), _ => CanPlaceOrder());
            ContinueShoppingCommand = new RelayCommand(_ => _mainViewModel.NavigateToDishes());
            
            LoadDiscountSettings();
            LoadDeliveryFeeSettings();
            
            LoadCartFromSession();
        }
        
        private void LoadDiscountSettings()
        {
            try
            {
                var configuration = ((App)Application.Current).Configuration;
                if (configuration != null)
                {
                    var clientDiscountsSection = configuration.GetSection("ClientDiscounts");
                    
                    var loyaltySection = clientDiscountsSection.GetSection("LoyaltyDiscount");
                    
                    string minimumOrdersStr = loyaltySection["MinimumOrders"];
                    string discountPercentStr = loyaltySection["DiscountPercent"];
                    
                    if (!string.IsNullOrEmpty(minimumOrdersStr) && int.TryParse(minimumOrdersStr, out int minimumOrders))
                    {
                        _minimumOrdersForDiscount = minimumOrders;
                    }
                    
                    if (!string.IsNullOrEmpty(discountPercentStr) && int.TryParse(discountPercentStr, out int discountPercent))
                    {
                        _discountPercent = discountPercent;
                    }
                    
                    var orderValueSection = clientDiscountsSection.GetSection("OrderValueDiscount");
                    
                    string minimumOrderValueStr = orderValueSection["MinimumOrderValue"];
                    string orderValueDiscountPercentStr = orderValueSection["DiscountPercent"];
                    
                    if (!string.IsNullOrEmpty(minimumOrderValueStr) && decimal.TryParse(minimumOrderValueStr, out decimal minimumOrderValue))
                    {
                        _minimumOrderValue = minimumOrderValue;
                    }
                    
                    if (!string.IsNullOrEmpty(orderValueDiscountPercentStr) && int.TryParse(orderValueDiscountPercentStr, out int orderValueDiscountPercent))
                    {
                        _orderValueDiscountPercent = orderValueDiscountPercent;
                    }
                    
                }
            }
            catch (Exception ex)
            {}
        }
        
        private void LoadDeliveryFeeSettings()
        {
            try
            {
                var configuration = ((App)Application.Current).Configuration;
                if (configuration != null)
                {
                    var deliveryFeesSection = configuration.GetSection("DeliveryFees");
                    
                    string standardFeeStr = deliveryFeesSection["StandardFee"];
                    string freeDeliveryThresholdStr = deliveryFeesSection["FreeDeliveryThreshold"];
                    
                    
                    if (!string.IsNullOrEmpty(standardFeeStr))
                    {
                        if (decimal.TryParse(standardFeeStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal standardFee))
                        {
                            _standardDeliveryFee = standardFee;
                        }
                        else
                        {
                            string alternateFormat = standardFeeStr.Replace('.', ',');
                            if (decimal.TryParse(alternateFormat, out standardFee))
                            {
                                _standardDeliveryFee = standardFee;
                            }
                            else
                            {
                                _standardDeliveryFee = 15.00m;
                            }
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(freeDeliveryThresholdStr))
                    {
                        if (decimal.TryParse(freeDeliveryThresholdStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal freeDeliveryThreshold))
                        {
                            _freeDeliveryThreshold = freeDeliveryThreshold;
                        }
                        else
                        {
                            _freeDeliveryThreshold = 50.00m;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _standardDeliveryFee = 15.00m;
                _freeDeliveryThreshold = 50.00m;
            }
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
        
        public decimal OriginalAmount
        {
            get => _originalAmount;
            set => SetProperty(ref _originalAmount, value);
        }
        
        public decimal DiscountAmount
        {
            get => _discountAmount;
            set => SetProperty(ref _discountAmount, value);
        }
        
        public decimal OrderValueDiscountAmount
        {
            get => _orderValueDiscountAmount;
            set => SetProperty(ref _orderValueDiscountAmount, value);
        }
        
        public decimal DeliveryFee
        {
            get => _deliveryFee;
            set => SetProperty(ref _deliveryFee, value);
        }
        
        public bool DiscountApplied
        {
            get => _discountApplied;
            set => SetProperty(ref _discountApplied, value);
        }
        
        public bool OrderValueDiscountApplied
        {
            get => _orderValueDiscountApplied;
            set => SetProperty(ref _orderValueDiscountApplied, value);
        }
        
        public string DiscountInfo => DiscountApplied 
            ? $"Reducere de {_discountPercent}% client fidel (min. {_minimumOrdersForDiscount} comenzi în ultimele 30 zile): -{DiscountAmount:F2} lei" 
            : string.Empty;
            
        public string OrderValueDiscountInfo => OrderValueDiscountApplied 
            ? $"Reducere de {_orderValueDiscountPercent}% pentru comenzi peste {_minimumOrderValue:F2} lei: -{OrderValueDiscountAmount:F2} lei" 
            : string.Empty;
            
        public string DeliveryFeeInfo
        {
            get
            {
                if (CartItems.Count == 0)
                    return "Taxă de livrare: 0,00 lei";
                    
                decimal subtotalProducts = CartItems.Sum(item => item.LineTotal);
                if (subtotalProducts >= _freeDeliveryThreshold)
                    return "Livrare gratuită (pentru comenzi peste 50,00 lei)";
                else
                    return string.Format(CultureInfo.CurrentCulture, "Taxă de livrare: {0:0.00} lei", _standardDeliveryFee);
            }
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
                var menuService = ((App)Application.Current).ServiceProvider.GetService(typeof(IRestaurantS<Menu>)) as IRestaurantS<Menu>;
                if (menuService != null)
                {
                    var completeMenu = await menuService.GetByIdAsync(menu.MenuId);
                    if (completeMenu != null)
                    {
                        var existingItem = CartItems.FirstOrDefault(item => 
                            item.IsMenuDish == true && 
                            item.Menu != null && 
                            item.Menu.MenuId == completeMenu.MenuId);
                            
                        if (existingItem != null)
                        {
                            existingItem.Quantity += quantity;
                            
                            if (showMessage)
                            {
                                MessageBox.Show($"Cantitatea pentru meniul '{completeMenu.Name}' a fost actualizată la {existingItem.Quantity}.", 
                                    "Coș cumpărături", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        else
                        {
                            CartItems.Add(new CartItem { Menu = completeMenu, Quantity = quantity, IsMenuDish = true });
                            
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
        
        private async void CalculateTotalAmount()
        {
            decimal productTotal = CartItems.Sum(item => item.LineTotal);
            
            decimal deliveryFee = 0;
            
            if (CartItems.Count > 0)
            {
                if (productTotal >= _freeDeliveryThreshold)
                {
                    deliveryFee = 0; // Livrare gratuită
                }
                else
                {
                    deliveryFee = _standardDeliveryFee;
                }
            }
            
            DeliveryFee = deliveryFee;
            
            decimal subTotal = productTotal + deliveryFee;
            OriginalAmount = subTotal;
            
            DiscountAmount = 0;
            OrderValueDiscountAmount = 0;
            
            bool loyaltyDiscountApplied = false;
            bool orderValueDiscountApplied = false;
            decimal totalDiscountAmount = 0;
            
            if (_mainViewModel.IsUserLoggedIn && _mainViewModel.CurrentUser != null)
            {
                try 
                {
                    int userId = _mainViewModel.CurrentUser.UserId;
                    int orderCount = await CountCompletedOrders(userId);
                    
                    loyaltyDiscountApplied = orderCount >= _minimumOrdersForDiscount;
                    
                    
                    if (loyaltyDiscountApplied)
                    {
                        DiscountAmount = subTotal * ((decimal)_discountPercent / 100);
                        totalDiscountAmount += DiscountAmount;
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Eroare la calculul reducerii de loialitate: {ex.Message}";
                }
            }
            
            try
            {
                if (productTotal >= _minimumOrderValue)
                {
                    orderValueDiscountApplied = true;
                    OrderValueDiscountAmount = subTotal * ((decimal)_orderValueDiscountPercent / 100);
                    totalDiscountAmount += OrderValueDiscountAmount;
                    
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la calculul reducerii pentru valoarea comenzii: {ex.Message}";
            }
            
            DiscountApplied = loyaltyDiscountApplied;
            OrderValueDiscountApplied = orderValueDiscountApplied;
            
            TotalAmount = OriginalAmount - totalDiscountAmount;
            
            OnPropertyChanged(nameof(DiscountInfo));
            OnPropertyChanged(nameof(OrderValueDiscountInfo));
            OnPropertyChanged(nameof(DeliveryFeeInfo));
        }
        
        private async Task<int> CountCompletedOrders(int userId)
        {
            try
            {
                var comenzi = await _orderService.GetComenziUtilizatorAsync(userId);
                
                DateTime dataLimita = DateTime.Now.AddDays(-30);
                
                return comenzi.Count(c => 
                    c.Status.ToLower() == "livrata" && 
                    c.OrderDate >= dataLimita);
            }
            catch (Exception ex)
            {
                return 0;
            }
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
            
            var cartItemsCopy = new List<CartItem>(CartItems);
            
            try
            {
                bool stockIsAvailable = await VerifyStockAvailability(cartItemsCopy);
                if (!stockIsAvailable)
                {
                    return; 
                }
                
                decimal productTotal = cartItemsCopy.Sum(item => item.LineTotal);
                
                decimal deliveryFee;
                
                if (productTotal >= _freeDeliveryThreshold)
                {
                    deliveryFee = 0;
                }
                else
                {
                    deliveryFee = _standardDeliveryFee;
                }
                
                decimal finalAmount = productTotal + deliveryFee;
                
                decimal totalDiscount = 0;
                
                if (DiscountApplied)
                {
                    totalDiscount += DiscountAmount;
                }
                
                if (OrderValueDiscountApplied)
                {
                    totalDiscount += OrderValueDiscountAmount;
                }
                
                finalAmount -= totalDiscount;
                
                var order = new Order
                {
                    UserId = _mainViewModel.CurrentUser.UserId,
                    OrderDate = DateTime.Now,
                    Status = "inregistrata",
                    DeliveryFee = deliveryFee,
                    FinalAmount = finalAmount
                };

                int orderId = 0;
                
                try
                {
                    orderId = await Task.Run(async () => 
                    {
                        var savedOrder = await _orderService.AddAsync(order);
                        if (savedOrder == null || savedOrder.OrderId <= 0)
                        {
                            throw new Exception("Nu s-a putut salva comanda principală");
                        }
                        
                        int newOrderId = savedOrder.OrderId;
                        
                        await ProcessIndividualProductsAsync(newOrderId, cartItemsCopy);
                        
                        await ProcessMenuProductsAsync(newOrderId, cartItemsCopy);
                        
                        return newOrderId;
                    });
                    
                    if (orderId <= 0)
                    {
                        throw new Exception("ID-ul comenzii nu este valid");
                    }
                    
                    MessageBox.Show($"Comanda dumneavoastră a fost înregistrată cu succes.\nNumăr comandă: {orderId}\nTotal: {finalAmount:F2} lei (inclusiv taxa de transport: {deliveryFee:F2} lei)", 
                        "Comandă plasată", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    CartItems.Clear();
                    SaveCartToSession();
                    CalculateTotalAmount();
                    
                    await Task.Delay(100);
                    
                    Application.Current.Dispatcher.Invoke(() => 
                    {
                        _mainViewModel.NavigateToDishes();
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la salvarea comenzii: {ex.Message}", 
                        "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la plasarea comenzii: {ex.Message}";
                
                string detaliiEroare = ex.Message;
                
                if (ex.InnerException != null)
                {
                    detaliiEroare += $"\n\nDetalii suplimentare: {ex.InnerException.Message}";
                }
                
                MessageBox.Show($"A apărut o eroare la plasarea comenzii:\n{detaliiEroare}", 
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async Task<bool> VerifyStockAvailability(List<CartItem> cartItems)
        {
            foreach (var item in cartItems.Where(i => !i.IsMenuDish && i.Dish != null))
            {
                var dish = await _dishService.GetByIdAsync(item.Dish.DishId);
                
                if (dish == null)
                {
                    MessageBox.Show($"Preparatul '{item.Dish.Name}' nu mai este disponibil.", 
                        "Produs indisponibil", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                
                int cantitateNecesara = item.Quantity * dish.PortionSizeGrams;
                
                if (dish.TotalQuantityGrams < cantitateNecesara)
                {
                    int portiiDisponibile = dish.TotalQuantityGrams / dish.PortionSizeGrams;
                    MessageBox.Show($"Ne pare rău, nu avem suficientă cantitate pentru '{dish.Name}'.\nCantitate disponibilă: {portiiDisponibile} porții.", 
                        "Stoc insuficient", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            
            foreach (var item in cartItems.Where(i => i.IsMenuDish && i.Menu != null))
            {
                foreach (var menuDish in item.Menu.MenuDishes)
                {
                    var dish = await _dishService.GetByIdAsync(menuDish.Dish.DishId);
                    
                    if (dish == null)
                    {
                        MessageBox.Show($"Preparatul '{menuDish.Dish.Name}' din meniul '{item.Menu.Name}' nu mai este disponibil.", 
                            "Produs indisponibil", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    
                    int cantitateNecesara = item.Quantity * dish.PortionSizeGrams;
                    
                    if (dish.TotalQuantityGrams < cantitateNecesara)
                    {
                        int portiiDisponibile = dish.TotalQuantityGrams / dish.PortionSizeGrams;
                        MessageBox.Show($"Ne pare rău, nu avem suficientă cantitate pentru '{dish.Name}' din meniul '{item.Menu.Name}'.\nCantitate disponibilă: {portiiDisponibile} porții.", 
                            "Stoc insuficient", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        private async Task ProcessIndividualProductsAsync(int orderId, List<CartItem> cartItems)
        {
            foreach (var item in cartItems.Where(i => !i.IsMenuDish && i.Dish != null))
            {
                var orderDish = new OrderDish
                {
                    OrderId = orderId,
                    DishId = item.Dish.DishId,
                    Quantity = item.Quantity,
                    MenuId = null
                };
                
                await _orderService.AddOrderDishAsync(orderDish);
                
                var dish = await _dishService.GetByIdAsync(item.Dish.DishId);
                if (dish != null)
                {
                    int cantitateConsumata = item.Quantity * dish.PortionSizeGrams;
                    dish.TotalQuantityGrams -= cantitateConsumata;
                    await _dishService.UpdateAsync(dish);
                }
            }
        }
        
        private async Task ProcessMenuProductsAsync(int orderId, List<CartItem> cartItems)
        {
            foreach (var item in cartItems.Where(i => i.IsMenuDish && i.Menu != null))
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
                    
                    var dish = await _dishService.GetByIdAsync(menuDish.Dish.DishId);
                    if (dish != null)
                    {
                        int cantitateConsumata = item.Quantity * dish.PortionSizeGrams;
                        dish.TotalQuantityGrams -= cantitateConsumata;
                        await _dishService.UpdateAsync(dish);
                    }
                }
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
                LoadCartFromSession();
                
                if (CartItems.Count == 0)
                {
                    return;
                }
                
                var updatedItems = new ObservableCollection<CartItem>();
                var errorEncountered = false;
                
                foreach (var item in CartItems)
                {
                    try 
                    {
                        if (item.IsMenuDish && item.Menu != null)
                        {
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
                                else
                                {
                                    updatedItems.Add(item);
                                    errorEncountered = true;
                                }
                            }
                            else
                            {
                                updatedItems.Add(item);
                                errorEncountered = true;
                            }
                        }
                        else if (!item.IsMenuDish && item.Dish != null)
                        {
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
                            else
                            {
                                updatedItems.Add(item);
                                errorEncountered = true;
                            }
                        }
                        else
                        {
                            updatedItems.Add(item);
                            errorEncountered = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        updatedItems.Add(item);
                        errorEncountered = true;
                    }
                }
                
                if (errorEncountered)
                {
                    ErrorMessage = "Unele produse nu au putut fi reîmprospătate complet.";
                }
                
                if (updatedItems.Count > 0)
                {
                    CartItems = updatedItems;
                    SaveCartToSession();
                    CalculateTotalAmount();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la reîmprospătarea coșului: {ex.Message}";
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
            ? GetMenuPrice() * Quantity 
            : (Dish?.Price ?? 0) * Quantity;
        
        public string Name => IsMenuDish 
            ? Menu?.Name ?? "Meniu necunoscut" 
            : Dish?.Name ?? "Preparat necunoscut";
        
        public decimal UnitPrice => IsMenuDish 
            ? GetMenuPrice() 
            : Dish?.Price ?? 0;
            
        private decimal GetMenuPrice()
        {
            if (Menu == null) return 0;
            
            return Menu.HasDiscount 
                ? Menu.DiscountedPrice 
                : Menu.TotalPrice;
        }
    }
} 