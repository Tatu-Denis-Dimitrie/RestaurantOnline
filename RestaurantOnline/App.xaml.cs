using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantOnline.Data;
using RestaurantOnline.Models;
using RestaurantOnline.Services;
using RestaurantOnline.ViewModels;

namespace RestaurantOnline
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;

        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // Afisam un mesaj de eroare utilizatorului
            MessageBox.Show($"A aparut o eroare neasteptata: {e.Exception.Message}\n\nDetalii: {e.Exception.StackTrace}", 
                "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            
            // Marcam exceptia ca tratata pentru a preveni inchiderea aplicatiei
            e.Handled = true;
        }

        private void ConfigureServices(ServiceCollection services)
        {
            try
            {
                // Configurare DbContext - schimbat in ServiceLifetime.Scoped
                services.AddDbContext<RestaurantDbContext>(options =>
                {
                    options.UseSqlServer("Server=DESKTOP-4MN145N;Database=RestaurantDB;Trusted_Connection=True;TrustServerCertificate=True;");
                    options.EnableSensitiveDataLogging(true);
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                }, ServiceLifetime.Scoped);

                // Servicii pentru entitati - schimbat din Singleton in Scoped
                services.AddScoped<IRestaurantS<Dish>, DishS>();
                services.AddScoped<IRestaurantS<Category>, CategoryS>();
                services.AddScoped<IRestaurantS<Allergen>, RestaurantDataS<Allergen>>();
                services.AddScoped<IRestaurantS<Menu>, RestaurantDataS<Menu>>();
                services.AddScoped<IRestaurantS<User>, UserS>();
                services.AddScoped<IRestaurantS<Order>, OrderS>();
                services.AddScoped<IRestaurantS<Settingse>, RestaurantDataS<Settingse>>();

                // Servicii specializate - schimbat din Singleton in Scoped
                services.AddScoped<DishS>();
                services.AddScoped<CategoryS>();
                services.AddScoped<OrderS>();
                services.AddScoped<UserS>();

                // ViewModels - cream factory pattern pentru ViewModel-uri
                services.AddTransient<MainViewModel>(provider => {
                    var preparatService = provider.GetRequiredService<DishS>();
                    var categorieService = provider.GetRequiredService<CategoryS>();
                    var utilizatorService = provider.GetRequiredService<UserS>();
                    var comandaService = provider.GetRequiredService<OrderS>();
                    return new MainViewModel(preparatService, categorieService, utilizatorService, comandaService);
                });

                // Vizualizari
                services.AddSingleton<MainWindow>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la configurarea serviciilor: {ex.Message}\n\nDetalii: {ex.StackTrace}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                
                var mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
                var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.DataContext = mainViewModel;
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la pornirea aplicatiei: {ex.Message}\n\nDetalii: {ex.StackTrace}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}
