using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Data;
using RestaurantOnline.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantOnline.Services
{
    public class UtilizatorService : RestaurantDataService<Utilizator>
    {
        public UtilizatorService(RestaurantDbContext context) : base(context)
        {
        }

        public override async Task<Utilizator?> GetByIdAsync(object id)
        {
            if (id is int utilizatorId)
            {
                return await _dbSet
                    .FirstOrDefaultAsync(u => u.IdUtilizator == utilizatorId);
            }
            
            return null;
        }

        public async Task<Utilizator?> GetByEmail(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Utilizator?> Autentificare(string email, string parola)
        {
            var utilizator = await GetByEmail(email);
            if (utilizator == null) return null;

            // Verifică parola cu hash-ul stocat
            if (VerificaParola(parola, utilizator.ParolaHash))
                return utilizator;

            return null;
        }

        public async Task<Utilizator?> Inregistrare(Utilizator utilizator, string parola)
        {
            // Verifică dacă există deja un utilizator cu acest email
            if (await GetByEmail(utilizator.Email) != null)
                return null;

            // Generează hash pentru parolă
            utilizator.ParolaHash = HashParola(parola);

            // Salvează utilizatorul
            await AddAsync(utilizator);
            return utilizator;
        }

        private string HashParola(string parola)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(parola));
            return Convert.ToBase64String(hashBytes);
        }

        private bool VerificaParola(string parola, string hash)
        {
            var hashParola = HashParola(parola);
            return hashParola == hash;
        }
    }
} 