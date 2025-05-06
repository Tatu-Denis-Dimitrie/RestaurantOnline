using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using RestaurantOnline.Models;
using RestaurantOnline.Services;

namespace RestaurantOnline.ViewModels
{
    public class PreparateViewModel : ViewModelBase
    {
        private readonly PreparatService _preparatService;
        private readonly CategorieService _categorieService;
        private readonly Dispatcher _dispatcher;
        private readonly object _lockObject = new object(); // Obiect de sincronizare
        private bool _isLoading = false; // Flag pentru a verifica dacă se încarcă date
        
        private ObservableCollection<Preparat> _preparate;
        private ObservableCollection<Categorie> _categorii;
        private Categorie? _categorieSelectata;
        private string _searchText;
        
        public PreparateViewModel(PreparatService preparatService, CategorieService categorieService)
        {
            _preparatService = preparatService ?? throw new ArgumentNullException(nameof(preparatService));
            _categorieService = categorieService ?? throw new ArgumentNullException(nameof(categorieService));
            _dispatcher = Application.Current.Dispatcher;
            
            _preparate = new ObservableCollection<Preparat>();
            _categorii = new ObservableCollection<Categorie>();
            _searchText = string.Empty;
            
            SearchCommand = new RelayCommand(_ => SearchPreparate());
            
            // Inițializăm cu o categorie implicită pentru a evita probleme de nulabilitate
            var toateCategoriile = new Categorie { IdCategorie = 0, Nume = "Toate categoriile" };
            _categorieSelectata = toateCategoriile;
            
            // Încărcăm datele inițiale cu un delay mic pentru a permite UI-ului să se inițializeze
            _dispatcher.BeginInvoke(new Action(() => LoadDataAsync()), DispatcherPriority.Loaded);
        }
        
        public ObservableCollection<Preparat> Preparate
        {
            get => _preparate;
            set => SetProperty(ref _preparate, value);
        }
        
        public ObservableCollection<Categorie> Categorii
        {
            get => _categorii;
            set => SetProperty(ref _categorii, value);
        }
        
        public Categorie? CategorieSelectata
        {
            get => _categorieSelectata;
            set
            {
                if (SetProperty(ref _categorieSelectata, value))
                {
                    FilterByCategorie();
                }
            }
        }
        
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }
        
        public ICommand SearchCommand { get; }
        
        private async void LoadDataAsync()
        {
            // Verificăm dacă există deja o operațiune în curs
            lock (_lockObject)
            {
                if (_isLoading)
                    return;
                
                _isLoading = true;
            }
            
            try
            {
                // Încărcăm categoriile întâi
                await LoadCategoriiAsync();
                
                // Apoi încărcăm preparatele
                await LoadPreparateAsync();
            }
            catch (Exception ex)
            {
                ShowError($"Eroare la încărcarea datelor: {ex.Message}");
            }
            finally
            {
                // Resetăm flag-ul
                lock (_lockObject)
                {
                    _isLoading = false;
                }
            }
        }
        
        private async Task LoadCategoriiAsync()
        {
            try
            {
                // Rulăm operațiunea pe un task separat pentru a evita blocarea UI
                var categoriiTask = Task.Run(() => _categorieService.GetAllAsync());
                var categorii = await categoriiTask;
                
                _dispatcher.Invoke(() => {
                    Categorii.Clear();
                    
                    // Adăugăm o categorie pentru "Toate"
                    var toateCategoriile = new Categorie { IdCategorie = 0, Nume = "Toate categoriile" };
                    Categorii.Add(toateCategoriile);
                    
                    foreach (var categorie in categorii)
                    {
                        Categorii.Add(categorie);
                    }
                    
                    // Setăm categoria selectată numai dacă nu a fost setată deja
                    if (_categorieSelectata?.IdCategorie == 0)
                    {
                        CategorieSelectata = toateCategoriile;
                    }
                });
            }
            catch (Exception)
            {
                _dispatcher.Invoke(() => {
                    // Adăugăm cel puțin categoria implicită dacă încărcarea eșuează
                    Categorii.Clear();
                    var toateCategoriile = new Categorie { IdCategorie = 0, Nume = "Toate categoriile" };
                    Categorii.Add(toateCategoriile);
                    CategorieSelectata = toateCategoriile;
                });
                throw;
            }
        }
        
