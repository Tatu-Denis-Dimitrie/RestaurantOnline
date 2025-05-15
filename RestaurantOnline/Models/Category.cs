using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOnline.Models
{
    public class Category : BaseModel
    {
        private int _categoryId;
        private string _name;
        private ObservableCollection<Dish> _dishes;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId
        {
            get => _categoryId;
            set => SetField(ref _categoryId, value);
        }

        [Required]
        [StringLength(100)]
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        // Relatie one-to-many cu Dishes
        public virtual ObservableCollection<Dish> Dishes
        {
            get => _dishes ??= new ObservableCollection<Dish>();
            set => SetField(ref _dishes, value);
        }

        public Category()
        {
            _dishes = new ObservableCollection<Dish>();
        }
    }
} 