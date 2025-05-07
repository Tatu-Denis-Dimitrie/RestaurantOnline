using System.Collections.ObjectModel;
using RestaurantOnline.Models;
using RestaurantOnline.Services;

namespace RestaurantOnline.ViewModels
{
    public class UtilizatoriViewModel : ViewModelBase
    {
        private readonly UserS _utilizatorService;
        private ObservableCollection<User> _utilizatori;

        public UtilizatoriViewModel(UserS utilizatorService)
        {
            _utilizatorService = utilizatorService;
            _utilizatori = new ObservableCollection<User>();
            
            LoadDataAsync();
        }

        public ObservableCollection<User> Utilizatori
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