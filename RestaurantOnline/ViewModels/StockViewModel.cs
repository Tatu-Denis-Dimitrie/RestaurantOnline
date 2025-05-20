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
        private string _successMessage;
        private ObservableCollection<Dish> _preparate;
        private int _stockThreshold;
        private Dish _selectedPreparat;
        private int _quantityToAdd;

        public StockViewModel(DishS dishService)
        {
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _preparate = new ObservableCollection<Dish>();
            _quantityToAdd = 0;
            
            // Obține valoarea pragului din configurație
            var app = App.Current as App;
            _stockThreshold = app?.AppSettings?.StockThreshold ?? 1000;
            
            RefreshCommand = new RelayCommand(_ => LoadStockAsync());
            UpdateStockCommand = new RelayCommand(_ => UpdateStockAsync(), _ => CanUpdateStock());
            
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
        
        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
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
        
        public Dish SelectedPreparat
        {
            get => _selectedPreparat;
            set 
            {
                if (SetProperty(ref _selectedPreparat, value))
                {
                    // Resetăm eventuale mesaje când se schimbă selecția
                    SuccessMessage = string.Empty;
                    ErrorMessage = string.Empty;
                }
            }
        }
        
        public int QuantityToAdd
        {
            get => _quantityToAdd;
            set => SetProperty(ref _quantityToAdd, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand UpdateStockCommand { get; }
        
        private bool CanUpdateStock()
        {
            return SelectedPreparat != null && !IsLoading;
        }
        
        private async void UpdateStockAsync()
        {
            if (SelectedPreparat == null)
            {
                ErrorMessage = "Selectați un preparat pentru actualizarea stocului.";
                return;
            }
            
            // Verificăm dacă utilizatorul a introdus o valoare numerică validă
            if (QuantityToAdd == 0)
            {
                ErrorMessage = "Introduceți o cantitate diferită de zero pentru actualizarea stocului.";
                return;
            }
            
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;
                
                int dishId = SelectedPreparat.DishId;
                string dishName = SelectedPreparat.Name;
                int quantityToAdd = QuantityToAdd;
                
                // Salvăm valorile înainte de a apela serviciul pentru a evita modificări concurente
                Console.WriteLine($"Se actualizează stocul pentru {dishName} (ID: {dishId}) cu {quantityToAdd}g");
                
                bool success = await _dishService.UpdateStockAsync(dishId, quantityToAdd);
                
                if (success)
                {
                    // Reîncărcăm datele pentru a afișa valorile actualizate
                    await ReloadDataAsync();
                    
                    // Reselecțăm preparatul
                    SelectedPreparat = Preparate.FirstOrDefault(p => p.DishId == dishId);
                    
                    // Afișăm mesaj de succes
                    SuccessMessage = $"Stocul pentru {dishName} a fost actualizat cu succes.";
                    
                    // Resetăm cantitatea de adăugat
                    QuantityToAdd = 0;
                }
                else
                {
                    ErrorMessage = "Nu s-a putut actualiza stocul. Verificați cantitatea introdusă.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la actualizarea stocului: {ex.Message}";
                if (ex.InnerException != null)
                {
                    ErrorMessage += $" Detalii: {ex.InnerException.Message}";
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        // Metodă separată pentru reîncărcarea datelor
        private async Task ReloadDataAsync()
        {
            try
            {
                // Încarcă toate preparatele pentru a vedea stocul actualizat
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
                ErrorMessage = $"Eroare la reîncărcarea datelor: {ex.Message}";
            }
        }

        private async void LoadStockAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;
                
                // Utilizăm metoda comună pentru încărcarea datelor
                await ReloadDataAsync();
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