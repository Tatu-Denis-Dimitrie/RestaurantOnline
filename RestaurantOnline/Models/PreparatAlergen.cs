using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOnline.Models
{
    public class PreparatAlergen : BaseModel
    {
        private int _idPreparate;
        private int _idAlergen;
        private Preparat? _preparat;
        private Alergen? _alergen;

        [Key, Column(Order = 0)]
        public int IdPreparate
        {
            get => _idPreparate;
            set => SetField(ref _idPreparate, value);
        }

        [Key, Column(Order = 1)]
        public int IdAlergen
        {
            get => _idAlergen;
            set => SetField(ref _idAlergen, value);
        }

        [ForeignKey("IdPreparate")]
        public virtual Preparat? Preparat
        {
            get => _preparat;
            set => SetField(ref _preparat, value);
        }

        [ForeignKey("IdAlergen")]
        public virtual Alergen? Alergen
        {
            get => _alergen;
            set => SetField(ref _alergen, value);
        }
    }
} 