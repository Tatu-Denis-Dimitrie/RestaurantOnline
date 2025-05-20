using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Data;
using RestaurantOnline.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Linq;

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
            // Încărcăm meniurile și includem relațiile necesare pentru a afișa preparatele
            var menus = await _context.Menus
                .Include(m => m.Category)
                .Include(m => m.MenuDishes)
                    .ThenInclude(md => md.Dish)
                        .ThenInclude(d => d.Photos)
                .AsNoTracking()
                .ToListAsync();

            // Încărcăm manual alergenii pentru fiecare preparat
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
                
                // Aplicăm reducerea pentru meniu dacă este configurată
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
                    // Încărcăm manual alergenii pentru fiecare preparat
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
                    
                    // Aplicăm reducerea pentru meniu dacă este configurată
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
                // Resetam starea de tracking pentru a evita probleme cu entitati duplicate
                _context.ChangeTracker.Clear();
                
                if (!(id is int menuId))
                    return false;
                
                // Verificăm dacă meniul este prezent în comenzi
                bool existsInOrders = await _context.OrderDishes
                    .AnyAsync(od => od.MenuId == menuId);
                
                if (existsInOrders)
                {
                    // Nu putem șterge un meniu care este folosit în comenzi
                    Console.WriteLine($"Meniul cu id-ul {menuId} nu poate fi șters deoarece există comenzi care îl conțin.");
                    return false;
                }
                
                // Găsim meniul cu toate relațiile sale
                var menu = await _context.Menus
                    .Include(m => m.MenuDishes)
                    .FirstOrDefaultAsync(m => m.MenuId == menuId);

                if (menu == null)
                    return false;

                // Ștergem toate relațiile MenuDish
                foreach (var menuDish in menu.MenuDishes.ToList())
                {
                    _context.MenuDishes.Remove(menuDish);
                }

                // Ștergem meniul
                _context.Menus.Remove(menu);

                // Salvăm schimbările
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Logăm eroarea sau o tratăm corespunzător
                Console.WriteLine($"Eroare la ștergerea meniului: {ex.Message}");
                throw; // Re-aruncăm excepția pentru a fi tratată de apelant
            }
        }
    }
} 