using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using RestaurantOnline.ViewModels;

namespace RestaurantOnline.Models
{
    public class Dish : ViewModelBase
    {
        private int _dishId;
        private string _name = string.Empty;
        private decimal _price;
        private int _portionSizeGrams;
        private int _totalQuantityGrams;
        private int _categoryId;
        private Category _category;
        private ObservableCollection<DishImage> _photos;
        private ObservableCollection<DishAllergen> _dishAllergens;
        private ObservableCollection<MenuDish> _menuDishes;

        public Dish()
        {
            _photos = new ObservableCollection<DishImage>();
            _dishAllergens = new ObservableCollection<DishAllergen>();
            _menuDishes = new ObservableCollection<MenuDish>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DishId
        {
            get => _dishId;
            set => SetProperty(ref _dishId, value);
        }

        [Required]
        [StringLength(100)]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        [Required]
        public int PortionSizeGrams
        {
            get => _portionSizeGrams;
            set => SetProperty(ref _portionSizeGrams, value);
        }

        [Required]
        public int TotalQuantityGrams
        {
            get => _totalQuantityGrams;
            set => SetProperty(ref _totalQuantityGrams, value);
        }

        [Required]
        public int CategoryId
        {
            get => _categoryId;
            set => SetProperty(ref _categoryId, value);
        }

        [ForeignKey("CategoryId")]
        public virtual Category Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public virtual ObservableCollection<DishImage> Photos
        {
            get => _photos ??= new ObservableCollection<DishImage>();
            set => SetProperty(ref _photos, value);
        }

        public virtual ObservableCollection<DishAllergen> DishAllergens
        {
            get => _dishAllergens ??= new ObservableCollection<DishAllergen>();
            set => SetProperty(ref _dishAllergens, value);
        }

        public virtual ObservableCollection<MenuDish> MenuDishes
        {
            get => _menuDishes ??= new ObservableCollection<MenuDish>();
            set => SetProperty(ref _menuDishes, value);
        }

        [NotMapped]
        public virtual IEnumerable<Allergen> Allergens => DishAllergens?.Select(da => da.Allergen).Where(a => a != null) ?? Enumerable.Empty<Allergen>();

        [NotMapped]
        public virtual IEnumerable<Menu> Menus => MenuDishes?.Select(md => md.Menu).Where(m => m != null) ?? Enumerable.Empty<Menu>();
    }
} 