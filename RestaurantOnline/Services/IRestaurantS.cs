using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RestaurantOnline.Services
{
    public interface IRestaurantS<T> where T : class
    {
        Task<ObservableCollection<T>> GetAllAsync();
        Task<T> GetByIdAsync(object id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(object id);
    }
} 