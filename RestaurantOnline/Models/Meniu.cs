using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RestaurantOnline.Models
{
    public class Meniu : BaseModel
    {
        private int _idMeniu;
        private string _denumire = string.Empty;
        private int _idCategorie;
        private Categorie? _categorie;
        private ObservableCollection<MeniuPreparat>? _meniuPreparate;

        public Meniu()
        {
            _meniuPreparate = new ObservableCollection<MeniuPreparat>();
        }

        [Key]
        public int IdMeniu
        {
            get => _idMeniu;
            set => SetField(ref _idMeniu, value);
        }

        [Required]
        [MaxLength(100)]
        public string Denumire
        {
            get => _denumire;
            set => SetField(ref _denumire, value);
        }

        [Required]
        public int IdCategorie
        {
            get => _idCategorie;
            set => SetField(ref _idCategorie, value);
        }

        [ForeignKey("IdCategorie")]
        public virtual Categorie? Categorie
        {
            get => _categorie;
            set => SetField(ref _categorie, value);
        }

        public virtual ObservableCollection<MeniuPreparat>? MeniuPreparate
        {
            get => _meniuPreparate;
            set => SetField(ref _meniuPreparate, value);
        }

        [NotMapped]
        public virtual IEnumerable<Preparat> Preparate => MeniuPreparate?.Select(mp => mp.Preparat).Where(p => p != null).Cast<Preparat>() ?? Enumerable.Empty<Preparat>();
    }

    public class MeniuPreparat : BaseModel
    {
        private int _idMeniu;
        private int _idPreparate;
        private int _cantitateGrame;
        private Meniu? _meniu;
        private Preparat? _preparat;

        [Key, Column(Order = 0)]
        public int IdMeniu
        {
            get => _idMeniu;
            set => SetField(ref _idMeniu, value);
        }

        [Key, Column(Order = 1)]
        public int IdPreparate
        {
            get => _idPreparate;
            set => SetField(ref _idPreparate, value);
        }

        [Required]
        public int CantitateGrame
        {
            get => _cantitateGrame;
            set => SetField(ref _cantitateGrame, value);
        }

        [ForeignKey("IdMeniu")]
        public virtual Meniu? Meniu
        {
            get => _meniu;
            set => SetField(ref _meniu, value);
        }

        [ForeignKey("IdPreparate")]
        public virtual Preparat? Preparat
        {
            get => _preparat;
            set => SetField(ref _preparat, value);
        }
    }
} 