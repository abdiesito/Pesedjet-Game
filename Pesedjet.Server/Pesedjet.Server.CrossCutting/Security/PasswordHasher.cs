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

    public bool VerifyPassword(string password, string passwordHash, Guid playerId)
    {
        bool isValid = false;

        string computedHash = HashPassword(password, playerId);
        isValid = CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(computedHash),
            Convert.FromHexString(passwordHash));

        return isValid;
    }
}