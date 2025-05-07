using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RestaurantOnline.Models
{
    public class Allergen : BaseModel
    {
        private int _idAllergen;
        private string _name = string.Empty;
        private ObservableCollection<DishAllergens>? _dishAllergens;

        public Allergen()
        {
            _dishAllergens = new ObservableCollection<DishAllergens>();
        }

        [Key]
        public int IdAlergen
        {
            get => _idAllergen;
            set => SetField(ref _idAllergen, value);
        }

        [Required]
        [MaxLength(100)]
        public string Nume
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        public virtual ObservableCollection<DishAllergens>? DishAllergens
        {
            get => _dishAllergens;
            set => SetField(ref _dishAllergens, value);
        }

        [NotMapped]
        public virtual IEnumerable<Dish> Dishes => DishAllergens?.Select(pa => pa.Preparat).Where(p => p != null).Cast<Dish>() ?? Enumerable.Empty<Dish>();
    }
} 