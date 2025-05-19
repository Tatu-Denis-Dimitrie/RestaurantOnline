using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RestaurantOnline.Models;
using RestaurantOnline.Services;

namespace RestaurantOnline.ViewModels
{
    public class StockViewModel : ViewModelBase
    {
        private readonly DishS _dishService;
        private bool _isLoading;
        private string _errorMessage;
        private ObservableCollection<Dish> _preparate;
        private int _stockThreshold;

        public StockViewModel(DishS dishService)
        {
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _preparate = new ObservableCollection<Dish>();
            
            // Obține valoarea pragului din configurație
            var app = App.Current as App;
            _stockThreshold = app?.AppSettings?.StockThreshold ?? 1000;
            
            RefreshCommand = new RelayCommand(_ => LoadStockAsync());
            
            // Încărcarea inițială a datelor
            LoadStockAsync();
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

        public ObservableCollection<Dish> Preparate
        {
            get => _preparate;
            set => SetProperty(ref _preparate, value);
        }
        
        public int StockThreshold
        {
            get => _stockThreshold;
        }

        public ICommand RefreshCommand { get; }

        private async void LoadStockAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                
                // Încarcă toate preparatele pentru a vedea stocul
                var preparate = await _dishService.GetAllAsync();
                
                // Sortăm preparatele după nume pentru afișare
                var sortedPreparate = preparate.OrderBy(p => p.Name).ToList();
                
                // Actualizează colecția
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Preparate.Clear();
                    foreach (var preparat in sortedPreparate)
                    {
                        Preparate.Add(preparat);
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la încărcarea stocului: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 