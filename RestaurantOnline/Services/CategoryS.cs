using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Data;
using RestaurantOnline.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RestaurantOnline.Services
{
    public class CategoryS : RestaurantDataS<Category>
    {
        public CategoryS(RestaurantDbContext context) : base(context)
        {
        }

        public override async Task<ObservableCollection<Category>> GetAllAsync()
        {
            var categorii = await _context.Categories.ToListAsync();
            return new ObservableCollection<Category>(categorii);
        }

        public override async Task<Category> GetByIdAsync(object id)
        {
            if (id is int categorieId)
            {
                return await _context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryId == categorieId);
            }
            return null;
        }

        public async Task<Category> GetByIdWithPreparate(int id)
        {
            return await _context.Categories
                .Include(c => c.Dishes)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }
    }
} 