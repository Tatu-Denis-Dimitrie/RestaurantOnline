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

        private async void LoadUserOrders()
        {
            if (IsLoading) return;

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var orders = await _orderService.GetComenziUtilizatorAsync(_currentUser.UserId);
                Comenzi = orders;
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
    }
} 