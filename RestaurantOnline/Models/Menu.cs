using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RestaurantOnline.Models
{
    public class Menu : BaseModel
    {
        private int _idMenu;
        private string _name = string.Empty;
        private int _idCategory;
        private Category? _category;
        private ObservableCollection<MenuDish>? _menuDish;

        public Menu()
        {
            _menuDish = new ObservableCollection<MenuDish>();
        }

        [Key]
        public int IdMeniu
        {
            get => _idMenu;
            set => SetField(ref _idMenu, value);
        }

        [Required]
        [MaxLength(100)]
        public string Denumire
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        [Required]
        public int IdCategorie
        {
            get => _idCategory;
            set => SetField(ref _idCategory, value);
        }

        [ForeignKey("IdCategorie")]
        public virtual Category? Categorie
        {
            get => _category;
            set => SetField(ref _category, value);
        }

        public virtual ObservableCollection<MenuDish>? MeniuPreparate
        {
            get => _menuDish;
            set => SetField(ref _menuDish, value);
        }

        [NotMapped]
        public virtual IEnumerable<Dish> Preparate => MeniuPreparate?.Select(mp => mp.Preparat).Where(p => p != null).Cast<Dish>() ?? Enumerable.Empty<Dish>();
    }

    public class MenuDish : BaseModel
    {
        private int _idMenu;
        private int _idDish;
        private int _quantity;
        private Menu? _menu;
        private Dish? _dish;

        [Key, Column(Order = 0)]
        public int IdMeniu
        {
            get => _idMenu;
            set => SetField(ref _idMenu, value);
        }

        [Key, Column(Order = 1)]
        public int IdPreparate
        {
            get => _idDish;
            set => SetField(ref _idDish, value);
        }

        [Required]
        public int CantitateGrame
        {
            get => _quantity;
            set => SetField(ref _quantity, value);
        }

        [ForeignKey("IdMeniu")]
        public virtual Menu? Meniu
        {
            get => _menu;
            set => SetField(ref _menu, value);
        }

        [ForeignKey("IdPreparate")]
        public virtual Dish? Preparat
        {
            get => _dish;
            set => SetField(ref _dish, value);
        }
    }
} 