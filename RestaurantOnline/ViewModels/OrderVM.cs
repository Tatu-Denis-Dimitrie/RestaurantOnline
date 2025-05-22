using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RestaurantOnline.Models;
using RestaurantOnline.Services;
using System.Linq;

namespace RestaurantOnline.ViewModels
{
    public class ComenziViewModel : ViewModelBase
    {
        private readonly OrderS _comandaService;
        private readonly DishS _preparatService;
        private readonly UserS _utilizatorService;
        private ObservableCollection<Order> _comenzi;
        private ObservableCollection<Order> _comenziAfisate;
        private Order _comandaSelectata;
        private bool _isLoading;
        private string _errorMessage;
        private string _selectedStatus;
        private readonly List<string> _availableStatuses;
        private bool _arataDoarComenziActive;
        private string _filtruStatus;

        public ComenziViewModel(
            OrderS comandaService, 
            DishS preparatService, 
            UserS utilizatorService)
        {
            _comandaService = comandaService;
            _preparatService = preparatService;
            _utilizatorService = utilizatorService;
            _comenzi = new ObservableCollection<Order>();
            _comenziAfisate = new ObservableCollection<Order>();
            
            _availableStatuses = new List<string> 
            { 
                "inregistrata", 
                "se_pregateste", 
                "a plecat la client", 
                "livrata",
                "anulata"
            };
            
            RefreshCommand = new RelayCommand(_ => LoadComenzi());
            DetaliiComandaCommand = new RelayCommand(_ => DetaliiComanda());
            SchimbaStatusCommand = new RelayCommand(_ => SchimbaStatusComanda(), _ => ComandaSelectata != null);
            ArataComenziActiveCommand = new RelayCommand(_ => SetArataDoarComenziActive(true));
            ArataComenziToateCommand = new RelayCommand(_ => SetArataDoarComenziActive(false));
            
            _arataDoarComenziActive = false;
            LoadComenzi();
        }

        public ObservableCollection<Order> Comenzi
        {
            get => _comenzi;
            set => SetProperty(ref _comenzi, value);
        }

        public ObservableCollection<Order> ComenziAfisate
        {
            get => _comenziAfisate;
            set => SetProperty(ref _comenziAfisate, value);
        }

        public Order ComandaSelectata
        {
            get => _comandaSelectata;
            set
            {
                SetProperty(ref _comandaSelectata, value);
                if (value != null)
                {
                    SelectedStatus = value.Status;
                }
            }
        }

        public List<string> StatusuriDisponibile => _availableStatuses;

        public string SelectedStatus
        {
            get => _selectedStatus;
            set => SetProperty(ref _selectedStatus, value);
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

        public bool ArataDoarComenziActive
        {
            get => _arataDoarComenziActive;
            private set => SetProperty(ref _arataDoarComenziActive, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand DetaliiComandaCommand { get; }
        public ICommand SchimbaStatusCommand { get; }
        public ICommand ArataComenziActiveCommand { get; }
        public ICommand ArataComenziToateCommand { get; }

        private async void LoadComenzi()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var comenzi = await _comandaService.GetAllAsync();
                
                var comenziSortate = comenzi.OrderByDescending(c => c.OrderDate).ToList();
                Comenzi = new ObservableCollection<Order>(comenziSortate);
                
                ActualizeazaComenziAfisate();
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Eroare la incarcarea comenzilor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SetArataDoarComenziActive(bool value)
        {
            ArataDoarComenziActive = value;
            ActualizeazaComenziAfisate();
        }

        private void ActualizeazaComenziAfisate()
        {
            if (ArataDoarComenziActive)
            {
                var comenziActive = Comenzi.Where(c => 
                    c.Status != "livrata" && 
                    c.Status != "anulata").ToList();
                
                ComenziAfisate = new ObservableCollection<Order>(comenziActive);
            }
            else
            {
                ComenziAfisate = new ObservableCollection<Order>(Comenzi);
            }
        }

        private void DetaliiComanda()
        {
            if (ComandaSelectata == null) return;
            
            var detalii = $"ID Comandă: {ComandaSelectata.OrderId}\n" +
                          $"Data: {ComandaSelectata.OrderDate:dd/MM/yyyy HH:mm}\n" +
                          $"Estimare livrare: {ComandaSelectata.EstimatedDeliveryTime:dd/MM/yyyy HH:mm}\n" +
                          $"Status: {ComandaSelectata.Status}\n" +
                          $"Total: {ComandaSelectata.FinalAmount:F2} lei\n\n" +
                          $"Informații Client:\n" +
                          $"Nume: {ComandaSelectata.User?.NumeComplet ?? "N/A"}\n" +
                          $"Email: {ComandaSelectata.User?.Email ?? "N/A"}\n" +
                          $"Telefon: {ComandaSelectata.User?.Phone ?? "N/A"}\n" +
                          $"Adresă livrare: {ComandaSelectata.User?.DeliveryAddress ?? "N/A"}\n\n" +
                          "Produse comandate:\n";
            
            var menuGroups = ComandaSelectata.OrderDishes
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
            
            MessageBox.Show(detalii, $"Detalii Comandă #{ComandaSelectata.OrderId}", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void SchimbaStatusComanda()
        {
            if (ComandaSelectata == null || string.IsNullOrEmpty(SelectedStatus)) return;
            
            var result = MessageBox.Show(
                $"Doriți să schimbați statusul comenzii #{ComandaSelectata.OrderId} din '{ComandaSelectata.Status}' în '{SelectedStatus}'?",
                "Confirmare schimbare status",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
                
            if (result != MessageBoxResult.Yes) return;
            
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            try
            {
                var success = await _comandaService.ActualizeazaStareComandaAsync(ComandaSelectata.OrderId, SelectedStatus);
                
                if (success)
                {
                    ComandaSelectata.Status = SelectedStatus;
                    MessageBox.Show(
                        $"Statusul comenzii #{ComandaSelectata.OrderId} a fost schimbat cu succes în '{SelectedStatus}'.",
                        "Status actualizat",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    
                    await Task.Delay(500); 
                    LoadComenzi();
                }
                else
                {
                    MessageBox.Show(
                        $"Nu s-a putut actualiza statusul comenzii #{ComandaSelectata.OrderId}.",
                        "Eroare",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Eroare la actualizarea statusului comenzii: {ex.Message}",
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