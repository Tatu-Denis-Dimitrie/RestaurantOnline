using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Data;
using RestaurantOnline.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RestaurantOnline.Services
{
    public class MenuService : RestaurantDataS<Menu>
    {
        public MenuService(RestaurantDbContext context) : base(context)
        {
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
                }

                return menu;
            }
            return null;
        }
    }
} 