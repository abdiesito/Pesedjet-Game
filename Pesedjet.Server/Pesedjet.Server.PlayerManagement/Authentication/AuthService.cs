using System.Threading.Tasks;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Interfaces;

namespace Pesedjet.Server.PlayerManagement.Authentication;

public class AuthService : IAuthService
{
    public Task<UserSession> LoginAsync(UserLoginRequest request)
    {
        throw new System.NotImplementedException();
    }

    public Task<UserSession> ResumeSessionAsync(string rememberToken)
    {
        throw new System.NotImplementedException();
    }

    public Task<UserSession> CreateGuestSessionAsync()
    {
        throw new System.NotImplementedException();
    }

    public Task LogoutAsync(string username)
    {
        throw new System.NotImplementedException();
    }
}