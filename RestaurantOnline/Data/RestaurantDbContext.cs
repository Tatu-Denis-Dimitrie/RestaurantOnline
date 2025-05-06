using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Models;

namespace RestaurantOnline.Data
{
    public class RestaurantDbContext : DbContext
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
            : base(options)
        {
        }

        public DbSet<Categorie> Categorii { get; set; }
        public DbSet<Alergen> Alergeni { get; set; }
        public DbSet<Preparat> Preparate { get; set; }
        public DbSet<FotografiePreparat> FotografiiPreparate { get; set; }
        public DbSet<PreparatAlergen> PreparatAlergeni { get; set; }
        public DbSet<Meniu> Meniuri { get; set; }
        public DbSet<MeniuPreparat> MeniuPreparate { get; set; }
        public DbSet<Utilizator> Utilizatori { get; set; }
        public DbSet<Comanda> Comenzi { get; set; }
        public DbSet<ComandaPreparat> ComandaPreparate { get; set; }
        public DbSet<Setare> Setari { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurare pentru chei primare compuse
            modelBuilder.Entity<PreparatAlergen>()
                .HasKey(pa => new { pa.IdPreparate, pa.IdAlergen });

            modelBuilder.Entity<MeniuPreparat>()
                .HasKey(mp => new { mp.IdMeniu, mp.IdPreparate });

            modelBuilder.Entity<ComandaPreparat>()
                .HasKey(cp => new { cp.IdComanda, cp.IdPreparate });

            // Configurare relații many-to-many
            modelBuilder.Entity<PreparatAlergen>()
                .HasOne(pa => pa.Preparat)
                .WithMany(p => p.PreparatAlergeni)
                .HasForeignKey(pa => pa.IdPreparate)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PreparatAlergen>()
                .HasOne(pa => pa.Alergen)
                .WithMany(a => a.PreparatAlergeni)
                .HasForeignKey(pa => pa.IdAlergen)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MeniuPreparat>()
                .HasOne(mp => mp.Meniu)
                .WithMany(m => m.MeniuPreparate)
                .HasForeignKey(mp => mp.IdMeniu)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MeniuPreparat>()
                .HasOne(mp => mp.Preparat)
                .WithMany(p => p.MeniuPreparate)
                .HasForeignKey(mp => mp.IdPreparate)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ComandaPreparat>()
                .HasOne(cp => cp.Comanda)
                .WithMany(c => c.ComandaPreparate)
                .HasForeignKey(cp => cp.IdComanda)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ComandaPreparat>()
                .HasOne(cp => cp.Preparat)
                .WithMany()
                .HasForeignKey(cp => cp.IdPreparate)
                .OnDelete(DeleteBehavior.Restrict);

            // Alte relații
            modelBuilder.Entity<Preparat>()
                .HasOne(p => p.Categorie)
                .WithMany(c => c.Preparate)
                .HasForeignKey(p => p.IdCategorie)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Meniu>()
                .HasOne(m => m.Categorie)
                .WithMany(c => c.Meniuri)
                .HasForeignKey(m => m.IdCategorie)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FotografiePreparat>()
                .HasOne(f => f.Preparat)
                .WithMany(p => p.Fotografii)
                .HasForeignKey(f => f.IdPreparate)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comanda>()
                .HasOne(c => c.Utilizator)
                .WithMany(u => u.Comenzi)
                .HasForeignKey(c => c.IdUtilizator)
                .OnDelete(DeleteBehavior.Restrict);

            // Convertirea enum-ului StareComanda la string
            modelBuilder.Entity<Comanda>()
                .Property(c => c.Stare)
                .HasConversion<string>();

            // Configurarea preciziei pentru valorile monetare
            modelBuilder.Entity<Preparat>()
                .Property(p => p.Pret)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Comanda>()
                .Property(c => c.ValoareFinala)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Comanda>()
                .Property(c => c.Transport)
                .HasColumnType("decimal(10,2)");

            // Email unic pentru utilizatori
            modelBuilder.Entity<Utilizator>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Nume unic pentru setări
            modelBuilder.Entity<Setare>()
                .HasIndex(s => s.Nume)
                .IsUnique();

            // Configurare tabel pentru entități
            modelBuilder.Entity<Categorie>().ToTable("Categorii");
            modelBuilder.Entity<Alergen>().ToTable("Alergeni");
            modelBuilder.Entity<Preparat>().ToTable("Preparate");
            modelBuilder.Entity<FotografiePreparat>().ToTable("FotografiiPreparate");
            modelBuilder.Entity<PreparatAlergen>().ToTable("PreparatAlergen");
            modelBuilder.Entity<Meniu>().ToTable("Meniuri");
            modelBuilder.Entity<MeniuPreparat>().ToTable("MeniuPreparat");
            modelBuilder.Entity<Utilizator>().ToTable("Utilizatori");
            modelBuilder.Entity<Comanda>().ToTable("Comenzi");
            modelBuilder.Entity<ComandaPreparat>().ToTable("ComandaPreparat");
            modelBuilder.Entity<Setare>().ToTable("Setari");
        }
    }
} 