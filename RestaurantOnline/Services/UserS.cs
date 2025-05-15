using Microsoft.EntityFrameworkCore;
using RestaurantOnline.Data;
using RestaurantOnline.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantOnline.Services
{
    public class UserS : RestaurantDataS<User>, IUserS
    {
        public UserS(RestaurantDbContext context) : base(context)
        {
        }

        public override async Task<User?> GetByIdAsync(object id)
        {
            if (id is int utilizatorId)
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == utilizatorId);
            }
            return null;
        }

        public async Task<User> GetByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;
                
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public User Autentificare(string email, string parola)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(parola))
                return null;
                
            return _context.Users
                .FirstOrDefault(u => u.Email == email && u.Password == parola);
        }

        public void Adauga(User utilizator)
        {
            if (_context.Users.Any(u => u.Email == utilizator.Email))
                throw new Exception("Acest email este deja inregistrat.");

            _context.Users.Add(utilizator);
            _context.SaveChanges();
        }

        public async Task<User?> Inregistrare(User utilizator, string parola)
        {
            if (await GetByEmail(utilizator.Email) != null)
                return null;

            return await AddAsync(utilizator);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                // Resetam starea de tracking pentru a evita probleme cu entitati duplicate
                _context.ChangeTracker.Clear();
                
                // Gasim utilizatorul
                var utilizator = await _context.Users
                    .Include(u => u.Orders)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (utilizator == null)
                    return false;
                
                // Verificam daca utilizatorul are comenzi
                if (utilizator.Orders.Any())
                {
                    throw new Exception("Nu se poate sterge un utilizator care are comenzi asociate.");
                }

                // stergem utilizatorul
                _context.Users.Remove(utilizator);

                // Salvam schimbarile
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Loggam eroarea sau o tratam corespunzator
                Console.WriteLine($"Eroare la stergerea utilizatorului: {ex.Message}");
                throw; // Re-aruncam exceptia pentru a fi tratata de apelant
            }
        }
        
        public async Task<bool> UpdateToAngajatAsync(int userId)
        {
            try
            {
                // Resetam starea de tracking pentru a evita probleme cu entitati duplicate
                _context.ChangeTracker.Clear();
                
                // Gasim utilizatorul
                var utilizator = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (utilizator == null)
                    return false;
                
                // Verificam daca utilizatorul este deja Angajat
                if (utilizator.Role == "Angajat")
                {
                    return true; // Rolul e deja corect
                }

                // Setam rolul utilizatorului la Angajat
                utilizator.Role = "Angajat";

                // Marcam entitatea ca fiind modificata
                _context.Entry(utilizator).State = EntityState.Modified;

                // Salvam schimbarile explicit
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                // Loggam eroarea sau o tratam corespunzator
                Console.WriteLine($"Eroare la actualizarea rolului utilizatorului: {ex.Message}");
                throw; // Re-aruncam exceptia pentru a fi tratata de apelant
            }
        }
        
        public async Task<bool> UpdateToClientAsync(int userId)
        {
            try
            {
                // Resetam starea de tracking pentru a evita probleme cu entitati duplicate
                _context.ChangeTracker.Clear();
                
                // Gasim utilizatorul
                var utilizator = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (utilizator == null)
                    return false;
                
                // Verificam daca utilizatorul este deja Client
                if (utilizator.Role == "Client")
                {
                    return true; // Rolul e deja corect
                }

                // Setam rolul utilizatorului la Client
                utilizator.Role = "Client";

                // Marcam entitatea ca fiind modificata
                _context.Entry(utilizator).State = EntityState.Modified;

                // Salvam schimbarile explicit
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                // Loggam eroarea sau o tratam corespunzator
                Console.WriteLine($"Eroare la actualizarea rolului utilizatorului: {ex.Message}");
                throw; // Re-aruncam exceptia pentru a fi tratata de apelant
            }
        }
    }
} 