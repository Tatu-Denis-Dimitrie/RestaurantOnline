using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOnline.Models
{
    public class DishImage : BaseModel
    {
        private int _idImage;
        private int _idDish;
        private string _url = string.Empty;
        private Dish? _dish;

        [Key]
        public int IdFoto
        {
            get => _idImage;
            set => SetField(ref _idImage, value);
        }

        public int IdPreparate
        {
            get => _idDish;
            set => SetField(ref _idDish, value);
        }

        [Required]
        [MaxLength(255)]
        public string Url
        {
            get => _url;
            set => SetField(ref _url, value);
        }

        [ForeignKey("IdPreparate")]
        public virtual Dish? Preparat
        {
            get => _dish;
            set => SetField(ref _dish, value);
        }
    }
} 