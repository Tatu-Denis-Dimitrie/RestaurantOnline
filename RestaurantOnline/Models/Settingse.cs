using System.ComponentModel.DataAnnotations;

namespace RestaurantOnline.Models
{
    public class Settingse : BaseModel
    {
        private int _idSettings;
        private string _name = string.Empty;
        private string _value = string.Empty;

        [Key]
        public int IdSetare
        {
            get => _idSettings;
            set => SetField(ref _idSettings, value);
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
        public string Valoare
        {
            get => _value;
            set => SetField(ref _value, value);
        }
    }
} 