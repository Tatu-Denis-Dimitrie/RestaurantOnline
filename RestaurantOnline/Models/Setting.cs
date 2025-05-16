using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RestaurantOnline.ViewModels;

namespace RestaurantOnline.Models
{
    public class Setting : ViewModelBase
    {
        private int _settingId;
        private string _name = string.Empty;
        private string _value = string.Empty;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SettingId
        {
            get => _settingId;
            set => SetProperty(ref _settingId, value);
        }

        [Required]
        [StringLength(100)]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        [Required]
        [StringLength(100)]
        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }
} 