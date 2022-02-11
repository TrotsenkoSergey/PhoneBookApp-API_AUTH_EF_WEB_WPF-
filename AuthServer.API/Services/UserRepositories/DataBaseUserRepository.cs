using AuthServer.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AuthServer.API.Services.UserRepositories
{
    public class DataBaseUserRepository : IUserRepository
    {

        private readonly AuthenticationDbContext _dbContext;

        public DataBaseUserRepository(AuthenticationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> Create(User user)
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return user;
        }

        public async Task<User> GetByEmail(string email)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetById(Guid userId)
        {
            return await _dbContext.Users.FindAsync(userId);
        }

        public async Task<User> GetByUsername(string username)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}
