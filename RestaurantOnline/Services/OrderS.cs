using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Data;
using RestaurantOnline.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace RestaurantOnline.Services
{
    public class OrderS : RestaurantDataS<Order>
    {
        private readonly int _minimumOrdersForDiscount = 5;
        private readonly int _discountPercent = 10;
        private readonly bool _applyToTotalOnly = true;

        public OrderS(RestaurantDbContext context, IConfiguration configuration) : base(context)
        {
            // Încărcăm setările pentru reducere din fișierul appsettings.json
            try
            {
                var clientDiscountsSection = configuration.GetSection("ClientDiscounts");
                var loyaltySection = clientDiscountsSection.GetSection("LoyaltyDiscount");
                
                // Încercăm să citim valorile direct
                string minimumOrdersStr = loyaltySection["MinimumOrders"];
                string discountPercentStr = loyaltySection["DiscountPercent"];
                string applyToTotalOnlyStr = loyaltySection["ApplyToTotalOnly"];
                
                if (!string.IsNullOrEmpty(minimumOrdersStr) && int.TryParse(minimumOrdersStr, out int minimumOrders))
                {
                    _minimumOrdersForDiscount = minimumOrders;
                }
                
                if (!string.IsNullOrEmpty(discountPercentStr) && int.TryParse(discountPercentStr, out int discountPercent))
                {
                    _discountPercent = discountPercent;
                }
                
                if (!string.IsNullOrEmpty(applyToTotalOnlyStr) && bool.TryParse(applyToTotalOnlyStr, out bool applyToTotalOnly))
                {
                    _applyToTotalOnly = applyToTotalOnly;
                }
                
                System.Diagnostics.Debug.WriteLine($"Setări reducere client: Minim comenzi: {_minimumOrdersForDiscount}, Procent: {_discountPercent}%, Doar pe total: {_applyToTotalOnly}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eroare la încărcarea setărilor de reducere: {ex.Message}");
                // Folosim valorile implicite definite mai sus
            }
        }

        public override async Task<ObservableCollection<Order>> GetAllAsync()
        {
            var comenzi = await _context.Orders
                .AsNoTracking()
                .Include(c => c.User)
                .ToListAsync();

            foreach (var comanda in comenzi)
            {
                var orderDishes = await _context.OrderDishes
                    .AsNoTracking()
                    .Where(od => od.OrderId == comanda.OrderId)
                    .ToListAsync();

                foreach (var orderDish in orderDishes)
                {
                    var dish = await _context.Dishes
                        .AsNoTracking()
                        .Include(d => d.Category)
                        .FirstOrDefaultAsync(d => d.DishId == orderDish.DishId);
                        
                    orderDish.Dish = dish;
                }

                comanda.OrderDishes = new ObservableCollection<OrderDish>(orderDishes);
                
                // Aplicăm reducerea pentru clienții fideli
                await CalculateFinalAmountWithDiscountsAsync(comanda);
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
            var comenzi = await _context.Orders
                .AsNoTracking()
                .Where(c => c.UserId == idUtilizator)
                .ToListAsync();

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
                
                // Încărcăm și User pentru a putea aplica reducerea
                comanda.User = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == idUtilizator);
                
                // Aplicăm reducerea pentru clienții fideli
                await CalculateFinalAmountWithDiscountsAsync(comanda);
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
                
                // Aplicăm reducerea pentru clienții fideli
                await CalculateFinalAmountWithDiscountsAsync(comanda);
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

        public async Task<Order> CalculateFinalAmountWithDiscountsAsync(Order order)
        {
            // Verificăm dacă utilizatorul este eligibil pentru reducere
            if (order.User != null)
            {
                // Dacă utilizatorul nu este încărcat complet, îl încărcăm
                if (order.User.UserId == 0)
                {
                    var user = await _context.Users.FindAsync(order.UserId);
                    if (user != null)
                    {
                        order.User = user;
                    }
                }
                
                // Obținem numărul de comenzi anterioare ale utilizatorului
                int orderCount = await _context.Orders
                    .AsNoTracking() // Adăugăm AsNoTracking pentru a evita probleme de tracking
                    .Where(o => o.UserId == order.UserId && (o.Status == "livrata" || o.Status == "Livrata"))
                    .CountAsync();
                
                // Afișăm în consolă pentru debug
                System.Diagnostics.Debug.WriteLine($"Utilizatorul {order.UserId} are {orderCount} comenzi livrate. Prag pentru reducere: {_minimumOrdersForDiscount}");
                
                // Dacă numărul de comenzi depășește pragul, aplicăm reducerea
                if (orderCount >= _minimumOrdersForDiscount)
                {
                    decimal originalAmount = order.FinalAmount;
                    decimal discountPercent = _discountPercent / 100.0m;
                    
                    // Aplicăm reducerea la suma totală
                    decimal discountAmount = originalAmount * discountPercent;
                    order.FinalAmount = originalAmount - discountAmount;
                    
                    System.Diagnostics.Debug.WriteLine($"S-a aplicat reducere de {_discountPercent}% pentru utilizatorul {order.UserId}. Suma inițială: {originalAmount}, Suma finală: {order.FinalAmount}");
                }
            }
            
            return order;
        }

        public override async Task<Order> AddAsync(Order order)
        {
            // Folosim baza existentă pentru a adăuga comanda
            var addedOrder = await base.AddAsync(order);
            
            // Apoi aplicăm reducerea dacă este cazul
            addedOrder = await CalculateFinalAmountWithDiscountsAsync(addedOrder);
            
            // Dacă suma s-a modificat, actualizăm comanda
            await UpdateAsync(addedOrder);
            
            return addedOrder;
        }
    }
} 