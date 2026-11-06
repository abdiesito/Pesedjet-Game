namespace Pesedjet.Server.PlayerManagement.Authentication;


public interface ITokenInvalidator
{
    bool InvalidateToken(string rememberToken);
}