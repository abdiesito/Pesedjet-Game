namespace Pesedjet.Server.PlayerManagement.Authentication;

using System.Threading.Tasks;

public interface ITokenInvalidator
{
    Task<bool> InvalidateTokenAsync(string rememberToken);
}