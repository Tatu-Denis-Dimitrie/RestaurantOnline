namespace RestaurantOnline.Models
{
    public class AppSettings
    {
        public int StockThreshold { get; set; } = 1000; // Valoare implicită în caz că nu se poate citi configurarea
        public int MenuDiscountPercent { get; set; } = 0; // Procentul de reducere pentru meniuri (0 = fără reducere)
    }
} 