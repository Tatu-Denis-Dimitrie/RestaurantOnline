using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RestaurantOnline.ViewModels;

namespace RestaurantOnline.Models
{
    public class OrderDish : ViewModelBase
    {
        private int _orderId;
        private int _dishId;
        private int _quantity;
        private int? _menuId;
        private Order _order;
        private Dish _dish;

        [Key]
        [Column(Order = 0)]
        public int OrderId
        {
            get => _orderId;
            set => SetProperty(ref _orderId, value);
        }

        [Key]
        [Column(Order = 1)]
        public int DishId
        {
            get => _dishId;
            set => SetProperty(ref _dishId, value);
        }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        public int? MenuId
        {
            get => _menuId;
            set => SetProperty(ref _menuId, value);
        }

        [ForeignKey("OrderId")]
        public virtual Order Order
        {
            get => _order;
            set => SetProperty(ref _order, value);
        }

        [ForeignKey("DishId")]
        public virtual Dish Dish
        {
            get => _dish;
            set => SetProperty(ref _dish, value);
        }
    }
} 