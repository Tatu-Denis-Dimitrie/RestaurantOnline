using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace RestaurantOnline.Models
{
    public class User : BaseModel
    {
        private int _idUser;
        private string _name = string.Empty;
        private string _surname = string.Empty;
        private string _email = string.Empty;
        private string? _phoneNumber;
        private string? _deliveryAddress;
        private string _pass = string.Empty;
        private string _role = "Client";
        private ObservableCollection<Order>? _orders;

        public User()
        {
            _orders = new ObservableCollection<Order>();
        }

        [Key]
        public int IdUtilizator
        {
            get => _idUser;
            set => SetField(ref _idUser, value);
        }

        [Required]
        [MaxLength(100)]
        public string Nume
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        [Required]
        [MaxLength(100)]
        public string Prenume
        {
            get => _surname;
            set => SetField(ref _surname, value);
        }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email
        {
            get => _email;
            set => SetField(ref _email, value);
        }

        [MaxLength(20)]
        public string? Telefon
        {
            get => _phoneNumber;
            set => SetField(ref _phoneNumber, value);
        }

        [MaxLength(200)]
        public string? AdresaLivrare
        {
            get => _deliveryAddress;
            set => SetField(ref _deliveryAddress, value);
        }

        [Required]
        [MaxLength(255)]
        public string Parola
        {
            get => _pass;
            set => SetField(ref _pass, value);
        }

        [Required]
        [MaxLength(20)]
        public string Rol
        {
            get => _role;
            set => SetField(ref _role, value);
        }

        public virtual ObservableCollection<Order>? Comenzi
        {
            get => _orders;
            set => SetField(ref _orders, value);
        }

        public string NumeComplet => $"{Nume} {Prenume}";
    }
} 