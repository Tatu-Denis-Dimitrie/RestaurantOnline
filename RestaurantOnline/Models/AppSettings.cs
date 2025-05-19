namespace RestaurantOnline.Models
{
    public class AppSettings
    {
        public int StockThreshold { get; set; } = 1000; // Valoare implicită în caz că nu se poate citi configurarea
    }
} 