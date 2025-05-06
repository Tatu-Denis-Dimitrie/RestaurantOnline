using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace RestaurantOnline.Models
{
    public class Categorie : BaseModel
    {
        private int _idCategorie;
        private string _nume = string.Empty;
        private ObservableCollection<Preparat>? _preparate;
        private ObservableCollection<Meniu>? _meniuri;

        public Categorie()
        {
            _preparate = new ObservableCollection<Preparat>();
            _meniuri = new ObservableCollection<Meniu>();
        }

        [Key]
        public int IdCategorie
        {
            get => _idCategorie;
            set => SetField(ref _idCategorie, value);
        }

        [Required]
        [MaxLength(100)]
        public string Nume
        {
            get => _nume;
            set => SetField(ref _nume, value);
        }

        public virtual ObservableCollection<Preparat>? Preparate
        {
            get => _preparate;
            set => SetField(ref _preparate, value);
        }

        public virtual ObservableCollection<Meniu>? Meniuri
        {
            get => _meniuri;
            set => SetField(ref _meniuri, value);
        }
    }
} 