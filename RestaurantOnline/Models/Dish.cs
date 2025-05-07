using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RestaurantOnline.Models
{
    public class Dish : BaseModel
    {
        private int _idDish;
        private string _name = string.Empty;
        private decimal _price;
        private int _gramQuantityPortion;
        private int _totalQuantity;
        private int _idCategry;
        private Category? _category;
        private ObservableCollection<DishImage>? _image;
        private ObservableCollection<DishAllergens>? _dishAllergens;
        private ObservableCollection<MenuDish>? _menuDish;

        public Dish()
        {
            _image = new ObservableCollection<DishImage>();
            _dishAllergens = new ObservableCollection<DishAllergens>();
            _menuDish = new ObservableCollection<MenuDish>();
        }

        [Key]
        public int IdPreparate
        {
            get => _idDish;
            set => SetField(ref _idDish, value);
        }

        [Required]
        [MaxLength(100)]
        public string Denumire
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Pret
        {
            get => _price;
            set => SetField(ref _price, value);
        }

        [Required]
        public int CantitatePortieGrame
        {
            get => _gramQuantityPortion;
            set => SetField(ref _gramQuantityPortion, value);
        }

        [Required]
        public int CantitateTotalaGrame
        {
            get => _totalQuantity;
            set => SetField(ref _totalQuantity, value);
        }

        [Required]
        public int IdCategorie
        {
            get => _idCategry;
            set => SetField(ref _idCategry, value);
        }

        [ForeignKey("IdCategorie")]
        public virtual Category? Categorie
        {
            get => _category;
            set => SetField(ref _category, value);
        }

        public virtual ObservableCollection<DishImage>? Fotografii
        {
            get => _image;
            set => SetField(ref _image, value);
        }

        public virtual ObservableCollection<DishAllergens>? PreparatAlergeni
        {
            get => _dishAllergens;
            set => SetField(ref _dishAllergens, value);
        }

        public virtual ObservableCollection<MenuDish>? MeniuPreparate
        {
            get => _menuDish;
            set => SetField(ref _menuDish, value);
        }

        [NotMapped]
        public virtual IEnumerable<Allergen> Alergeni => PreparatAlergeni?.Select(pa => pa.Alergen).Where(a => a != null).Cast<Allergen>() ?? Enumerable.Empty<Allergen>();

        [NotMapped]
        public virtual IEnumerable<Menu> Meniuri => MeniuPreparate?.Select(mp => mp.Meniu).Where(m => m != null).Cast<Menu>() ?? Enumerable.Empty<Menu>();
    }
} 