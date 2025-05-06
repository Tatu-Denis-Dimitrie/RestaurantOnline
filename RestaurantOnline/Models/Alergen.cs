using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RestaurantOnline.Models
{
    public class Alergen : BaseModel
    {
        private int _idAlergen;
        private string _nume = string.Empty;
        private ObservableCollection<PreparatAlergen>? _preparatAlergeni;

        public Alergen()
        {
            _preparatAlergeni = new ObservableCollection<PreparatAlergen>();
        }

        [Key]
        public int IdAlergen
        {
            get => _idAlergen;
            set => SetField(ref _idAlergen, value);
        }

        [Required]
        [MaxLength(100)]
        public string Nume
        {
            get => _nume;
            set => SetField(ref _nume, value);
        }

        public virtual ObservableCollection<PreparatAlergen>? PreparatAlergeni
        {
            get => _preparatAlergeni;
            set => SetField(ref _preparatAlergeni, value);
        }

        [NotMapped]
        public virtual IEnumerable<Preparat> Preparate => PreparatAlergeni?.Select(pa => pa.Preparat).Where(p => p != null).Cast<Preparat>() ?? Enumerable.Empty<Preparat>();
    }
} 