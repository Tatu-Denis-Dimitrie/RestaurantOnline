using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOnline.Models
{
    public class Setting : BaseModel
    {
        private int _settingId;
        private string _name = string.Empty;
        private string _value = string.Empty;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SettingId
        {
            get => _settingId;
            set => SetField(ref _settingId, value);
        }

        [Required]
        [StringLength(100)]
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        [Required]
        [StringLength(100)]
        public string Value
        {
            get => _value;
            set => SetField(ref _value, value);
        }
    }
} 