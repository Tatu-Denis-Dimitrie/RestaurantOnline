using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Data;
using RestaurantOnline.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RestaurantOnline.Services
{
    public class AllergenS : RestaurantDataS<Allergen>
    {
        public AllergenS(RestaurantDbContext context) : base(context)
        {
        }

        public override async Task<ObservableCollection<Allergen>> GetAllAsync()
        {
            var alergeni = await _context.Allergens.ToListAsync();
            return new ObservableCollection<Allergen>(alergeni);
        }

        public override async Task<Allergen> GetByIdAsync(object id)
        {
            if (id is int alergenId)
            {
                return await _context.Allergens
                    .FirstOrDefaultAsync(a => a.AllergenId == alergenId);
            }
            return null;
        }
    }
} 