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
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _configuration = builder.Build();
            
            AppSettings = new AppSettings();
            var stockThreshold = _configuration["AppSettings:StockThreshold"];
            if (!string.IsNullOrEmpty(stockThreshold) && int.TryParse(stockThreshold, out int threshold))
            {
                AppSettings.StockThreshold = threshold;
            }
            
            var menuDiscountPercent = _configuration["AppSettings:MenuDiscountPercent"];
            if (!string.IsNullOrEmpty(menuDiscountPercent) && int.TryParse(menuDiscountPercent, out int discount))
            {
                AppSettings.MenuDiscountPercent = discount;
            }
            
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"A aparut o eroare neasteptata: {e.Exception.Message}\n\nDetalii: {e.Exception.StackTrace}", 
                "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            
            e.Handled = true;
        }

        private void ConfigureServices(ServiceCollection services)
        {
            try
            {
                services.AddSingleton<IConfiguration>(_configuration);
                services.AddSingleton(AppSettings);
                
                services.AddDbContext<RestaurantDbContext>(options =>
                {
                    options.UseSqlServer("Server=DESKTOP-4MN145N;Database=RestaurantDB2;Trusted_Connection=True;TrustServerCertificate=True;");
                    options.EnableSensitiveDataLogging(true);
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                }, ServiceLifetime.Scoped);

                services.AddScoped<IRestaurantS<Dish>, DishS>();
                services.AddScoped<IRestaurantS<Category>, CategoryS>();
                services.AddScoped<IRestaurantS<Allergen>, AllergenS>();
                services.AddScoped<IRestaurantS<Menu>, MenuService>();
                services.AddScoped<IRestaurantS<User>, UserS>();
                services.AddScoped<IRestaurantS<Order>, OrderS>();

                services.AddScoped<DishS>();
                services.AddScoped<CategoryS>();
                services.AddScoped<OrderS>(provider => {
                    var dbContext = provider.GetRequiredService<RestaurantDbContext>();
                    var configuration = provider.GetRequiredService<IConfiguration>();
                    return new OrderS(dbContext, configuration);
                });
                services.AddScoped<UserS>();
                services.AddScoped<AllergenS>();
                services.AddScoped<MenuService>();

                services.AddTransient<MainViewModel>(provider => {
                    var preparatService = provider.GetRequiredService<DishS>();
                    var categorieService = provider.GetRequiredService<CategoryS>();
                    var utilizatorService = provider.GetRequiredService<UserS>();
                    var comandaService = provider.GetRequiredService<OrderS>();
                    var allergenService = provider.GetRequiredService<AllergenS>();
                    return new MainViewModel(preparatService, categorieService, utilizatorService, comandaService, allergenService);
                });

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
