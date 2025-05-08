using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOnline.Models
{
    public class OrderDish : BaseModel
    {
        private int _orderId;
        private int _dishId;
        private int _quantity;
        private Order _order;
        private Dish _dish;

        [Key]
        [Column(Order = 0)]
        public int OrderId
        {
            get => _orderId;
            set => SetField(ref _orderId, value);
        }

        [Key]
        [Column(Order = 1)]
        public int DishId
        {
            get => _dishId;
            set => SetField(ref _dishId, value);
        }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity
        {
            get => _quantity;
            set => SetField(ref _quantity, value);
        }

        [ForeignKey("OrderId")]
        public virtual Order Order
        {
            get => _order;
            set => SetField(ref _order, value);
        }

        [ForeignKey("DishId")]
        public virtual Dish Dish
        {
            get => _dish;
            set => SetField(ref _dish, value);
        }
    }
} 