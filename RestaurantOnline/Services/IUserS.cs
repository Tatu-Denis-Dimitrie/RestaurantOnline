using RestaurantOnline.Models;
using System.Threading.Tasks;

namespace RestaurantOnline.Services
{
    public interface IUserS : IRestaurantS<User>
    {
        Task<User> GetByEmail(string email);
        User Autentificare(string email, string parola);
        void Adauga(User utilizator);
        Task<User?> Inregistrare(User utilizator, string parola);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> UpdateToAngajatAsync(int userId);
        Task<bool> UpdateToClientAsync(int userId);
    }
} 