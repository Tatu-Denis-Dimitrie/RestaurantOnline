using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Data;
using RestaurantOnline.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

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
                _context.Dishes.Add(entity);
                
                await _context.SaveChangesAsync();
                
                return entity;
            }
            catch
            {
                throw; 
            }
        }

        public async Task<bool> DeleteDishAsync(int dishId)
        {
            try
            {
                _context.ChangeTracker.Clear();
                
                bool existsInOrders = await _context.OrderDishes
                    .AnyAsync(od => od.DishId == dishId);
                
                if (existsInOrders)
                {
                    return false;
                }
                
                var preparat = await _context.Dishes
                    .Include(p => p.DishAllergens)
                    .Include(p => p.Photos)
                    .Include(p => p.MenuDishes)
                    .FirstOrDefaultAsync(p => p.DishId == dishId);

                if (preparat == null)
                    return false;

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

                _context.Dishes.Remove(preparat);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw; 
            }
        }

        public async Task<bool> UpdateStockAsync(int dishId, int quantityToAdd)
        {
            try
            {
                _context.ChangeTracker.Clear();

                var preparat = await _context.Dishes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.DishId == dishId);

                if (preparat == null)
                {
                    return false;
                }

                int currentQuantity = preparat.TotalQuantityGrams;

                int newQuantity = currentQuantity + quantityToAdd;

                if (newQuantity < 0)
                {
                    return false;
                }

                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC UpdateDishStock @DishId, @NewQuantity",
                    new Microsoft.Data.SqlClient.SqlParameter("@DishId", dishId),
                    new Microsoft.Data.SqlClient.SqlParameter("@NewQuantity", newQuantity));

                return result > 0;
            }
            catch
            {
                throw;
            }
        }

        public override async Task<Dish> UpdateAsync(Dish entity)
        {
            return await base.UpdateAsync(entity);
        }
        
        public async Task<Dish> UpdateWithAllergensAsync(Dish dish)
        {
            try
            {
                _context.ChangeTracker.Clear();
                
                var dishId = dish.DishId;
                var allergenIds = dish.DishAllergens.Select(da => da.AllergenId).ToList();
                
                var allergenIdsString = string.Join(",", allergenIds);
                
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC UpdateDishWithAllergens @p0, @p1, @p2, @p3, @p4, @p5, @p6",
                    dishId, dish.Name, dish.Price, dish.PortionSizeGrams, dish.TotalQuantityGrams, 
                    dish.CategoryId, allergenIdsString);
                
                if (dish.Photos != null && dish.Photos.Count > 0)
                {
                    var photo = dish.Photos.FirstOrDefault();
                    if (photo != null && !string.IsNullOrEmpty(photo.Url))
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "EXEC UpdateDishPhoto @p0, @p1",
                            dishId, photo.Url);
                    }
                }
                
                _context.ChangeTracker.Clear();
                
                return await GetByIdAsync(dishId);
            }
            catch
            {
                throw;
            }
        }
    }
} 