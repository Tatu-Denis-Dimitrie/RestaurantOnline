using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RestaurantOnline.Models;
using RestaurantOnline.Services;

namespace RestaurantOnline.ViewModels
{
    public class ComenziViewModel : ViewModelBase
    {
        private readonly OrderS _comandaService;
        private readonly DishS _preparatService;
        private readonly UserS _utilizatorService;
        private ObservableCollection<Order> _comenzi;
        private Order _comandaSelectata;
        private bool _isLoading;
        private string _errorMessage;
        private string _selectedStatus;
        private readonly List<string> _availableStatuses;

        public ComenziViewModel(
            OrderS comandaService, 
            DishS preparatService, 
            UserS utilizatorService)
        {
            _comandaService = comandaService;
            _preparatService = preparatService;
            _utilizatorService = utilizatorService;
            _comenzi = new ObservableCollection<Order>();
            
            // Lista statusurilor disponibile
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
            
            LoadComenzi();
        }

        public ObservableCollection<Order> Comenzi
        {
            get => _comenzi;
            set => SetProperty(ref _comenzi, value);
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

        public ICommand RefreshCommand { get; }
        public ICommand DetaliiComandaCommand { get; }
        public ICommand SchimbaStatusCommand { get; }

        private async void LoadComenzi()
            {
                IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var comenzi = await _comandaService.GetAllAsync();
                Comenzi = comenzi;
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

        private void DetaliiComanda()
        {
            if (ComandaSelectata == null) return;
            
            var detalii = $"ID Comandă: {ComandaSelectata.OrderId}\n" +
                          $"Client: {ComandaSelectata.User?.NumeComplet ?? "N/A"}\n" +
                          $"Data: {ComandaSelectata.OrderDate:dd/MM/yyyy HH:mm}\n" +
                          $"Status: {ComandaSelectata.Status}\n" +
                          $"Total: {ComandaSelectata.FinalAmount:F2} lei\n\n" +
                          "Produse:\n";
            
            foreach (var item in ComandaSelectata.OrderDishes)
            {
                detalii += $"- {item.Quantity} x {item.Dish?.Name ?? "Produs necunoscut"}\n";
            }
            
            MessageBox.Show(detalii, "Detalii Comandă", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private async void SchimbaStatusComanda()
        {
            if (ComandaSelectata == null || string.IsNullOrEmpty(SelectedStatus)) return;
            
            // Confirmă schimbarea statusului
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
                    // Actualizează statusul local pentru a reflecta schimbarea
                    ComandaSelectata.Status = SelectedStatus;
                    MessageBox.Show(
                        $"Statusul comenzii #{ComandaSelectata.OrderId} a fost schimbat cu succes în '{SelectedStatus}'.",
                        "Status actualizat",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    
                    // Reîmprospătează lista de comenzi
                    await Task.Delay(500); // pauză mică pentru a lăsa DB să se actualizeze
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
                ErrorMessage = $"Eroare la actualizarea statusului comenzii: {ex.Message}";
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