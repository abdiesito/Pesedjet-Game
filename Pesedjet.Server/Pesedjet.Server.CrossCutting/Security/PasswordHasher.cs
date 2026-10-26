using System.Security.Cryptography;

namespace Pesedjet.Server.CrossCutting.Security;

public class PasswordHasher : IPasswordHasher
{
    private const int Iterations = 600000;
    private const int HashSizeInBytes = 32;

    public string HashPassword(string password, Guid playerId)
    {
        byte[] salt = playerId.ToByteArray();
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSizeInBytes);

        return Convert.ToHexString(hash);
    }
}
