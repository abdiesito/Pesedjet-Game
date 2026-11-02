namespace Pesedjet.Server.CrossCutting.Security;

public interface IPasswordHasher
{
    string HashPassword(string password, Guid playerId);
    bool VerifyPassword(string password, string passwordHash, Guid playerId);
}
