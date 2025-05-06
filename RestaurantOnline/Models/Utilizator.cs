using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace RestaurantOnline.Models
{
    public class Utilizator : BaseModel
    {
        private int _idUtilizator;
        private string _nume = string.Empty;
        private string _prenume = string.Empty;
        private string _email = string.Empty;
        private string? _telefon;
        private string? _adresaLivrare;
        private string _parolaHash = string.Empty;
        private string _rol = "Client"; // "Client", "Angajat", etc.
        private ObservableCollection<Comanda>? _comenzi;

        public Utilizator()
        {
            _comenzi = new ObservableCollection<Comanda>();
        }

        [Key]
        public int IdUtilizator
        {
            get => _idUtilizator;
            set => SetField(ref _idUtilizator, value);
        }

        [Required]
        [MaxLength(100)]
        public string Nume
        {
            get => _nume;
            set => SetField(ref _nume, value);
        }

        [Required]
        [MaxLength(100)]
        public string Prenume
        {
            get => _prenume;
            set => SetField(ref _prenume, value);
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
            get => _telefon;
            set => SetField(ref _telefon, value);
        }

        [MaxLength(200)]
        public string? AdresaLivrare
        {
            get => _adresaLivrare;
            set => SetField(ref _adresaLivrare, value);
        }

        [Required]
        [MaxLength(255)]
        public string ParolaHash
        {
            get => _parolaHash;
            set => SetField(ref _parolaHash, value);
        }

        [Required]
        [MaxLength(20)]
        public string Rol
        {
            get => _rol;
            set => SetField(ref _rol, value);
        }

        public virtual ObservableCollection<Comanda>? Comenzi
        {
            get => _comenzi;
            set => SetField(ref _comenzi, value);
        }

        public string NumeComplet => $"{Nume} {Prenume}";
    }
} 