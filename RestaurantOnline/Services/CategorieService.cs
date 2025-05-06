using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Data;
using RestaurantOnline.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantOnline.Services
{
    public class CategorieService : RestaurantDataService<Categorie>
    {
        public CategorieService(RestaurantDbContext context) : base(context)
        {
        }

        public override async Task<Categorie?> GetByIdAsync(object id)
        {
            if (id is int categorieId)
            {
                return await _dbSet
                    .Include(c => c.Preparate)
                    .FirstOrDefaultAsync(c => c.IdCategorie == categorieId);
            }
            
            return null;
        }
    }
} 