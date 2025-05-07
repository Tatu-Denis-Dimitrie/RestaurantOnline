using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Data;
using RestaurantOnline.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

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
                .Include(c => c.Utilizator)
                .Include(c => c.ComandaPreparate)
                    .ThenInclude(cp => cp.Preparat)
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
            var comenzi = await _context.Orders
                .Where(c => c.IdUtilizator == idUtilizator)
                .Include(c => c.ComandaPreparate)
                    .ThenInclude(cp => cp.Preparat)
                .ToListAsync();

            return new ObservableCollection<Order>(comenzi);
        }

        public async Task<Order> GetComandaDetaliiAsync(int idComanda)
        {
            return await _context.Orders
                .Include(c => c.Utilizator)
                .Include(c => c.ComandaPreparate)
                    .ThenInclude(cp => cp.Preparat)
                .FirstOrDefaultAsync(c => c.IdComanda == idComanda);
        }

        public async Task<bool> ActualizeazaStareComandaAsync(int idComanda, string stareNoua)
        {
            var comanda = await _context.Orders.FindAsync(idComanda);
            if (comanda == null)
                return false;
                
            comanda.Stare = stareNoua;
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 