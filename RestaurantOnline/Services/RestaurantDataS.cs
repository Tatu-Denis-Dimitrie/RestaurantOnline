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
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
                _dbSet.Attach(entity);
                
            entry.State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<bool> DeleteAsync(object id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 