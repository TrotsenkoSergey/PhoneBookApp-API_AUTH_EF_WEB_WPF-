using AuthServer.API.Models;
using System.Threading.Tasks;

namespace AuthServer.API.Services.UserRepositories
{
    public interface IUserRepository
    {
        Task<User> GetByEmail(string email);
        Task<User> GetByUsername(string username);
        Task<User> Create(User user);
    }
}
