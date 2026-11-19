using Pesedjet.Client.Networking.Results;
using Pesedjet.Server.Contracts.DTO;

namespace Pesedjet.Client.Networking.Proxies.Interfaces;

public interface IAuthClient
{
    ClientResult<UserSession> Login(UserLoginRequest request);

    ClientResult<UserSession> ResumeSession(string rememberToken);

    ClientResult<UserSession> CreateGuestSession();

    ClientResult<bool> Logout(string sessionToken);
}
