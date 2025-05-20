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
                
                // Verificăm dacă preparatul este prezent în comenzi
                bool existsInOrders = await _context.OrderDishes
                    .AnyAsync(od => od.DishId == dishId);
                
                if (existsInOrders)
                {
                    // Nu putem șterge un preparat care este folosit în comenzi
                    Console.WriteLine($"Preparatul cu id-ul {dishId} nu poate fi șters deoarece există comenzi care îl conțin.");
                    return false;
                }
                
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
        
        /// <summary>
        /// Actualizează cantitatea în stoc pentru un preparat
        /// </summary>
        /// <param name="dishId">ID-ul preparatului</param>
        /// <param name="quantityToAdd">Cantitatea de adăugat (poate fi și negativă pentru scădere)</param>
        /// <returns>True dacă actualizarea a reușit, false în caz contrar</returns>
        public async Task<bool> UpdateStockAsync(int dishId, int quantityToAdd)
        {
            try
            {
                // Resetăm starea de tracking pentru a evita probleme cu entități duplicate
                _context.ChangeTracker.Clear();
                
                // Găsim preparatul direct din baza de date, nu din cache
                var preparat = await _context.Dishes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.DishId == dishId);

                if (preparat == null)
                {
                    Console.WriteLine($"Preparatul cu id-ul {dishId} nu a fost găsit.");
                    return false;
                }
                
                // Luăm cantitatea curentă din baza de date
                int currentQuantity = preparat.TotalQuantityGrams;
                Console.WriteLine($"Cantitate curentă: {currentQuantity}, cantitate de adăugat: {quantityToAdd}");
                
                // Calculăm noua cantitate
                int newQuantity = currentQuantity + quantityToAdd;
                
                // Verificăm să nu fie negativă
                if (newQuantity < 0)
                {
                    Console.WriteLine($"Cantitatea nouă ar fi negativă: {newQuantity}");
                    return false;
                }
                
                // Actualizăm direct în baza de date folosind SQL pentru a evita probleme de concurență
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE Dishes SET TotalQuantityGrams = {0} WHERE DishId = {1}",
                    newQuantity, dishId);
                
                Console.WriteLine($"Linii afectate de actualizare: {result}");
                
                return result > 0;
            }
            catch (Exception ex)
            {
                // Logăm eroarea sau o tratăm corespunzător
                Console.WriteLine($"Eroare la actualizarea stocului: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Detalii eroare: {ex.InnerException.Message}");
                }
                throw; // Re-aruncăm excepția pentru a fi tratată de apelant
            }
        }

        public override async Task<Dish> UpdateAsync(Dish entity)
        {
            // Apelăm implementarea de bază pentru a actualiza entitatea
            return await base.UpdateAsync(entity);
        }
        
        public async Task<Dish> UpdateWithAllergensAsync(Dish dish)
        {
            try
            {
                // Resetăm starea de tracking pentru a evita probleme
                _context.ChangeTracker.Clear();
                
                // Salvăm referințe la colecțiile actuale
                var dishId = dish.DishId;
                var allergenIds = dish.DishAllergens.Select(da => da.AllergenId).ToList();
                
                // Concatenăm ID-urile alergenilor într-un string separat prin virgule
                var allergenIdsString = string.Join(",", allergenIds);
                
                // Apelăm procedura stocată pentru actualizarea preparatului și alergenilor
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC UpdateDishWithAllergens @p0, @p1, @p2, @p3, @p4, @p5, @p6",
                    dishId, dish.Name, dish.Price, dish.PortionSizeGrams, dish.TotalQuantityGrams, 
                    dish.CategoryId, allergenIdsString);
                
                // Actualizăm imaginea dacă există
                if (dish.Photos != null && dish.Photos.Count > 0)
                {
                    var photo = dish.Photos.FirstOrDefault();
                    if (photo != null && !string.IsNullOrEmpty(photo.Url))
                    {
                        // Apelăm procedura stocată pentru actualizarea imaginii
                        await _context.Database.ExecuteSqlRawAsync(
                            "EXEC UpdateDishPhoto @p0, @p1",
                            dishId, photo.Url);
                    }
                }
                
                // Curățăm contextul pentru a fi siguri că nimic nu interferează
                _context.ChangeTracker.Clear();
                
                // Reîncărcăm preparatul complet pentru a returna date actualizate
                return await GetByIdAsync(dishId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la actualizarea preparatului cu alergeni: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Detalii eroare: {ex.InnerException.Message}");
                }
                throw;
            }
        }
    }
} 