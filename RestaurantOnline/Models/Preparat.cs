using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RestaurantOnline.Models
{
    public class Preparat : BaseModel
    {
        private int _idPreparate;
        private string _denumire = string.Empty;
        private decimal _pret;
        private int _cantitatePortieGrame;
        private int _cantitateTotalaGrame;
        private int _idCategorie;
        private Categorie? _categorie;
        private ObservableCollection<FotografiePreparat>? _fotografii;
        private ObservableCollection<PreparatAlergen>? _preparatAlergeni;
        private ObservableCollection<MeniuPreparat>? _meniuPreparate;

        public Preparat()
        {
            _fotografii = new ObservableCollection<FotografiePreparat>();
            _preparatAlergeni = new ObservableCollection<PreparatAlergen>();
            _meniuPreparate = new ObservableCollection<MeniuPreparat>();
        }

        [Key]
        public int IdPreparate
        {
            get => _idPreparate;
            set => SetField(ref _idPreparate, value);
        }

        [Required]
        [MaxLength(100)]
        public string Denumire
        {
            get => _denumire;
            set => SetField(ref _denumire, value);
        }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Pret
        {
            get => _pret;
            set => SetField(ref _pret, value);
        }

        [Required]
        public int CantitatePortieGrame
        {
            get => _cantitatePortieGrame;
            set => SetField(ref _cantitatePortieGrame, value);
        }

        [Required]
        public int CantitateTotalaGrame
        {
            get => _cantitateTotalaGrame;
            set => SetField(ref _cantitateTotalaGrame, value);
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

        public virtual ObservableCollection<FotografiePreparat>? Fotografii
        {
            get => _fotografii;
            set => SetField(ref _fotografii, value);
        }

        public virtual ObservableCollection<PreparatAlergen>? PreparatAlergeni
        {
            get => _preparatAlergeni;
            set => SetField(ref _preparatAlergeni, value);
        }

        public virtual ObservableCollection<MeniuPreparat>? MeniuPreparate
        {
            get => _meniuPreparate;
            set => SetField(ref _meniuPreparate, value);
        }

        [NotMapped]
        public virtual IEnumerable<Alergen> Alergeni => PreparatAlergeni?.Select(pa => pa.Alergen).Where(a => a != null).Cast<Alergen>() ?? Enumerable.Empty<Alergen>();

        [NotMapped]
        public virtual IEnumerable<Meniu> Meniuri => MeniuPreparate?.Select(mp => mp.Meniu).Where(m => m != null).Cast<Meniu>() ?? Enumerable.Empty<Meniu>();
    }
} 