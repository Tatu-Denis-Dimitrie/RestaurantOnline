using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOnline.Models
{
    public class Order : BaseModel
    {
        private int _orderId;
        private int _userId;
        private DateTime _orderDate;
        private string _status = "inregistrata";
        private decimal _finalAmount;
        private decimal _deliveryFee;
        private User _user;
        private ObservableCollection<OrderDish> _orderDishes;

        public Order()
        {
            _orderDate = DateTime.Now;
            _orderDishes = new ObservableCollection<OrderDish>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId
        {
            get => _orderId;
            set => SetField(ref _orderId, value);
        }

        [Required]
        public int UserId
        {
            get => _userId;
            set => SetField(ref _userId, value);
        }

        [Required]
        public DateTime OrderDate
        {
            get => _orderDate;
            set => SetField(ref _orderDate, value);
        }

        [Required]
        [StringLength(20)]
        public string Status
        {
            get => _status;
            set => SetField(ref _status, value);
        }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal FinalAmount
        {
            get => _finalAmount;
            set => SetField(ref _finalAmount, value);
        }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal DeliveryFee
        {
            get => _deliveryFee;
            set => SetField(ref _deliveryFee, value);
        }

        [ForeignKey("UserId")]
        public virtual User User
        {
            get => _user;
            set => SetField(ref _user, value);
        }

        public virtual ObservableCollection<OrderDish> OrderDishes
        {
            get => _orderDishes ??= new ObservableCollection<OrderDish>();
            set => SetField(ref _orderDishes, value);
        }
    }
} 