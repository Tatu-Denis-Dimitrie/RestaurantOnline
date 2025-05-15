using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RestaurantOnline.Models
{
    public class Menu : BaseModel
    {
        private int _menuId;
        private string _name = string.Empty;
        private int _categoryId;
        private Category _category;
        private ObservableCollection<MenuDish> _menuDishes;

        public Menu()
        {
            _menuDishes = new ObservableCollection<MenuDish>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MenuId
        {
            get => _menuId;
            set => SetField(ref _menuId, value);
        }

        [Required]
        [StringLength(100)]
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        [Required]
        public int CategoryId
        {
            get => _categoryId;
            set => SetField(ref _categoryId, value);
        }

        [ForeignKey("CategoryId")]
        public virtual Category Category
        {
            get => _category;
            set => SetField(ref _category, value);
        }

        // Relatie many-to-many cu Dish prin MenuDish
        public virtual ObservableCollection<MenuDish> MenuDishes
        {
            get => _menuDishes ??= new ObservableCollection<MenuDish>();
            set => SetField(ref _menuDishes, value);
        }

        [NotMapped]
        public virtual IEnumerable<Dish> Dishes => MenuDishes?.Select(md => md.Dish).Where(d => d != null) ?? Enumerable.Empty<Dish>();
    }

    public class MenuDish : BaseModel
    {
        private int _menuId;
        private int _dishId;
        private int _quantityGrams;
        private Menu _menu;
        private Dish _dish;

        [Key]
        [Column(Order = 0)]
        public int MenuId
        {
            get => _menuId;
            set => SetField(ref _menuId, value);
        }

        [Key]
        [Column(Order = 1)]
        public int DishId
        {
            get => _dishId;
            set => SetField(ref _dishId, value);
        }

        [Required]
        public int QuantityGrams
        {
            get => _quantityGrams;
            set => SetField(ref _quantityGrams, value);
        }

        [ForeignKey("MenuId")]
        public virtual Menu Menu
        {
            get => _menu;
            set => SetField(ref _menu, value);
        }

        [ForeignKey("DishId")]
        public virtual Dish Dish
        {
            get => _dish;
            set => SetField(ref _dish, value);
        }
    }
} 