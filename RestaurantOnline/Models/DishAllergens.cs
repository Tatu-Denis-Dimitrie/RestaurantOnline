using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RestaurantOnline.ViewModels;

namespace RestaurantOnline.Models
{
    public class DishAllergen : ViewModelBase
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
            set => SetProperty(ref _dishId, value);
        }

        [Key]
        [Column(Order = 1)]
        public int AllergenId
        {
            get => _allergenId;
            set => SetProperty(ref _allergenId, value);
        }

        [ForeignKey("DishId")]
        public virtual Dish Dish
        {
            get => _dish;
            set => SetProperty(ref _dish, value);
        }

        [ForeignKey("AllergenId")]
        public virtual Allergen Allergen
        {
            get => _allergen;
            set => SetProperty(ref _allergen, value);
        }
    }
} 