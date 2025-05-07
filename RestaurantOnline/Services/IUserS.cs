using RestaurantOnline.Models;
using System.Threading.Tasks;

namespace RestaurantOnline.Services
{
    public interface IUserS : IRestaurantS<User>
    {
        Task<User> GetByEmail(string email);
        User Autentificare(string email, string parola);
        void Adauga(User utilizator);
    }
} 