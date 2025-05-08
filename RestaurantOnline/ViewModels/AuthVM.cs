using System;
using System.Windows.Input;
using RestaurantOnline.Models;
using RestaurantOnline.Services;

namespace RestaurantOnline.ViewModels
{
    public class AuthVM : ViewModelBase
    {
        private readonly IUserS _utilizatorService;
        private readonly MainViewModel _mainViewModel;
        private string _email;
        private string _parola;
        private string _nume;
        private string _prenume;
        private string _telefon;
        private string _adresaLivrare;
        private string _errorMessage;
        private bool _isLoginMode = true;

        public AuthVM(IUserS utilizatorService, MainViewModel mainViewModel, bool isRegisterMode = false)
        {
            _utilizatorService = utilizatorService;
            _mainViewModel = mainViewModel;
            _isLoginMode = !isRegisterMode;
            LoginCommand = new RelayCommand(_ => Login());
            RegisterCommand = new RelayCommand(_ => Register());
            ToggleModeCommand = new RelayCommand(_ => ToggleMode());
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        public string Parola
        {
            get => _parola;
            set
            {
                _parola = value;
                OnPropertyChanged();
            }
        }

        public string Nume
        {
            get => _nume;
            set
            {
                _nume = value;
                OnPropertyChanged();
            }
        }

        public string Prenume
        {
            get => _prenume;
            set
            {
                _prenume = value;
                OnPropertyChanged();
            }
        }

        public string Telefon
        {
            get => _telefon;
            set
            {
                _telefon = value;
                OnPropertyChanged();
            }
        }

        public string AdresaLivrare
        {
            get => _adresaLivrare;
            set
            {
                _adresaLivrare = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ErrorMessageVisibility));
            }
        }

        public System.Windows.Visibility ErrorMessageVisibility => string.IsNullOrEmpty(ErrorMessage) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

        public bool IsLoginMode
        {
            get => _isLoginMode;
            set
            {
                _isLoginMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LoginButtonVisibility));
                OnPropertyChanged(nameof(RegisterButtonVisibility));
                OnPropertyChanged(nameof(FormTitle));
            }
        }

        public string FormTitle => IsLoginMode ? "Autentificare" : "Înregistrare";
        public System.Windows.Visibility LoginButtonVisibility => IsLoginMode ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility RegisterButtonVisibility => IsLoginMode ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        public System.Windows.Visibility RegisterFieldsVisibility => IsLoginMode ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand ToggleModeCommand { get; }

        private void Login()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Parola))
            {
                ErrorMessage = "Va rugam sa completati toate campurile obligatorii.";
                return;
            }

            try
            {
                var utilizator = _utilizatorService.Autentificare(Email, Parola);
                if (utilizator != null)
                {
                    _mainViewModel.UtilizatorCurent = utilizator;
                    _mainViewModel.NavigateToHome();
                }
                else
                {
                    ErrorMessage = "Email sau parola incorecta.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la autentificare: {ex.Message}";
            }
        }

        private void Register()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Parola) ||
                string.IsNullOrWhiteSpace(Nume) || string.IsNullOrWhiteSpace(Prenume) ||
                string.IsNullOrWhiteSpace(Telefon) || string.IsNullOrWhiteSpace(AdresaLivrare))
            {
                ErrorMessage = "Va rugam sa completati toate campurile obligatorii.";
                return;
            }

            try
            {
                var utilizator = new User
                {
                    Email = Email,
                    Parola = Parola, // in productie, parola ar trebui hash-uita
                    Nume = Nume,
                    Prenume = Prenume,
                    Telefon = Telefon,
                    AdresaLivrare = AdresaLivrare,
                    Rol = "Client" // Rol implicit pentru utilizatorii noi
                };

                _utilizatorService.Adauga(utilizator);
                _mainViewModel.UtilizatorCurent = utilizator;
                _mainViewModel.NavigateToHome();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la inregistrare: {ex.Message}";
            }
        }

        private void ToggleMode()
        {
            IsLoginMode = !IsLoginMode;
            ErrorMessage = string.Empty;
        }
    }
} 