using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Data;
using RestaurantOnline.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantOnline.Services
{
    public class DishS : RestaurantDataS<Dish>
    {
        public DishS(RestaurantDbContext context) : base(context)
        {
        }

        public override async Task<ObservableCollection<Dish>> GetAllAsync()
        {
            var preparate = await _context.Dishes
                .Include(p => p.Category)
                .Include(p => p.Photos)
                .Include(p => p.DishAllergens)
                    .ThenInclude(pa => pa.Allergen)
                .ToListAsync();

            return new ObservableCollection<Dish>(preparate);
        }

        public override async Task<Dish?> GetByIdAsync(object id)
        {
            if (id is int preparatId)
            {
                return await _context.Dishes
                    .Include(p => p.Category)
                    .Include(p => p.Photos)
                    .Include(p => p.DishAllergens)
                    .ThenInclude(pa => pa.Allergen)
                    .FirstOrDefaultAsync(p => p.DishId == preparatId);
            }
            return null;
        }

        public async Task<ObservableCollection<Dish>> GetByCategorie(int categorieId)
        {
            var preparate = await _context.Dishes
                .Where(p => p.CategoryId == categorieId)
                .Include(p => p.Photos)
                .Include(p => p.DishAllergens)
                    .ThenInclude(pa => pa.Allergen)
                .ToListAsync();

            return new ObservableCollection<Dish>(preparate);
        }
        
        public async Task<Dish> GetDetaliiPreparat(int preparatId)
        {
            return await GetByIdAsync(preparatId);
        }

        public async Task<bool> AreAlergeni(int preparatId, int[] alergeniIds)
        {
            return await _context.DishAllergens
                .AnyAsync(pa => pa.DishId == preparatId && alergeniIds.Contains(pa.AllergenId));
        }

        public async Task<IEnumerable<Dish>> SearchPreparat(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return await GetAllAsync();

            searchTerm = searchTerm.ToLower();
            
            return await _context.Dishes
                .Include(p => p.Category)
                .Include(p => p.Photos)
                .Include(p => p.DishAllergens)
                    .ThenInclude(pa => pa.Allergen)
                .Where(p => p.Name.ToLower().Contains(searchTerm) || 
                            p.Category.Name.ToLower().Contains(searchTerm))
                .ToListAsync();
        }
    }
} 