using System.ComponentModel.DataAnnotations;

namespace RestaurantOnline.Models
{
    public class Setare : BaseModel
    {
        private int _idSetare;
        private string _nume = string.Empty;
        private string _valoare = string.Empty;

        [Key]
        public int IdSetare
        {
            get => _idSetare;
            set => SetField(ref _idSetare, value);
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
        public string Valoare
        {
            get => _valoare;
            set => SetField(ref _valoare, value);
        }
    }
} 