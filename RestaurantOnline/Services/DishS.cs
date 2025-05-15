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

        public override async Task<Dish> AddAsync(Dish entity)
        {
            try
            {
                // Adaugam entitatea
                _context.Dishes.Add(entity);
                
                // Salvam modificarile
                await _context.SaveChangesAsync();
                
                return entity;
            }
            catch (Exception ex)
            {
                // Loggam eroarea sau o tratam corespunzator
                Console.WriteLine($"Eroare la adaugarea preparatului: {ex.Message}");
                throw; // Re-aruncam exceptia pentru a fi tratata de apelant
            }
        }

        public async Task<bool> DeleteDishAsync(int dishId)
        {
            try
            {
                // Resetam starea de tracking pentru a evita probleme cu entitati duplicate
                _context.ChangeTracker.Clear();
                
                // Gasim preparatul cu toate relatiile sale
                var preparat = await _context.Dishes
                    .Include(p => p.DishAllergens)
                    .Include(p => p.Photos)
                    .Include(p => p.MenuDishes)
                    .FirstOrDefaultAsync(p => p.DishId == dishId);

                if (preparat == null)
                    return false;

                // stergem toate relatiile
                foreach (var alergen in preparat.DishAllergens.ToList())
                {
                    _context.DishAllergens.Remove(alergen);
                }
                
                foreach (var photo in preparat.Photos.ToList())
                {
                    _context.DishPhotos.Remove(photo);
                }
                
                foreach (var menuDish in preparat.MenuDishes.ToList())
                {
                    _context.MenuDishes.Remove(menuDish);
                }

                // stergem preparatul
                _context.Dishes.Remove(preparat);

                // Salvam schimbarile
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Loggam eroarea sau o tratam corespunzator
                Console.WriteLine($"Eroare la stergerea preparatului: {ex.Message}");
                throw; // Re-aruncam exceptia pentru a fi tratata de apelant
            }
        }
    }
} 