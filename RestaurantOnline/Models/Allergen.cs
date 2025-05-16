using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using RestaurantOnline.ViewModels;

namespace RestaurantOnline.Models
{
    public class Allergen : ViewModelBase
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
            set => SetProperty(ref _allergenId, value);
        }

        [Required]
        [StringLength(100)]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public virtual ObservableCollection<DishAllergen>? DishAllergens
        {
            get => _dishAllergens;
            set => SetProperty(ref _dishAllergens, value);
        }

        [NotMapped]
        public virtual IEnumerable<Dish> Dishes => DishAllergens?.Select(pa => pa.Dish).Where(p => p != null).Cast<Dish>() ?? Enumerable.Empty<Dish>();
    }
} 