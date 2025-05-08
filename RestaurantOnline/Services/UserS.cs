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
    }
} 