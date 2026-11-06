using Pesedjet.Client.Networking.Results;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.Networking.Interfaces;

public interface IAuthClient
{
    ClientResult<UserSession> Login(UserLoginRequest request);

    ClientResult<UserSession> ResumeSession(string rememberToken);

    ClientResult<UserSession> CreateGuestSession();

    ClientResult<bool> Logout(string sessionToken);
}
