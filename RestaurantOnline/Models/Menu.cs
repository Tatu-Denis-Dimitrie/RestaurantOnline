using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using RestaurantOnline.ViewModels;

namespace RestaurantOnline.Models
{
    public class Menu : ViewModelBase
    {
        private int _menuId;
        private string _name = string.Empty;
        private int _categoryId;
        private Category _category;
        private ObservableCollection<MenuDish> _menuDishes;
        private decimal _discountPercent = 0;

        public Menu()
        {
            _menuDishes = new ObservableCollection<MenuDish>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MenuId
        {
            get => _menuId;
            set => SetProperty(ref _menuId, value);
        }

        [Required]
        [StringLength(100)]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
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

        public virtual ObservableCollection<MenuDish> MenuDishes
        {
            get => _menuDishes ??= new ObservableCollection<MenuDish>();
            set => SetProperty(ref _menuDishes, value);
        }

        [NotMapped]
        public decimal DiscountPercent
        {
            get => _discountPercent;
            set => SetProperty(ref _discountPercent, value);
        }

        [NotMapped]
        public virtual IEnumerable<Dish> Dishes => MenuDishes?.Select(md => md.Dish).Where(d => d != null) ?? Enumerable.Empty<Dish>();

        [NotMapped]
        public decimal TotalPrice => MenuDishes?.Sum(md => md.Dish?.Price ?? 0) ?? 0;
        
        [NotMapped]
        public decimal DiscountedPrice => TotalPrice * (1 - DiscountPercent / 100);
        
        [NotMapped]
        public decimal DiscountAmount => TotalPrice - DiscountedPrice;
        
        [NotMapped]
        public bool HasDiscount => DiscountPercent > 0;
    }

    public class MenuDish : ViewModelBase
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
            set => SetProperty(ref _menuId, value);
        }

        [Key]
        [Column(Order = 1)]
        public int DishId
        {
            get => _dishId;
            set => SetProperty(ref _dishId, value);
        }

        [Required]
        public int QuantityGrams
        {
            get => _quantityGrams;
            set => SetProperty(ref _quantityGrams, value);
        }

        [ForeignKey("MenuId")]
        public virtual Menu Menu
        {
            get => _menu;
            set => SetProperty(ref _menu, value);
        }

        [ForeignKey("DishId")]
        public virtual Dish Dish
        {
            get => _dish;
            set => SetProperty(ref _dish, value);
        }
    }
} 