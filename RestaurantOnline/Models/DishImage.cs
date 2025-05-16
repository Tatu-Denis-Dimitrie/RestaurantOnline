using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RestaurantOnline.ViewModels;

namespace RestaurantOnline.Models
{
    public class DishImage : ViewModelBase
    {
        private int _photoId;
        private int _dishId;
        private string _url = string.Empty;
        private Dish _dish;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PhotoId
        {
            get => _photoId;
            set => SetProperty(ref _photoId, value);
        }

        [Required]
        public int DishId
        {
            get => _dishId;
            set => SetProperty(ref _dishId, value);
        }

        [Required]
        [StringLength(255)]
        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        [ForeignKey("DishId")]
        public virtual Dish Dish
        {
            get => _dish;
            set => SetProperty(ref _dish, value);
        }
    }
} 