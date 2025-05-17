using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RestaurantOnline.Models;
using RestaurantOnline.Services;

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
                // Forțăm context-ul să ignore cache-ul și să reîncarce datele din baza de date
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
                MessageBox.Show($"Detalii comandă:\n" +
                                $"Număr: {order.OrderId}\n" +
                                $"Data: {order.OrderDate}\n" +
                                $"Status: {order.Status}\n" +
                                $"Total: {order.FinalAmount:F2} lei",
                                "Detalii Comandă",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
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

            // Confirmă cu utilizatorul
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
                // IMPORTANT: Actualizează statusul comenzii în baza de date folosind OrderService
                var success = await _orderService.ActualizeazaStareComandaAsync(order.OrderId, "anulata");

                // Actualizează și local statusul comenzii pentru a reflecta imediat în UI
                order.Status = "anulata";
                MessageBox.Show(
                    $"Comanda #{order.OrderId} a fost anulată cu succes. Status modificat în baza de date.",
                    "Comandă anulată",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Reîncărcăm comenzile pentru a reflecta schimbările din baza de date
                LoadUserOrders();
            }
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