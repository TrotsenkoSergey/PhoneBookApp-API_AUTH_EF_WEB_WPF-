using AuthServer.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.API.Services.RefreshTokenRepositories
{
    public class DataBaseRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AuthenticationDbContext _dbContext;

        public DataBaseRefreshTokenRepository(AuthenticationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Create(RefreshToken refreshToken)
        {
            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var token = await _dbContext.RefreshTokens.FindAsync(id);
            if (token != null)
            {
                _dbContext.RefreshTokens.Remove(token);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteAll(Guid userId)
        {
            IEnumerable<RefreshToken> refreshTokens = await _dbContext.RefreshTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();

            _dbContext.RefreshTokens.RemoveRange(refreshTokens);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<RefreshToken> GetByToken(string token)
        {
            return await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
        }
    }
}
