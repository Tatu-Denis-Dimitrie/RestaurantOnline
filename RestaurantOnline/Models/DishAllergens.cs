using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOnline.Models
{
    public class DishAllergen : BaseModel
    {
        private int _dishId;
        private int _allergenId;
        private Dish _dish;
        private Allergen _allergen;

        [Key]
        [Column(Order = 0)]
        public int DishId
        {
            get => _dishId;
            set => SetField(ref _dishId, value);
        }

        [Key]
        [Column(Order = 1)]
        public int AllergenId
        {
            get => _allergenId;
            set => SetField(ref _allergenId, value);
        }

        [ForeignKey("DishId")]
        public virtual Dish Dish
        {
            get => _dish;
            set => SetField(ref _dish, value);
        }

        [ForeignKey("AllergenId")]
        public virtual Allergen Allergen
        {
            get => _allergen;
            set => SetField(ref _allergen, value);
        }
    }
} 