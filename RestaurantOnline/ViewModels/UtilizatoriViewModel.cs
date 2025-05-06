using System.Collections.ObjectModel;
using RestaurantOnline.Models;
using RestaurantOnline.Services;

namespace RestaurantOnline.ViewModels
{
    public class UtilizatoriViewModel : ViewModelBase
    {
        private readonly UtilizatorService _utilizatorService;
        private ObservableCollection<Utilizator> _utilizatori;

        public UtilizatoriViewModel(UtilizatorService utilizatorService)
        {
            _utilizatorService = utilizatorService;
            _utilizatori = new ObservableCollection<Utilizator>();
            
            LoadDataAsync();
        }

        public ObservableCollection<Utilizator> Utilizatori
        {
            get => _utilizatori;
            set => SetProperty(ref _utilizatori, value);
        }

        private async void LoadDataAsync()
        {
            var utilizatori = await _utilizatorService.GetAllAsync();
            Utilizatori.Clear();
            
            foreach (var utilizator in utilizatori)
            {
                Utilizatori.Add(utilizator);
            }
        }
    }
} 