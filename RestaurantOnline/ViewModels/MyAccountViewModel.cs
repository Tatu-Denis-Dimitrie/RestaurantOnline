using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RestaurantOnline.Models;
using RestaurantOnline.Services;
using System.Linq;

namespace RestaurantOnline.ViewModels
{
    public class MyAccountViewModel : ViewModelBase
    {
        private readonly OrderS _orderService;
        private readonly DishS _dishService;
        private readonly User _currentUser;
        private ObservableCollection<Order> _comenzi;
        private Order _selectedOrder;
        private bool _isLoading;
        private string _errorMessage;

        public MyAccountViewModel(OrderS orderService, DishS dishService, User currentUser)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _comenzi = new ObservableCollection<Order>();

            ViewOrderDetailsCommand = new RelayCommand(o => ViewOrderDetails(o as Order));
            RefreshCommand = new RelayCommand(_ => LoadUserOrders());
            CancelOrderCommand = new RelayCommand(o => CancelOrder(o as Order));

            LoadUserOrders();
        }

        public ObservableCollection<Order> Comenzi
        {
            get => _comenzi;
            set => SetProperty(ref _comenzi, value);
        }

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set => SetProperty(ref _selectedOrder, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string UserFullName => _currentUser.NumeComplet;
        
        public string UserEmail => _currentUser.Email;

        public ICommand ViewOrderDetailsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CancelOrderCommand { get; private set; }

        private async void LoadUserOrders()
        {
            if (IsLoading) return;

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var orders = await _orderService.GetComenziUtilizatorAsync(_currentUser.UserId);
                Comenzi.Clear();
                foreach (var order in orders)
                {
                    Comenzi.Add(order);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la încărcarea comenzilor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ViewOrderDetails(Order order)
        {
            if (order == null) return;

            try
            {
                var detalii = $"Detalii comandă:\n" +
                              $"Număr: {order.OrderId}\n" +
                              $"Data: {order.OrderDate:dd/MM/yyyy HH:mm}\n" +
                              $"Estimare livrare: {order.EstimatedDeliveryTime:dd/MM/yyyy HH:mm}\n" +
                              $"Status: {order.Status}\n" +
                              $"Total: {order.FinalAmount:F2} lei\n\n" +
                              "Produse comandate:\n";
                
                var menuGroups = order.OrderDishes
                    .GroupBy(od => od.MenuId)
                    .ToList();
                
                var individualProducts = menuGroups
                    .FirstOrDefault(g => g.Key == null);
                
                if (individualProducts != null)
                {
                    detalii += "Produse individuale:\n";
                    foreach (var item in individualProducts)
                    {
                        detalii += $"- {item.Quantity} x {item.Dish?.Name ?? "Produs necunoscut"} - {item.Dish?.Price * item.Quantity:F2} lei\n";
                    }
                    detalii += "\n";
                }
                
                var menuProducts = menuGroups
                    .Where(g => g.Key.HasValue)
                    .ToList();
                
                if (menuProducts.Any())
                {
                    detalii += "Produse din meniuri:\n";
                    foreach (var menuGroup in menuProducts)
                    {
                        var menuId = menuGroup.Key.Value;
                        detalii += $"Meniu #{menuId}:\n";
                        
                        foreach (var item in menuGroup)
                        {
                            detalii += $"- {item.Quantity} x {item.Dish?.Name ?? "Produs necunoscut"}\n";
                        }
                        detalii += "\n";
                    }
                }
                
                MessageBox.Show(detalii, "Detalii Comandă", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la afișarea detaliilor comenzii: {ex.Message}", 
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CancelOrder(Order order)
        {
            if (order == null) return;

            var result = MessageBox.Show(
                $"Sigur doriți să anulați comanda #{order.OrderId}?",
                "Confirmare anulare",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var success = await _orderService.ActualizeazaStareComandaAsync(order.OrderId, "anulata");

                order.Status = "anulata";
                MessageBox.Show(
                    $"Comanda #{order.OrderId} a fost anulată cu succes. Status modificat în baza de date.",
                    "Comandă anulată",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadUserOrders();
            } //test
            catch (Exception ex)
            {
                string mesajDetaliat = $"Eroare la actualizarea stării comenzii în baza de date: {ex.Message}";
                if (ex.InnerException != null)
                {
                    mesajDetaliat += $"\nDetalii: {ex.InnerException.Message}";
                }
                
                ErrorMessage = mesajDetaliat;
                MessageBox.Show(
                    mesajDetaliat,
                    "Eroare",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 