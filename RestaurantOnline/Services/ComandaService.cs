using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Data;
using RestaurantOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantOnline.Services
{
    public class ComandaService : RestaurantDataService<Comanda>
    {
        public ComandaService(RestaurantDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Comanda>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.Utilizator)
                .OrderByDescending(c => c.DataComanda)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comanda>> GetComenziCompleteAsync()
        {
            return await _dbSet
                .Include(c => c.Utilizator)
                .Include(c => c.ComandaPreparate)
                    .ThenInclude(cp => cp.Preparat)
                .OrderByDescending(c => c.DataComanda)
                .ToListAsync();
        }

        public override async Task<Comanda?> GetByIdAsync(object id)
        {
            if (id is Guid comandaId)
            {
                return await _dbSet
                    .Include(c => c.Utilizator)
                    .Include(c => c.ComandaPreparate)
                        .ThenInclude(cp => cp.Preparat)
                    .FirstOrDefaultAsync(c => c.IdComanda == comandaId);
            }
            
            return null;
        }

        public async Task<IEnumerable<Comanda>> GetByUtilizator(int utilizatorId)
        {
            return await _dbSet
                .Include(c => c.Utilizator)
                .Where(c => c.IdUtilizator == utilizatorId)
                .OrderByDescending(c => c.DataComanda)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comanda>> GetByStare(StareComanda stare)
        {
            return await _dbSet
                .Include(c => c.Utilizator)
                .Where(c => c.Stare == stare)
                .OrderByDescending(c => c.DataComanda)
                .ToListAsync();
        }

        public async Task<bool> UpdateStareComanda(Guid comandaId, StareComanda stare)
        {
            var comanda = await GetByIdAsync(comandaId);
            if (comanda == null) return false;

            comanda.Stare = stare;
            await UpdateAsync(comanda);
            return true;
        }

        public async Task<Comanda> PlaseazaComanda(Comanda comanda, List<ComandaPreparat> preparate)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Adăugăm comanda
                await _dbSet.AddAsync(comanda);
                await _context.SaveChangesAsync();

                // Adăugăm preparatele comenzii
                foreach (var preparat in preparate)
                {
                    preparat.IdComanda = comanda.IdComanda;
                    await _context.ComandaPreparate.AddAsync(preparat);
                }
                await _context.SaveChangesAsync();

                // Calculăm valoarea finală
                decimal valoareTotala = preparate.Sum(p => {
                    var prepareInfo = _context.Preparate.FirstOrDefault(pr => pr.IdPreparate == p.IdPreparate);
                    return prepareInfo?.Pret * p.Cantitate ?? 0;
                });

                // Setăm transportul și valoarea finală
                comanda.ValoareFinala = valoareTotala + comanda.Transport;
                _context.Entry(comanda).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return comanda;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
} 