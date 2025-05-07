using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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

        public ComenziViewModel(
            OrderS comandaService, 
            DishS preparatService, 
            UserS utilizatorService)
        {
            _comandaService = comandaService;
            _preparatService = preparatService;
            _utilizatorService = utilizatorService;
            _comenzi = new ObservableCollection<Order>();
            
            RefreshCommand = new RelayCommand(_ => LoadComenzi());
            DetaliiComandaCommand = new RelayCommand(_ => DetaliiComanda());
            
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
            set => SetProperty(ref _comandaSelectata, value);
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
            
            // Implementare pentru detalii comanda
        }
    }
} 