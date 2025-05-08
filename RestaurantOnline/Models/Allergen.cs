using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RestaurantOnline.Models
{
    public class Allergen : BaseModel
    {
        private int _allergenId;
        private string _name = string.Empty;
        private ObservableCollection<DishAllergen>? _dishAllergens;

        public Allergen()
        {
            _dishAllergens = new ObservableCollection<DishAllergen>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AllergenId
        {
            get => _allergenId;
            set => SetField(ref _allergenId, value);
        }

        [Required]
        [StringLength(100)]
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        public virtual ObservableCollection<DishAllergen>? DishAllergens
        {
            get => _dishAllergens;
            set => SetField(ref _dishAllergens, value);
        }

        [NotMapped]
        public virtual IEnumerable<Dish> Dishes => DishAllergens?.Select(pa => pa.Dish).Where(p => p != null).Cast<Dish>() ?? Enumerable.Empty<Dish>();
    }
} 