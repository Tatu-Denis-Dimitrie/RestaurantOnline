using System.Collections.ObjectModel;
using System.Windows.Input;
using RestaurantOnline.Models;
using RestaurantOnline.Services;

namespace RestaurantOnline.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase? _currentViewModel;
        private readonly DishS _preparatService;
        private readonly CategoryS _categorieService;
        private readonly UserS _utilizatorService;
        private readonly OrderS _comandaService;
        private User? _utilizatorCurent;

        public MainViewModel(
            DishS preparatService,
            CategoryS categorieService,
            UserS utilizatorService,
            OrderS comandaService)
        {
            _preparatService = preparatService;
            _categorieService = categorieService;
            _utilizatorService = utilizatorService;
            _comandaService = comandaService;

            // Activam comenzile de navigare
            NavigateToPreparateCommand = new RelayCommand(_ => NavigateToPreparate());
            NavigateToUtilizatoriCommand = new RelayCommand(_ => NavigateToUtilizatori());
            NavigateToComenziCommand = new RelayCommand(_ => NavigateToComenzi());
            NavigateToAutentificareCommand = new RelayCommand(_ => NavigateToAutentificare());
            
            // incarcam automat pagina de preparate la pornire
            NavigateToPreparate();
        }

        public ViewModelBase? CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public User? UtilizatorCurent
        {
            get => _utilizatorCurent;
            set => SetProperty(ref _utilizatorCurent, value);
        }

        // Comenzi de navigare activate
        public ICommand NavigateToPreparateCommand { get; }
        public ICommand NavigateToUtilizatoriCommand { get; }
        public ICommand NavigateToComenziCommand { get; }
        public ICommand NavigateToAutentificareCommand { get; }

        private void NavigateToPreparate()
        {
            CurrentViewModel = new DishViewModel(_preparatService, _categorieService);
        }

        private void NavigateToUtilizatori()
        {
            CurrentViewModel = new UtilizatoriViewModel(_utilizatorService);
        }

        private void NavigateToComenzi()
        {
            CurrentViewModel = new ComenziViewModel(_comandaService, _preparatService, _utilizatorService);
        }

        private void NavigateToAutentificare()
        {
            CurrentViewModel = new AuthVM(_utilizatorService, this);
        }

        public void NavigateToHome()
        {
            NavigateToPreparate();
        }
    }
} 