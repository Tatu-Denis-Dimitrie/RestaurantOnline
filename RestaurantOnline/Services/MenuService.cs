using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Data;
using RestaurantOnline.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

namespace RestaurantOnline.Services
{
    public class MenuService : RestaurantDataS<Menu>
    {
        private readonly AppSettings _appSettings;
        
        public MenuService(RestaurantDbContext context, AppSettings appSettings) : base(context)
        {
            _appSettings = appSettings;
        }

        public override async Task<ObservableCollection<Menu>> GetAllAsync()
        {
            var menus = await _context.Menus
                .Include(m => m.Category)
                .Include(m => m.MenuDishes)
                    .ThenInclude(md => md.Dish)
                        .ThenInclude(d => d.Photos)
                .AsNoTracking()
                .ToListAsync();

            foreach (var menu in menus)
            {
                foreach (var menuDish in menu.MenuDishes)
                {
                    if (menuDish.Dish != null)
                    {
                        var dishWithAllergens = await _context.Dishes
                            .Include(d => d.DishAllergens)
                                .ThenInclude(da => da.Allergen)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(d => d.DishId == menuDish.Dish.DishId);
                        
                        if (dishWithAllergens != null)
                        {
                            menuDish.Dish.DishAllergens = dishWithAllergens.DishAllergens;
                        }
                    }
                }
                
                if (_appSettings.MenuDiscountPercent > 0)
                {
                    menu.DiscountPercent = _appSettings.MenuDiscountPercent;
                }
            }

            return new ObservableCollection<Menu>(menus);
        }

        public override async Task<Menu> GetByIdAsync(object id)
        {
            if (id is int menuId)
            {
                var menu = await _context.Menus
                    .Include(m => m.Category)
                    .Include(m => m.MenuDishes)
                        .ThenInclude(md => md.Dish)
                            .ThenInclude(d => d.Photos)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.MenuId == menuId);

                if (menu != null)
                {
                    foreach (var menuDish in menu.MenuDishes)
                    {
                        if (menuDish.Dish != null)
                        {
                            var dishWithAllergens = await _context.Dishes
                                .Include(d => d.DishAllergens)
                                    .ThenInclude(da => da.Allergen)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(d => d.DishId == menuDish.Dish.DishId);
                            
                            if (dishWithAllergens != null)
                            {
                                menuDish.Dish.DishAllergens = dishWithAllergens.DishAllergens;
                            }
                        }
                    }
                    
                    if (_appSettings.MenuDiscountPercent > 0)
                    {
                        menu.DiscountPercent = _appSettings.MenuDiscountPercent;
                    }
                }

                return menu;
            }
            return null;
        }
        
        public override async Task<bool> DeleteAsync(object id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                
                if (!(id is int menuId))
                    return false;
                
                bool existsInOrders = await _context.OrderDishes
                    .AnyAsync(od => od.MenuId == menuId);
                
                if (existsInOrders)
                {
                    return false;
                }
                
                var menu = await _context.Menus
                    .Include(m => m.MenuDishes)
                    .FirstOrDefaultAsync(m => m.MenuId == menuId);

                if (menu == null)
                    return false;

                foreach (var menuDish in menu.MenuDishes.ToList())
                {
                    _context.MenuDishes.Remove(menuDish);
                }

                _context.Menus.Remove(menu);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw; 
            }
        }

        public async Task<IEnumerable<Menu>> GetMenusWithoutAllergens(int[] allergenIds)
        {
            var menus = await _context.Menus
                .Include(m => m.Category)
                .Include(m => m.MenuDishes)
                    .ThenInclude(md => md.Dish)
                        .ThenInclude(d => d.Photos)
                .Include(m => m.MenuDishes)
                    .ThenInclude(md => md.Dish)
                        .ThenInclude(d => d.DishAllergens)
                .AsNoTracking()
                .ToListAsync();

            var filteredMenus = menus.Where(menu =>
                menu.MenuDishes.All(md =>
                    md.Dish == null || !md.Dish.DishAllergens.Any(da =>
                        allergenIds.Contains(da.AllergenId))
                )
            ).ToList();

            foreach (var menu in filteredMenus)
            {
                foreach (var menuDish in menu.MenuDishes)
                {
                    if (menuDish.Dish != null)
                    {
                        var dishWithAllergens = await _context.Dishes
                            .Include(d => d.DishAllergens)
                                .ThenInclude(da => da.Allergen)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(d => d.DishId == menuDish.Dish.DishId);
                        
                        if (dishWithAllergens != null)
                        {
                            menuDish.Dish.DishAllergens = dishWithAllergens.DishAllergens;
                        }
                    }
                }
                
                if (_appSettings.MenuDiscountPercent > 0)
                {
                    menu.DiscountPercent = _appSettings.MenuDiscountPercent;
                }
            }

            return filteredMenus;
        }
    }
} 