using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RestaurantOnline.ViewModels;

namespace RestaurantOnline.Models
{
    public class Category : ViewModelBase
    {
        private int _categoryId;
        private string _name;
        private ObservableCollection<Dish> _dishes;

        public Category()
        {
            _dishes = new ObservableCollection<Dish>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId
        {
            get => _categoryId;
            set => SetProperty(ref _categoryId, value);
        }

        [Required]
        [StringLength(100)]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public virtual ObservableCollection<Dish> Dishes
        {
            get => _dishes ??= new ObservableCollection<Dish>();
            set => SetProperty(ref _dishes, value);
        }
    }
} 