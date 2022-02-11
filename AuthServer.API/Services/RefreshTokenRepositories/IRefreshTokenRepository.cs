using AuthServer.API.Models;
using System;
using System.Threading.Tasks;

namespace AuthServer.API.Services.RefreshTokenRepositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> GetByToken(string token);
        Task Create(RefreshToken refreshToken);
        Task Delete(Guid id);

        Task DeleteAll(Guid userId);
    }
}
