using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using RestaurantOnline.Models;
using RestaurantOnline.Services;

namespace RestaurantOnline.ViewModels
{
    public class AllergenViewModel : ViewModelBase
    {
        private readonly AllergenS _allergenService;
        private ObservableCollection<Allergen> _allergens;
        private Allergen _selectedAllergen;
        private string _newAllergenName;
        private string _errorMessage;
        private bool _isLoading;

        public AllergenViewModel(AllergenS allergenService)
        {
            _allergenService = allergenService ?? throw new ArgumentNullException(nameof(allergenService));
            _allergens = new ObservableCollection<Allergen>();
            _newAllergenName = string.Empty;
            _errorMessage = string.Empty;

            SaveCommand = new RelayCommand(_ => SaveAllergen(), _ => CanSaveAllergen());
            DeleteCommand = new RelayCommand(_ => DeleteAllergen(), _ => CanDeleteAllergen());
            RefreshCommand = new RelayCommand(_ => LoadAllergens());

            LoadAllergens();
        }

        public ObservableCollection<Allergen> Allergens
        {
            get => _allergens;
            set => SetProperty(ref _allergens, value);
        }

        public Allergen SelectedAllergen
        {
            get => _selectedAllergen;
            set
            {
                if (SetProperty(ref _selectedAllergen, value) && value != null)
                {
                    NewAllergenName = value.Name;
                }
            }
        }

        public string NewAllergenName
        {
            get => _newAllergenName;
            set => SetProperty(ref _newAllergenName, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        private async void LoadAllergens()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var allergens = await _allergenService.GetAllAsync();
                Allergens = allergens;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la încărcarea alergenilor: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanSaveAllergen()
        {
            return !string.IsNullOrWhiteSpace(NewAllergenName);
        }

        private async void SaveAllergen()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                if (SelectedAllergen == null)
                {
                    // Adaugă alergen nou
                    var newAllergen = new Allergen { Name = NewAllergenName };
                    await _allergenService.AddAsync(newAllergen);
                    MessageBox.Show("Alergenul a fost adăugat cu succes.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Actualizează alergen existent
                    SelectedAllergen.Name = NewAllergenName;
                    await _allergenService.UpdateAsync(SelectedAllergen);
                    MessageBox.Show("Alergenul a fost actualizat cu succes.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                NewAllergenName = string.Empty;
                SelectedAllergen = null;
                LoadAllergens();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la salvarea alergenului: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanDeleteAllergen()
        {
            return SelectedAllergen != null;
        }

        private async void DeleteAllergen()
        {
            if (SelectedAllergen == null) return;

            // Confirmăm ștergerea
            var result = MessageBox.Show(
                $"Sigur doriți să ștergeți alergenul '{SelectedAllergen.Name}'?",
                "Confirmare ștergere",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                await _allergenService.DeleteAsync(SelectedAllergen.AllergenId);
                MessageBox.Show("Alergenul a fost șters cu succes.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                NewAllergenName = string.Empty;
                SelectedAllergen = null;
                LoadAllergens();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la ștergerea alergenului: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 