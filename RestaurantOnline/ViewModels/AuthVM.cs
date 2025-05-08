using System;
using System.Windows.Input;
using RestaurantOnline.Models;
using RestaurantOnline.Services;

namespace RestaurantOnline.ViewModels
{
    public class AuthVM : ViewModelBase
    {
        private readonly IUserS _ServiceUser;
        private readonly MainViewModel _mainViewModel;
        private string _email;
        private string _password;
        private string _name;
        private string _surname;
        private string _phoneNumber;
        private string _deliveryAddress;
        private string _errorMessage;
        private bool _isLoginMode = true;

        public AuthVM(IUserS utilizatorService, MainViewModel mainViewModel, bool isRegisterMode = false)
        {
            _ServiceUser = utilizatorService;
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
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string Nume
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Prenume
        {
            get => _surname;
            set
            {
                _surname = value;
                OnPropertyChanged();
            }
        }

        public string Telefon
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;
                OnPropertyChanged();
            }
        }

        public string AdresaLivrare
        {
            get => _deliveryAddress;
            set
            {
                _deliveryAddress = value;
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
                var utilizator = _ServiceUser.Autentificare(Email, Parola);
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
                    Password = Parola, // in productie, parola ar trebui hash-uita
                    FirstName = Nume,
                    LastName = Prenume,
                    Phone = Telefon,
                    DeliveryAddress = AdresaLivrare,
                    Role = "Client" // Rol implicit pentru utilizatorii noi
                };

                _ServiceUser.Adauga(utilizator);
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