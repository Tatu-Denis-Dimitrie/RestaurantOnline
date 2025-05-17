using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using RestaurantOnline.Models;
using RestaurantOnline.Services;

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
            
            // Încarcă coșul salvat dacă există
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
        
        // Adaugă un preparat în coș
        public void AddToCart(Dish dish, int quantity = 1, bool showMessage = true)
        {
            if (dish == null) return;
            
            // Verifică dacă produsul există deja în coș
            var existingItem = CartItems.FirstOrDefault(item => item.Dish.DishId == dish.DishId);
            if (existingItem != null)
            {
                // Crește cantitatea
                existingItem.Quantity += quantity;
            }
            else
            {
                // Adaugă un nou item
                CartItems.Add(new CartItem { Dish = dish, Quantity = quantity });
            }
            
            SaveCartToSession();
            CalculateTotalAmount();
            
            if (showMessage)
            {
                MessageBox.Show($"Produsul '{dish.Name}' a fost adăugat în coș.", "Coș cumpărături", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        // Șterge un produs din coș
        private void RemoveFromCart(object parameter)
        {
            if (parameter is CartItem item)
            {
                CartItems.Remove(item);
                SaveCartToSession();
                CalculateTotalAmount();
            }
        }
        
        // Calculează suma totală a produselor din coș
        private void CalculateTotalAmount()
        {
            // Suma produselor
            decimal productTotal = CartItems.Sum(item => item.Dish.Price * item.Quantity);
            
            // Adăugăm taxa de transport fixă (10 lei) doar dacă avem produse în coș
            decimal deliveryFee = CartItems.Count > 0 ? 10.00m : 0;
            
            // Suma totală = produse + transport
            TotalAmount = productTotal + deliveryFee;
        }
        
        // Verifică dacă se poate plasa comanda
        private bool CanPlaceOrder()
        {
            return _mainViewModel.IsUserLoggedIn && CartItems.Count > 0;
        }
        
        // Plasează comanda
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
                // Verificăm stocul disponibil pentru fiecare produs
                foreach (var item in CartItems)
                {
                    // Obținem detaliile actualizate ale preparatului
                    var dish = await _dishService.GetByIdAsync(item.Dish.DishId);
                    
                    if (dish == null)
                    {
                        MessageBox.Show($"Preparatul '{item.Dish.Name}' nu mai este disponibil.", 
                            "Produs indisponibil", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    // Calculăm cantitatea necesară pentru comanda curentă
                    int cantitateNecesara = item.Quantity * dish.PortionSizeGrams;
                    
                    // Verificăm dacă avem suficient stoc
                    if (dish.TotalQuantityGrams < cantitateNecesara)
                    {
                        int portiiDisponibile = dish.TotalQuantityGrams / dish.PortionSizeGrams;
                        MessageBox.Show($"Ne pare rău, nu avem suficientă cantitate pentru '{dish.Name}'.\nCantitate disponibilă: {portiiDisponibile} porții.", 
                            "Stoc insuficient", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                
                // Calculăm suma produselor
                decimal productTotal = CartItems.Sum(item => item.Dish.Price * item.Quantity);
                // Taxa fixă de livrare
                decimal deliveryFee = 10.00m;
                // Suma finală = produse + transport
                decimal finalAmount = productTotal + deliveryFee;
                
                // Creează o comandă nouă
                var order = new Order
                {
                    UserId = _mainViewModel.CurrentUser.UserId,
                    OrderDate = DateTime.Now,
                    Status = "inregistrata",
                    DeliveryFee = deliveryFee,
                    FinalAmount = finalAmount // Suma finală include produsele + taxa de transport
                };
                
                // Adaugă detaliile comenzii
                foreach (var item in CartItems)
                {
                    order.OrderDishes.Add(new OrderDish
                    {
                        DishId = item.Dish.DishId,
                        Quantity = item.Quantity
                    });
                }
                
                // Salvează comanda în baza de date
                var savedOrder = await _orderService.AddAsync(order);
                
                if (savedOrder != null)
                {
                    // Actualizăm stocul pentru fiecare produs
                    foreach (var item in CartItems)
                    {
                        // Obținem preparatul din baza de date
                        var dish = await _dishService.GetByIdAsync(item.Dish.DishId);
                        if (dish != null)
                        {
                            // Calculăm cantitatea consumată
                            int cantitateConsumata = item.Quantity * dish.PortionSizeGrams;
                            
                            // Actualizăm stocul
                            dish.TotalQuantityGrams -= cantitateConsumata;
                            
                            // Salvăm modificările
                            await _dishService.UpdateAsync(dish);
                        }
                    }
                    
                    MessageBox.Show($"Comanda dumneavoastră a fost înregistrată cu succes.\nNumăr comandă: {savedOrder.OrderId}\nTotal: {finalAmount:F2} lei (inclusiv taxa de transport: {deliveryFee:F2} lei)", 
                        "Comandă plasată", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Golește coșul după plasarea comenzii
                    CartItems.Clear();
                    SaveCartToSession();
                    CalculateTotalAmount();
                    
                    // Navighează înapoi la lista de preparate
                    _mainViewModel.NavigateToDishes();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la plasarea comenzii: {ex.Message}";
                MessageBox.Show($"A apărut o eroare la plasarea comenzii: {ex.Message}", 
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Salvează coșul în sesiune (pentru exemplu, doar reține referința)
        private void SaveCartToSession()
        {
            // În implementarea reală, ai putea salva în localStorage sau alt mecanism de persistență
            App.Current.Properties["CartItems"] = CartItems;
        }
        
        // Încarcă coșul din sesiune
        private void LoadCartFromSession()
        {
            try
            {
                if (App.Current.Properties.Contains("CartItems") && 
                    App.Current.Properties["CartItems"] is ObservableCollection<CartItem> savedCart)
                {
                    // Filtrez doar itemele valide, care au Dish inițializat
                    var validItems = savedCart.Where(item => item?.Dish != null).ToList();
                    CartItems = new ObservableCollection<CartItem>(validItems);
                }
            }
            catch (Exception ex)
            {
                // În caz de eroare, inițializăm un coș gol
                CartItems = new ObservableCollection<CartItem>();
                ErrorMessage = $"Eroare la încărcarea coșului: {ex.Message}";
            }
        }
    }
    
    // Clasa care reprezintă un item din coș
    public class CartItem : ViewModelBase
    {
        private Dish _dish;
        private int _quantity = 1;
        
        public Dish Dish
        {
            get => _dish;
            set => SetProperty(ref _dish, value);
        }
        
        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }
        
        public decimal LineTotal => Dish?.Price * Quantity ?? 0;
    }
} 