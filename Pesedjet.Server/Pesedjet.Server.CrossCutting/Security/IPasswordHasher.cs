namespace Pesedjet.Server.CrossCutting.Security;

public interface IPasswordHasher
{
    string HashPassword(string password, Guid playerId);
}
