using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Data;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RestaurantOnline.Services
{
    public class RestaurantDataS<T> : IRestaurantS<T> where T : class
    {
        protected readonly RestaurantDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public RestaurantDataS(RestaurantDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<ObservableCollection<T>> GetAllAsync()
        {
            var entities = await _dbSet.ToListAsync();
            return new ObservableCollection<T>(entities);
        }

        public virtual async Task<T> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            try
            {
                // Resetam starea de tracking pentru a evita probleme cu entitati duplicate
                _context.ChangeTracker.Clear();
                
                var entry = _context.Entry(entity);
                if (entry.State == EntityState.Detached)
                    _dbSet.Attach(entity);
                    
                entry.State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Eroare la actualizarea entitatii: {ex.Message}");
                throw; // Re-aruncam exceptia pentru a fi tratata de apelant
            }
        }

        public virtual async Task<bool> DeleteAsync(object id)
        {
            try
            {
                // Resetam starea de tracking pentru a evita probleme cu entitati duplicate
                _context.ChangeTracker.Clear();
                
                var entity = await GetByIdAsync(id);
                if (entity == null) return false;

                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Eroare la stergerea entitatii: {ex.Message}");
                throw; // Re-aruncam exceptia pentru a fi tratata de apelant
            }
        }
    }
} 