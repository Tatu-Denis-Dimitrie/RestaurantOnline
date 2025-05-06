using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantOnline.Services
{
    public class RestaurantDataService<T> : IRestaurantDataService<T> where T : class
    {
        protected readonly RestaurantDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public RestaurantDataService(RestaurantDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
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