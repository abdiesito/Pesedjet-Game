using Pesedjet.Server.DataAccess.Entities.Identity;

namespace Pesedjet.Server.DataAccess.Repositories.Interfaces;

public interface IPlayerRepository
{
    Player? GetPlayerById(Guid playerId);
    Player? GetPlayerByUsername(string username);

    Player? GetPlayerByEmail(string email);

    bool CreatePlayer(Player player);
    
    bool VerifyCode(string usernameOrEmail, string code, string purpose);
    bool GeneratePasswordResetCode(Guid playerId);
    bool UpdatePasswordHash(Guid playerId, string newPasswordHash);
    
    bool TryClaimRememberToken(Guid playerId, string token);
    Player? GetPlayerByRememberToken(string token);
    bool RevokeRememberToken(string token);
}
