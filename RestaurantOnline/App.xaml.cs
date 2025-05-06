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
            // Afișăm un mesaj de eroare utilizatorului
            MessageBox.Show($"A apărut o eroare neașteptată: {e.Exception.Message}\n\nDetalii: {e.Exception.StackTrace}", 
                "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            
            // Marcăm excepția ca tratată pentru a preveni închiderea aplicației
            e.Handled = true;
        }

        private void ConfigureServices(ServiceCollection services)
        {
            try
            {
                // Configurare DbContext cu setări pentru a permite operațiuni concurente
                services.AddDbContext<RestaurantDbContext>(options =>
                {
                    options.UseSqlServer("Server=DESKTOP-4MN145N;Database=RestaurantDB;Trusted_Connection=True;TrustServerCertificate=True;");
                    
                    // Permite accesul simultan la context
                    options.EnableSensitiveDataLogging(true); // Pentru debugging
                    
                    // Configurăm context-ul pentru a nu urmări modificările entităților,
                    // ceea ce îl face mai potrivit pentru operațiuni de doar citire
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                }, ServiceLifetime.Transient); // Folosim Transient în loc de Scoped pentru a evita refolosirea aceluiași context

                // Servicii pentru entități
                services.AddTransient<IRestaurantDataService<Preparat>, PreparatService>();
                services.AddTransient<IRestaurantDataService<Categorie>, CategorieService>();
                services.AddTransient<IRestaurantDataService<Alergen>, RestaurantDataService<Alergen>>();
                services.AddTransient<IRestaurantDataService<Meniu>, RestaurantDataService<Meniu>>();
                services.AddTransient<IRestaurantDataService<Utilizator>, UtilizatorService>();
                services.AddTransient<IRestaurantDataService<Comanda>, ComandaService>();
                services.AddTransient<IRestaurantDataService<Setare>, RestaurantDataService<Setare>>();

                // Servicii specializate
                services.AddTransient<PreparatService>();
                services.AddTransient<CategorieService>();
                services.AddTransient<ComandaService>();
                services.AddTransient<UtilizatorService>();

                // ViewModels
                services.AddTransient<MainViewModel>();

                // Vizualizări
                services.AddSingleton<MainWindow>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la configurarea serviciilor: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                var mainWindow = serviceProvider.GetService<MainWindow>();
                mainWindow?.Show();
                
                if (mainWindow == null)
                {
                    MessageBox.Show("Nu s-a putut crea fereastra principală. Verificați conexiunea la baza de date.",
                        "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la pornirea aplicației: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}
