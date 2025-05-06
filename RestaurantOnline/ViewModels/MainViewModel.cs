using System.Collections.ObjectModel;
using System.Windows.Input;
using RestaurantOnline.Models;
using RestaurantOnline.Services;

namespace RestaurantOnline.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase? _currentViewModel;
        private readonly PreparatService _preparatService;
        private readonly CategorieService _categorieService;
        private readonly UtilizatorService _utilizatorService;
        private readonly ComandaService _comandaService;

        public MainViewModel(
            PreparatService preparatService,
            CategorieService categorieService,
            UtilizatorService utilizatorService,
            ComandaService comandaService)
        {
            _preparatService = preparatService;
            _categorieService = categorieService;
            _utilizatorService = utilizatorService;
            _comandaService = comandaService;

            // Activăm comenzile de navigare
            NavigateToPreparateCommand = new RelayCommand(_ => NavigateToPreparate());
            NavigateToUtilizatoriCommand = new RelayCommand(_ => NavigateToUtilizatori());
            NavigateToComenziCommand = new RelayCommand(_ => NavigateToComenzi());
            
            // Încărcăm automat pagina de preparate la pornire
            NavigateToPreparate();
        }

        public ViewModelBase? CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        // Comenzi de navigare activate
        public ICommand NavigateToPreparateCommand { get; }
        public ICommand NavigateToUtilizatoriCommand { get; }
        public ICommand NavigateToComenziCommand { get; }

        private void NavigateToPreparate()
        {
            CurrentViewModel = new PreparateViewModel(_preparatService, _categorieService);
        }

        private void NavigateToUtilizatori()
        {
            CurrentViewModel = new UtilizatoriViewModel(_utilizatorService);
        }

        private void NavigateToComenzi()
        {
            CurrentViewModel = new ComenziViewModel(_comandaService, _preparatService, _utilizatorService);
        }
    }
} 