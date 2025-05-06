using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RestaurantOnline.Models
{
    public enum StareComanda
    {
        inregistrata,
        se_pregateste,
        a_plecat_la_client,
        livrata,
        anulata
    }

    public class Comanda : BaseModel
    {
        private Guid _idComanda;
        private int _idUtilizator;
        private DateTime _dataComanda = DateTime.Now;
        private StareComanda _stare = StareComanda.inregistrata;
        private decimal _valoareFinala;
        private decimal _transport;
        private Utilizator? _utilizator;
        private ObservableCollection<ComandaPreparat>? _comandaPreparate;

        public Comanda()
        {
            _idComanda = Guid.NewGuid();
            _comandaPreparate = new ObservableCollection<ComandaPreparat>();
        }

        [Key]
        public Guid IdComanda
        {
            get => _idComanda;
            set => SetField(ref _idComanda, value);
        }

        [Required]
        public int IdUtilizator
        {
            get => _idUtilizator;
            set => SetField(ref _idUtilizator, value);
        }

        [Required]
        public DateTime DataComanda
        {
            get => _dataComanda;
            set => SetField(ref _dataComanda, value);
        }

        [Required]
        [MaxLength(20)]
        public StareComanda Stare
        {
            get => _stare;
            set => SetField(ref _stare, value);
        }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ValoareFinala
        {
            get => _valoareFinala;
            set => SetField(ref _valoareFinala, value);
        }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Transport
        {
            get => _transport;
            set => SetField(ref _transport, value);
        }

        [ForeignKey("IdUtilizator")]
        public virtual Utilizator? Utilizator
        {
            get => _utilizator;
            set => SetField(ref _utilizator, value);
        }

        public virtual ObservableCollection<ComandaPreparat>? ComandaPreparate
        {
            get => _comandaPreparate;
            set => SetField(ref _comandaPreparate, value);
        }

        [NotMapped]
        public virtual IEnumerable<Preparat> Preparate => ComandaPreparate?.Select(cp => cp.Preparat).Where(p => p != null).Cast<Preparat>() ?? Enumerable.Empty<Preparat>();
    }

    public class ComandaPreparat : BaseModel
    {
        private Guid _idComanda;
        private int _idPreparate;
        private int _cantitate;
        private Comanda? _comanda;
        private Preparat? _preparat;

        [Key, Column(Order = 0)]
        public Guid IdComanda
        {
            get => _idComanda;
            set => SetField(ref _idComanda, value);
        }

        [Key, Column(Order = 1)]
        public int IdPreparate
        {
            get => _idPreparate;
            set => SetField(ref _idPreparate, value);
        }

        [Required]
        public int Cantitate
        {
            get => _cantitate;
            set => SetField(ref _cantitate, value);
        }

        [ForeignKey("IdComanda")]
        public virtual Comanda? Comanda
        {
            get => _comanda;
            set => SetField(ref _comanda, value);
        }

        [ForeignKey("IdPreparate")]
        public virtual Preparat? Preparat
        {
            get => _preparat;
            set => SetField(ref _preparat, value);
        }
    }
} 