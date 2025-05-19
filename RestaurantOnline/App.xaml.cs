using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private ServiceProvider _serviceProvider;
        private IConfiguration _configuration;
        public AppSettings AppSettings { get; private set; }

        public ServiceProvider ServiceProvider => _serviceProvider;
        public IConfiguration Configuration => _configuration;

        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            
            // Încărcăm configurația
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _configuration = builder.Build();
            
            // Încărcăm setările
            AppSettings = new AppSettings();
            var stockThreshold = _configuration["AppSettings:StockThreshold"];
            if (!string.IsNullOrEmpty(stockThreshold) && int.TryParse(stockThreshold, out int threshold))
            {
                AppSettings.StockThreshold = threshold;
            }
            
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
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
                // Adăugăm configurația ca serviciu
                services.AddSingleton<IConfiguration>(_configuration);
                services.AddSingleton(AppSettings);
                
                // Configurare DbContext - schimbat in ServiceLifetime.Scoped
                services.AddDbContext<RestaurantDbContext>(options =>
                {
                    options.UseSqlServer("Server=DESKTOP-4MN145N;Database=RestaurantDB2;Trusted_Connection=True;TrustServerCertificate=True;");
                    options.EnableSensitiveDataLogging(true);
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                }, ServiceLifetime.Scoped);

                // Servicii pentru entitati - schimbat din Singleton in Scoped
                services.AddScoped<IRestaurantS<Dish>, DishS>();
                services.AddScoped<IRestaurantS<Category>, CategoryS>();
                services.AddScoped<IRestaurantS<Allergen>, AllergenS>();
                services.AddScoped<IRestaurantS<Menu>, MenuService>();
                services.AddScoped<IRestaurantS<User>, UserS>();
                services.AddScoped<IRestaurantS<Order>, OrderS>();
                services.AddScoped<IRestaurantS<Setting>, RestaurantDataS<Setting>>();

                // Servicii specializate - schimbat din Singleton in Scoped
                services.AddScoped<DishS>();
                services.AddScoped<CategoryS>();
                services.AddScoped<OrderS>();
                services.AddScoped<UserS>();
                services.AddScoped<AllergenS>();
                services.AddScoped<MenuService>();

                // ViewModels - cream factory pattern pentru ViewModel-uri
                services.AddTransient<MainViewModel>(provider => {
                    var preparatService = provider.GetRequiredService<DishS>();
                    var categorieService = provider.GetRequiredService<CategoryS>();
                    var utilizatorService = provider.GetRequiredService<UserS>();
                    var comandaService = provider.GetRequiredService<OrderS>();
                    var allergenService = provider.GetRequiredService<AllergenS>();
                    return new MainViewModel(preparatService, categorieService, utilizatorService, comandaService, allergenService);
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
                
                var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
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
