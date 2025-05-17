using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Data;
using RestaurantOnline.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace RestaurantOnline.Services
{
    public class OrderS : RestaurantDataS<Order>
    {
        public OrderS(RestaurantDbContext context) : base(context)
        {
        }

        public override async Task<ObservableCollection<Order>> GetAllAsync()
        {
            var comenzi = await _context.Orders
                .Include(c => c.User)
                .Include(c => c.OrderDishes)
                    .ThenInclude(cp => cp.Dish)
                .ToListAsync();

            return new ObservableCollection<Order>(comenzi);
        }

        public override async Task<Order> GetByIdAsync(object id)
        {
            if (id is int idComanda)
            {
                return await GetComandaDetaliiAsync(idComanda);
            }
            return null;
        }

        public async Task<ObservableCollection<Order>> GetComenziUtilizatorAsync(int idUtilizator)
        {
            // AsNoTracking() ne asigură că Entity Framework nu va face cache la entități
            // și va încărca datele proaspete de fiecare dată
            var comenzi = await _context.Orders
                .AsNoTracking()
                .Where(c => c.UserId == idUtilizator)
                .Include(c => c.OrderDishes)
                    .ThenInclude(cp => cp.Dish)
                .ToListAsync();

            return new ObservableCollection<Order>(comenzi);
        }

        public async Task<Order> GetComandaDetaliiAsync(int idComanda)
        {
            return await _context.Orders
                .Include(c => c.User)
                .Include(c => c.OrderDishes)
                    .ThenInclude(cp => cp.Dish)
                .FirstOrDefaultAsync(c => c.OrderId == idComanda);
        }

        public async Task<bool> ActualizeazaStareComandaAsync(int idComanda, string stareNoua)
        {
            try
            {
                // Verificăm dacă starea este una validă
                string[] stariValide = { "inregistrata", "se_pregateste", "a plecat la client", "livrata", "anulata" };
                if (!stariValide.Contains(stareNoua))
                {
                    throw new ArgumentException("Starea comenzii nu este validă.");
                }

                // Verificăm statusul curent înainte de actualizare (fără tracking)
                var comandaExista = await _context.Orders
                    .AsNoTracking()
                    .AnyAsync(c => c.OrderId == idComanda);
                
                if (!comandaExista)
                {
                    throw new ArgumentException($"Comanda cu ID-ul {idComanda} nu există.");
                }

                // Apelăm procedura stocată (nu afectează tracking-ul)
                await _context.Database
                    .ExecuteSqlRawAsync("EXEC ActualizeazaStatusComanda @p0, @p1",
                                        idComanda, stareNoua);
                
                // Detașăm toate entitățile urmărite pentru a evita conflictele
                foreach (var entry in _context.ChangeTracker.Entries<Order>().ToList())
                {
                    if (entry.Entity.OrderId == idComanda)
                    {
                        entry.State = EntityState.Detached;
                    }
                }
                
                // Verificăm dacă statusul s-a actualizat cu adevărat, citind din nou comanda (fără tracking)
                var comandaDupa = await _context.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.OrderId == idComanda);
                
                // Dacă comanda există și statusul este cel dorit
                return comandaDupa != null && comandaDupa.Status == stareNoua;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eroare la actualizarea comenzii: {ex.Message}");
                throw;
            }
        }
    }
} 