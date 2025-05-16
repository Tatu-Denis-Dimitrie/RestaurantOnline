using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RestaurantOnline.ViewModels;

namespace RestaurantOnline.Models
{
    public class User : ViewModelBase
    {
        private int _userId;
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _email = string.Empty;
        private string _phone = string.Empty;
        private string _deliveryAddress = string.Empty;
        private string _password = string.Empty;
        private string _role = "Client";
        private ObservableCollection<Order> _orders;

        public User()
        {
            _orders = new ObservableCollection<Order>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        [Required]
        [StringLength(100)]
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        [Required]
        [StringLength(100)]
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        [StringLength(20)]
        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        [StringLength(200)]
        public string DeliveryAddress
        {
            get => _deliveryAddress;
            set => SetProperty(ref _deliveryAddress, value);
        }

        [Required]
        [StringLength(100)]
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        [Required]
        [StringLength(20)]
        public string Role
        {
            get => _role;
            set => SetProperty(ref _role, value);
        }

        public virtual ObservableCollection<Order> Orders
        {
            get => _orders ??= new ObservableCollection<Order>();
            set => SetProperty(ref _orders, value);
        }

        public string NumeComplet => $"{FirstName} {LastName}";
    }
} 