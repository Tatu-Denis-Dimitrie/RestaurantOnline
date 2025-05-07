using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RestaurantOnline.Models
{
    public enum OrderStatus
    {
        inregistrata,
        se_pregateste,
        a_plecat_la_client,
        livrata,
        anulata
    }

    public class Order : BaseModel
    {
        private int _idOrder;
        private int _idUser;
        private string _state = string.Empty;
        private decimal _finalValue;
        private decimal _deliveryFee;
        private DateTime _orderDate = DateTime.Now;
        private User? _user;
        private ObservableCollection<OrderDish>? _orderDish;

        public Order()
        {
            _orderDish = new ObservableCollection<OrderDish>();
        }

        [Key]
        public int IdComanda
        {
            get => _idOrder;
            set => SetField(ref _idOrder, value);
        }

        public int IdUtilizator
        {
            get => _idUser;
            set => SetField(ref _idUser, value);
        }

        public string Stare
        {
            get => _state;
            set => SetField(ref _state, value);
        }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ValoareFinala
        {
            get => _finalValue;
            set => SetField(ref _finalValue, value);
        }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Transport
        {
            get => _deliveryFee;
            set => SetField(ref _deliveryFee, value);
        }

        public DateTime DataComanda
        {
            get => _orderDate;
            set => SetField(ref _orderDate, value);
        }

        [ForeignKey("IdUtilizator")]
        public virtual User? Utilizator
        {
            get => _user;
            set => SetField(ref _user, value);
        }

        public virtual ObservableCollection<OrderDish>? ComandaPreparate
        {
            get => _orderDish;
            set => SetField(ref _orderDish, value);
        }
    }
} 