using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOnline.Models
{
    public class DishAllergens : BaseModel
    {
        private int _idDish;
        private int _idAllergen;
        private Dish? _dish;
        private Allergen? _allergen;

        [Key, Column(Order = 0)]
        public int IdPreparate
        {
            get => _idDish;
            set => SetField(ref _idDish, value);
        }

        [Key, Column(Order = 1)]
        public int IdAlergen
        {
            get => _idAllergen;
            set => SetField(ref _idAllergen, value);
        }

        [ForeignKey("IdPreparate")]
        public virtual Dish? Preparat
        {
            get => _dish;
            set => SetField(ref _dish, value);
        }

        [ForeignKey("IdAlergen")]
        public virtual Allergen? Alergen
        {
            get => _allergen;
            set => SetField(ref _allergen, value);
        }
    }
} 