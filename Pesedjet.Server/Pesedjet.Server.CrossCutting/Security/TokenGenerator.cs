using System;
using System.Security.Cryptography;

namespace Pesedjet.Server.CrossCutting.Security;

public class TokenGenerator : ITokenGenerator
{
    private const int SessionTokenByteLength = 32;
    private const int VerificationCodeByteLength = 4;
    private const int CodeModuloBase = 900000;
    private const int CodeOffset = 100000;

    public string GenerateSessionToken()
    {
        string sessionToken = string.Empty;
        byte[] randomBytes = new byte[SessionTokenByteLength];

        using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
        {
            randomNumberGenerator.GetBytes(randomBytes);
        }

        sessionToken = Convert.ToHexString(randomBytes);

        return sessionToken;
    }

    public string GenerateVerificationCode()
    {
        string verificationCode = string.Empty;
        byte[] randomBytes = new byte[VerificationCodeByteLength];

        using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
        {
            randomNumberGenerator.GetBytes(randomBytes);
        }

        uint randomNumber = BitConverter.ToUInt32(randomBytes, 0);
        int formattedNumber = (int)(randomNumber % CodeModuloBase) + CodeOffset;
        verificationCode = formattedNumber.ToString();

        return verificationCode;
    }
}