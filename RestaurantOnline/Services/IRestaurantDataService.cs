using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantOnline.Services
{
    public interface IRestaurantDataService<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(object id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(object id);
    }
} 