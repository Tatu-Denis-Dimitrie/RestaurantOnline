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
        private readonly ComandaService _comandaService;
        private readonly PreparatService _preparatService;
        private readonly UtilizatorService _utilizatorService;
        private ObservableCollection<Comanda> _comenzi;
        private bool _isLoading;

        public ComenziViewModel(
            ComandaService comandaService, 
            PreparatService preparatService, 
            UtilizatorService utilizatorService)
        {
            _comandaService = comandaService;
            _preparatService = preparatService;
            _utilizatorService = utilizatorService;
            _comenzi = new ObservableCollection<Comanda>();
            
            LoadDataAsync();
        }

        public ObservableCollection<Comanda> Comenzi
        {
            get => _comenzi;
            set => SetProperty(ref _comenzi, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private async void LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                
                // Încărcăm toate comenzile cu utilizator și preparate
                var comenzi = await _comandaService.GetComenziCompleteAsync();
                Comenzi.Clear();
                
                foreach (var comanda in comenzi)
                {
                    Comenzi.Add(comanda);
                }
            }
            catch (Exception ex)
            {
                // În mod normal aici ar trebui să afișăm o eroare sau să logăm
                System.Diagnostics.Debug.WriteLine($"Eroare la încărcarea comenzilor: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 