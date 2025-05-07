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
        public DbSet<Allergen> Alergens { get; set; }
        public DbSet<DishAllergens> DishAlergens { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDish> OrderDish { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<MenuDish> MenusDish { get; set; }
        public DbSet<DishImage> DishImage { get; set; }
        public DbSet<Settingse> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurare relatii si tabeluri
            
            // Configurare pentru Categoria
            modelBuilder.Entity<Category>()
                .ToTable("Categorii");
            
            // Configurare pentru Preparat
            modelBuilder.Entity<Dish>()
                .ToTable("Preparate");
            
            modelBuilder.Entity<Dish>()
                .HasOne(p => p.Categorie)
                .WithMany(c => c.Dishes)
                .HasForeignKey(p => p.IdCategorie)
                .IsRequired();
            
            // Configurare pentru FotografiePreparat
            modelBuilder.Entity<DishImage>()
                .ToTable("FotografiiPreparate");
            
            modelBuilder.Entity<DishImage>()
                .HasOne(f => f.Preparat)
                .WithMany(p => p.Fotografii)
                .HasForeignKey(f => f.IdPreparate);
            
            // Configurare pentru Alergen
            modelBuilder.Entity<Allergen>()
                .ToTable("Alergeni");
            
            // Configurare pentru PreparatAlergen (many-to-many)
            modelBuilder.Entity<DishAllergens>()
                .ToTable("PreparatAlergen");
            
            modelBuilder.Entity<DishAllergens>()
                .HasKey(pa => new { pa.IdPreparate, pa.IdAlergen });

            modelBuilder.Entity<DishAllergens>()
                .HasOne(pa => pa.Preparat)
                .WithMany(p => p.PreparatAlergeni)
                .HasForeignKey(pa => pa.IdPreparate);

            modelBuilder.Entity<DishAllergens>()
                .HasOne(pa => pa.Alergen)
                .WithMany(a => a.DishAllergens)
                .HasForeignKey(pa => pa.IdAlergen);
            
            // Configurare pentru Meniu
            modelBuilder.Entity<Menu>()
                .ToTable("Meniuri");
            
            // Configurare pentru MeniuPreparat (many-to-many)
            modelBuilder.Entity<MenuDish>()
                .ToTable("MeniuPreparat");
            
            modelBuilder.Entity<MenuDish>()
                .HasKey(mp => new { mp.IdMeniu, mp.IdPreparate });

            modelBuilder.Entity<MenuDish>()
                .HasOne(mp => mp.Meniu)
                .WithMany(m => m.MeniuPreparate)
                .HasForeignKey(mp => mp.IdMeniu);

            modelBuilder.Entity<MenuDish>()
                .HasOne(mp => mp.Preparat)
                .WithMany(p => p.MeniuPreparate)
                .HasForeignKey(mp => mp.IdPreparate);
            
            // Configurare pentru Utilizator
            modelBuilder.Entity<User>()
                .ToTable("Utilizatori");
            
            // Configurare pentru Comanda
            modelBuilder.Entity<Order>()
                .ToTable("Comenzi");
            
            modelBuilder.Entity<Order>()
                .HasOne(c => c.Utilizator)
                .WithMany(u => u.Comenzi)
                .HasForeignKey(c => c.IdUtilizator);
            
            // Configurare pentru ComandaPreparat (many-to-many)
            modelBuilder.Entity<OrderDish>()
                .ToTable("ComandaPreparat");
            
            modelBuilder.Entity<OrderDish>()
                .HasKey(cp => new { cp.IdComanda, cp.IdPreparate });

            modelBuilder.Entity<OrderDish>()
                .HasOne(cp => cp.Comanda)
                .WithMany(c => c.ComandaPreparate)
                .HasForeignKey(cp => cp.IdComanda);

            modelBuilder.Entity<OrderDish>()
                .HasOne(cp => cp.Preparat)
                .WithMany()
                .HasForeignKey(cp => cp.IdPreparate);

            // Configurare pentru Setare
            modelBuilder.Entity<Settingse>()
                .ToTable("Setari");
        }
    }
} 