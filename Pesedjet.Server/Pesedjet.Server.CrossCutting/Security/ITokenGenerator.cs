namespace Pesedjet.Server.CrossCutting.Security;

public interface ITokenGenerator
{
    string GenerateSessionToken();
    string GenerateVerificationCode();
}