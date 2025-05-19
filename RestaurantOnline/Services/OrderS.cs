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
            // Folosim AsNoTracking pentru a evita probleme de tracking
            var comenzi = await _context.Orders
                .AsNoTracking()
                .Include(c => c.User)
                .ToListAsync();

            // Pentru fiecare comandă, încărcăm manual OrderDishes
            foreach (var comanda in comenzi)
            {
                var orderDishes = await _context.OrderDishes
                    .AsNoTracking()
                    .Where(od => od.OrderId == comanda.OrderId)
                    .ToListAsync();

                // Încărcăm detaliile pentru fiecare dish
                foreach (var orderDish in orderDishes)
                {
                    var dish = await _context.Dishes
                        .AsNoTracking()
                        .Include(d => d.Category)
                        .FirstOrDefaultAsync(d => d.DishId == orderDish.DishId);
                        
                    orderDish.Dish = dish;
                }

                // Adăugăm OrderDishes la comandă
                comanda.OrderDishes = new ObservableCollection<OrderDish>(orderDishes);
            }

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
            var comenzi = await _context.Orders
                .AsNoTracking()
                .Where(c => c.UserId == idUtilizator)
                .ToListAsync();

            // Pentru fiecare comandă, încărcăm manual OrderDishes
            foreach (var comanda in comenzi)
            {
                var orderDishes = await _context.OrderDishes
                    .AsNoTracking()
                    .Where(od => od.OrderId == comanda.OrderId)
                    .ToListAsync();

                // Încărcăm detaliile pentru fiecare dish
                foreach (var orderDish in orderDishes)
                {
                    var dish = await _context.Dishes
                        .AsNoTracking()
                        .Include(d => d.Category)
                        .FirstOrDefaultAsync(d => d.DishId == orderDish.DishId);
                        
                    orderDish.Dish = dish;
                }

                // Adăugăm OrderDishes la comandă
                comanda.OrderDishes = new ObservableCollection<OrderDish>(orderDishes);
            }

            return new ObservableCollection<Order>(comenzi);
        }

        public async Task<Order> GetComandaDetaliiAsync(int idComanda)
        {
            // Folosim AsNoTracking pentru a evita probleme de tracking și SQL direct pentru
            // a încărca OrderDishes fără a fi afectați de problemele de relații ale EF
            var comanda = await _context.Orders
                .AsNoTracking()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.OrderId == idComanda);
                
            if (comanda != null)
            {
                // Încărcăm manual OrderDishes pentru a ne asigura că toate sunt incluse
                var orderDishes = await _context.OrderDishes
                    .AsNoTracking()
                    .Where(od => od.OrderId == idComanda)
                    .ToListAsync();
                    
                // Încărcăm detaliile pentru fiecare dish
                foreach (var orderDish in orderDishes)
                {
                    var dish = await _context.Dishes
                        .AsNoTracking()
                        .Include(d => d.Category)
                        .Include(d => d.Photos)
                        .FirstOrDefaultAsync(d => d.DishId == orderDish.DishId);
                        
                    orderDish.Dish = dish;
                }
                
                // Adăugăm OrderDishes la comandă
                comanda.OrderDishes = new ObservableCollection<OrderDish>(orderDishes);
            }
            
            return comanda;
        }

        public async Task<bool> ActualizeazaStareComandaAsync(int idComanda, string stareNoua)
        {
            try
            {
                // Folosim procedura stocată pentru toate actualizările de status
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC UpdateOrderStatus @OrderId, @NewStatus",
                    new Microsoft.Data.SqlClient.SqlParameter("@OrderId", idComanda),
                    new Microsoft.Data.SqlClient.SqlParameter("@NewStatus", stareNoua));
                return true;
            }
            catch (Exception ex)
            {
                // Logging-ul excepției
                System.Diagnostics.Debug.WriteLine($"Eroare la actualizarea stării comenzii: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                // Re-aruncăm excepția pentru a fi gestionată de nivelul superior
                throw;
            }
        }
        
        public async Task<OrderDish> AddOrderDishAsync(OrderDish orderDish)
        {
            try
            {
                // Vom folosi contextul existent, dar în mod diferit
                // Dezactivăm detectarea schimbărilor pentru performanță
                _context.ChangeTracker.AutoDetectChangesEnabled = false;
                // Curățăm tracking-ul
                _context.ChangeTracker.Clear();
                
                // Creăm un nou OrderDish detașat de alte tracked entities
                var newOrderDish = new OrderDish
                {
                    OrderId = orderDish.OrderId,
                    DishId = orderDish.DishId,
                    Quantity = orderDish.Quantity,
                    MenuId = orderDish.MenuId
                };
                
                // Adăugăm entitatea ca nouă
                await _context.OrderDishes.AddAsync(newOrderDish);
                
                // Salvăm schimbările
                await _context.SaveChangesAsync();
                
                // Reactivăm detectarea automată a schimbărilor
                _context.ChangeTracker.AutoDetectChangesEnabled = true;
                
                return orderDish;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eroare la adăugarea OrderDish: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }
    }
} 