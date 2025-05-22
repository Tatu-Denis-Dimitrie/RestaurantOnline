namespace RestaurantOnline.Models
{
    public class AppSettings
    {
        public int StockThreshold { get; set; } = 1000; 
        public int MenuDiscountPercent { get; set; } = 0; 
    }

    public class ClientDiscountSettings
    {
        public LoyaltyDiscount LoyaltyDiscount { get; set; } = new LoyaltyDiscount();
        public OrderValueDiscount OrderValueDiscount { get; set; } = new OrderValueDiscount();
    }

    public class LoyaltyDiscount
    {
        public int MinimumOrders { get; set; } = 5;
        public int DiscountPercent { get; set; } = 10;
        public bool ApplyToTotalOnly { get; set; } = true;
    }
    
    public class OrderValueDiscount
    {
        public decimal MinimumOrderValue { get; set; } = 150;
        public int DiscountPercent { get; set; } = 10;
        public bool ApplyToTotalOnly { get; set; } = true;
    }
    
    public class DeliveryFeeSettings
    {
        public decimal StandardFee { get; set; } = 10.00m;
        public decimal IncreasedFee { get; set; } = 15.00m; 
        public decimal FreeDeliveryThreshold { get; set; } = 0;
        public decimal IncreasedFeeThreshold { get; set; } = 50.00m;
    }
} 