using Pesedjet.Server.DataAccess.Entities.Identity;

namespace Pesedjet.Server.DataAccess.Repositories.Interfaces;

public interface IPlayerRepository
{
    Task<Player?> GetPlayerByUsernameAsync(string username);

    Task<Player?> GetPlayerByEmailAsync(string email);

    Task<bool> CreatePlayerAsync(Player player);
}
