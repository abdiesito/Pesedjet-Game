using Pesedjet.Server.DataAccess.Entities.Identity;

namespace Pesedjet.Server.DataAccess.Repositories.Interfaces;

public interface IPlayerRepository
{
    Task<Player?> GetPlayerByIdAsync(Guid playerId);
    Task<Player?> GetPlayerByUsernameAsync(string username);

    Task<Player?> GetPlayerByEmailAsync(string email);

    Task<bool> CreatePlayerAsync(Player player);
    
    Task<bool> VerifyCodeAsync(string usernameOrEmail, string code, string purpose);
    Task<bool> GeneratePasswordResetCodeAsync(Guid playerId);
    Task<bool> UpdatePasswordHashAsync(Guid playerId, string newPasswordHash);
}
