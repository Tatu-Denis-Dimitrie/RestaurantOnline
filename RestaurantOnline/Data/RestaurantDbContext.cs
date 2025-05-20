using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Models;

namespace RestaurantOnline.Data
{
    public class RestaurantDbContext : DbContext
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
            : base(options)
        {
            // Setam comportamentul de tracking implicit la NoTracking pentru toate interogarile
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Allergen> Allergens { get; set; }
        public DbSet<DishAllergen> DishAllergens { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDish> OrderDishes { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<MenuDish> MenuDishes { get; set; }
        public DbSet<DishImage> DishPhotos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            // Configurare pentru Category
            modelBuilder.Entity<Category>()
                .ToTable("Categories");
            
            // Configurare pentru Dish
            modelBuilder.Entity<Dish>()
                .ToTable("Dishes");
            
            modelBuilder.Entity<Dish>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Dishes)
                .HasForeignKey(p => p.CategoryId)
                .IsRequired();
            
            // Configurare pentru DishPhoto
            modelBuilder.Entity<DishImage>()
                .ToTable("DishPhotos");
            
            modelBuilder.Entity<DishImage>()
                .HasOne(f => f.Dish)
                .WithMany(p => p.Photos)
                .HasForeignKey(f => f.DishId);
            
            // Configurare pentru Allergen
            modelBuilder.Entity<Allergen>()
                .ToTable("Allergens");
            
            // Configurare pentru DishAllergen (many-to-many)
            modelBuilder.Entity<DishAllergen>()
                .ToTable("DishAllergen");
            
            modelBuilder.Entity<DishAllergen>()
                .HasKey(pa => new { pa.DishId, pa.AllergenId });

            modelBuilder.Entity<DishAllergen>()
                .HasOne(pa => pa.Dish)
                .WithMany(p => p.DishAllergens)
                .HasForeignKey(pa => pa.DishId);

            modelBuilder.Entity<DishAllergen>()
                .HasOne(pa => pa.Allergen)
                .WithMany(a => a.DishAllergens)
                .HasForeignKey(pa => pa.AllergenId);
            
            // Configurare pentru Menu
            modelBuilder.Entity<Menu>()
                .ToTable("Menus");
            
            // Configurare pentru MenuDish (many-to-many)
            modelBuilder.Entity<MenuDish>()
                .ToTable("MenuDish");
            
            modelBuilder.Entity<MenuDish>()
                .HasKey(mp => new { mp.MenuId, mp.DishId });

            modelBuilder.Entity<MenuDish>()
                .HasOne(mp => mp.Menu)
                .WithMany(m => m.MenuDishes)
                .HasForeignKey(mp => mp.MenuId);

            modelBuilder.Entity<MenuDish>()
                .HasOne(mp => mp.Dish)
                .WithMany(p => p.MenuDishes)
                .HasForeignKey(mp => mp.DishId);
            
            // Configurare pentru User
            modelBuilder.Entity<User>()
                .ToTable("Users");
            
            // Configurare pentru Order
            modelBuilder.Entity<Order>()
                .ToTable("Orders");
            
            modelBuilder.Entity<Order>()
                .HasOne(c => c.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(c => c.UserId);
            
            // Configurare pentru OrderDish (many-to-many)
            modelBuilder.Entity<OrderDish>()
                .ToTable("OrderDish");
            
            modelBuilder.Entity<OrderDish>()
                .HasKey(cp => new { cp.OrderId, cp.DishId });

            modelBuilder.Entity<OrderDish>()
                .HasOne(cp => cp.Order)
                .WithMany(c => c.OrderDishes)
                .HasForeignKey(cp => cp.OrderId);

            modelBuilder.Entity<OrderDish>()
                .HasOne(cp => cp.Dish)
                .WithMany()
                .HasForeignKey(cp => cp.DishId);

        }
    }
} 