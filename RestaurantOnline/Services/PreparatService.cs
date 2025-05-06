using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Data;
using RestaurantOnline.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantOnline.Services
{
    public class PreparatService : RestaurantDataService<Preparat>
    {
        public PreparatService(RestaurantDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Preparat>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.Categorie)
                .Include(p => p.PreparatAlergeni)
                    .ThenInclude(pa => pa.Alergen)
                .Include(p => p.Fotografii)
                .ToListAsync();
        }

        public override async Task<Preparat?> GetByIdAsync(object id)
        {
            if (id is int preparatId)
            {
                return await _dbSet
                    .Include(p => p.Categorie)
                    .Include(p => p.Fotografii)
                    .Include(p => p.PreparatAlergeni)
                        .ThenInclude(pa => pa.Alergen)
                    .FirstOrDefaultAsync(p => p.IdPreparate == preparatId);
            }
            
            return null;
        }

        public async Task<IEnumerable<Preparat>> GetByCategorie(int categorieId)
        {
            return await _dbSet
                .Include(p => p.Categorie)
                .Include(p => p.PreparatAlergeni)
                    .ThenInclude(pa => pa.Alergen)
                .Include(p => p.Fotografii)
                .Where(p => p.IdCategorie == categorieId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Preparat>> SearchPreparat(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return await GetAllAsync();

            return await _dbSet
                .Include(p => p.Categorie)
                .Include(p => p.PreparatAlergeni)
                    .ThenInclude(pa => pa.Alergen)
                .Include(p => p.Fotografii)
                .Where(p => p.Denumire.Contains(searchTerm) || 
                            p.Categorie.Nume.Contains(searchTerm))
                .ToListAsync();
        }
    }
} 