using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOnline.Models
{
    public class OrderDish : BaseModel
    {
        private int _idOrder;
        private int _idDish;
        private int _quantity;
        private Order? _order;
        private Dish? _dish;

        public int IdComanda
        {
            get => _idOrder;
            set => SetField(ref _idOrder, value);
        }

        public int IdPreparate
        {
            get => _idDish;
            set => SetField(ref _idDish, value);
        }

        [Required]
        public int Cantitate
        {
            get => _quantity;
            set => SetField(ref _quantity, value);
        }

        [ForeignKey("IdComanda")]
        public virtual Order? Comanda
        {
            get => _order;
            set => SetField(ref _order, value);
        }

        [ForeignKey("IdPreparate")]
        public virtual Dish? Preparat
        {
            get => _dish;
            set => SetField(ref _dish, value);
        }
    }
} 