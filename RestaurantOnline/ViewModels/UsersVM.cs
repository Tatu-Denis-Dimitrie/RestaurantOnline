using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using RestaurantOnline.Models;
using RestaurantOnline.Services;

namespace RestaurantOnline.ViewModels
{
    public class UtilizatoriViewModel : ViewModelBase
    {
        private readonly UserS _utilizatorService;
        private ObservableCollection<User> _utilizatori;
        private string _errorMessage = string.Empty;

        public UtilizatoriViewModel(UserS utilizatorService)
        {
            _utilizatorService = utilizatorService;
            _utilizatori = new ObservableCollection<User>();
            
            StergeUtilizatorCommand = new RelayCommand(u => StergeUtilizator(u as User));
            SchimbaRolCommand = new RelayCommand(u => SchimbaRol(u as User));
            
            LoadDataAsync();
        }

        public ObservableCollection<User> Utilizatori
        {
            get => _utilizatori;
            set => SetProperty(ref _utilizatori, value);
        }
        
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        
        public ICommand StergeUtilizatorCommand { get; }
        public ICommand SchimbaRolCommand { get; }

        private async void LoadDataAsync()
        {
            try
            {
                var utilizatori = await _utilizatorService.GetAllAsync();
                Utilizatori.Clear();
                
                foreach (var utilizator in utilizatori)
                {
                    Utilizatori.Add(utilizator);
                }
                
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la incarcarea utilizatorilor: {ex.Message}";
            }
        }
        
        private async void StergeUtilizator(User? utilizator)
        {
            if (utilizator == null) return;
            
            // Nu permitem stergerea utilizatorului curent (daca este acelasi)
            if (utilizator.Role == "Angajat")
            {
                MessageBox.Show("Nu poti sterge un utilizator cu rol de Angajat.", "Restrictie", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var result = MessageBox.Show(
                $"Esti sigur ca doresti sa stergi utilizatorul '{utilizator.NumeComplet}'?", 
                "Confirmare stergere",
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
                
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _utilizatorService.DeleteUserAsync(utilizator.UserId);
                    
                    if (success)
                    {
                        MessageBox.Show("Utilizatorul a fost sters cu succes.", "Succes", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                            
                        // Reincarcam lista de utilizatori
                        LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show("Nu s-a putut sterge utilizatorul.", "Eroare", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la stergerea utilizatorului: {ex.Message}", "Eroare", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private async void SchimbaRol(User? utilizator)
        {
            if (utilizator == null) return;
            
            string rolNou = utilizator.Role == "Angajat" ? "Client" : "Angajat";
            string actiune = utilizator.Role == "Angajat" ? "retrogradezi" : "promovezi";
            
            var result = MessageBox.Show(
                $"Esti sigur ca doresti sa {actiune} utilizatorul '{utilizator.NumeComplet}' la rolul de {rolNou}?", 
                "Confirmare modificare rol",
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
                
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool success;
                    
                    if (utilizator.Role == "Angajat")
                    {
                        success = await _utilizatorService.UpdateToClientAsync(utilizator.UserId);
                    }
                    else
                    {
                        success = await _utilizatorService.UpdateToAngajatAsync(utilizator.UserId);
                    }
                    
                    if (success)
                    {
                        MessageBox.Show($"Utilizatorul a fost {(utilizator.Role == "Angajat" ? "retrogradat" : "promovat")} cu succes la rolul de {rolNou}.", 
                            "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                            
                        // Reincarcam lista de utilizatori
                        LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show("Nu s-a putut modifica rolul utilizatorului.", "Eroare", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la modificarea rolului utilizatorului: {ex.Message}", "Eroare", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
} 