        private async Task LoadPreparateAsync()
        {
            try
            {
                // Rulăm operațiunea pe un task separat
                var preparateTask = Task.Run(() => _preparatService.GetAllAsync());
                var preparate = await preparateTask;
                
                _dispatcher.Invoke(() => {
                    Preparate.Clear();
                    
                    foreach (var preparat in preparate)
                    {
                        Preparate.Add(preparat);
                    }
                });
            }
            catch (Exception)
            {
                _dispatcher.Invoke(() => {
                    Preparate.Clear();
                });
                throw;
            }
        }
        
        private void FilterByCategorie()
        {
            // Verificăm dacă există deja o operațiune în curs
            lock (_lockObject)
            {
                if (_isLoading)
                    return;
                
                _isLoading = true;
            }
            
            try
            {
                if (CategorieSelectata == null || CategorieSelectata.IdCategorie == 0)
                {
                    // Reîncărcăm toate preparatele
                    _dispatcher.BeginInvoke(new Action(async () => {
                        try
                        {
                            await LoadPreparateAsync();
                        }
                        catch (Exception ex)
                        {
                            ShowError($"Eroare la filtrarea preparatelor: {ex.Message}");
                        }
                        finally
                        {
                            lock (_lockObject)
                            {
                                _isLoading = false;
                            }
                        }
                    }));
                }
                else
                {
                    // Filtrăm după categoria selectată
                    _dispatcher.BeginInvoke(new Action(async () => {
                        try
                        {
                            await LoadPreparateByCategorieAsync(CategorieSelectata.IdCategorie);
                        }
                        catch (Exception ex)
                        {
                            ShowError($"Eroare la filtrarea preparatelor: {ex.Message}");
                        }
                        finally
                        {
                            lock (_lockObject)
                            {
                                _isLoading = false;
                            }
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                ShowError($"Eroare la filtrarea preparatelor: {ex.Message}");
                
                lock (_lockObject)
                {
                    _isLoading = false;
                }
            }
        }
        
        private async Task LoadPreparateByCategorieAsync(int categorieId)
        {
            try
            {
                // Rulăm operațiunea pe un task separat
                var preparateTask = Task.Run(() => _preparatService.GetByCategorie(categorieId));
                var preparate = await preparateTask;
                
                _dispatcher.Invoke(() => {
                    Preparate.Clear();
                    
                    foreach (var preparat in preparate)
                    {
                        Preparate.Add(preparat);
                    }
                });
            }
            catch (Exception ex)
            {
                ShowError($"Eroare la încărcarea preparatelor după categorie: {ex.Message}");
            }
        }
        
        private async void SearchPreparate()
        {
            // Verificăm dacă există deja o operațiune în curs
            lock (_lockObject)
            {
                if (_isLoading)
                    return;
                
                _isLoading = true;
            }
            
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    // Dacă nu avem text de căutare, reîncărcăm toate preparatele
                    await LoadPreparateAsync();
                }
                else
                {
                    // Altfel, căutăm după text
                    var preparateTask = Task.Run(() => _preparatService.SearchPreparat(SearchText));
                    var preparate = await preparateTask;
                    
                    _dispatcher.Invoke(() => {
                        Preparate.Clear();
                        
                        foreach (var preparat in preparate)
                        {
                            Preparate.Add(preparat);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                ShowError($"Eroare la căutarea preparatelor: {ex.Message}");
            }
            finally
            {
                lock (_lockObject)
                {
                    _isLoading = false;
                }
            }
        }
        
        private void ShowError(string message)
        {
            _dispatcher.Invoke(() => {
                MessageBox.Show(message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }
} 