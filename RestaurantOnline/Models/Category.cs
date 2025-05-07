using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace RestaurantOnline.Models
{
    public class Category : BaseModel
    {
        private int _idCategory;
        private string _name = string.Empty;
        private ObservableCollection<Dish>? _dishes;

        public Category()
        {
            _dishes = new ObservableCollection<Dish>();
        }

        [Key]
        public int IdCategorie
        {
            get => _idCategory;
            set => SetField(ref _idCategory, value);
        }

        [Required]
        [MaxLength(100)]
        public string Nume
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        public virtual ObservableCollection<Dish>? Dishes
        {
            get => _dishes;
            set => SetField(ref _dishes, value);
        }
    }
} 