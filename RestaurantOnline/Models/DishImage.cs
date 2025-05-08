using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOnline.Models
{
    public class DishImage : BaseModel
    {
        private int _photoId;
        private int _dishId;
        private string _url = string.Empty;
        private Dish _dish;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PhotoId
        {
            get => _photoId;
            set => SetField(ref _photoId, value);
        }

        [Required]
        public int DishId
        {
            get => _dishId;
            set => SetField(ref _dishId, value);
        }

        [Required]
        [StringLength(255)]
        public string Url
        {
            get => _url;
            set => SetField(ref _url, value);
        }

        [ForeignKey("DishId")]
        public virtual Dish Dish
        {
            get => _dish;
            set => SetField(ref _dish, value);
        }
    }
} 