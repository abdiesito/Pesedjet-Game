using Pesedjet.Client.Networking.Results;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.Networking.Interfaces;

public interface IAuthClient
{
    Task<(AuthResult Result, UserSession? Session)> LoginAsync(UserLoginRequest request);
    
    Task<(AuthResult Result, UserSession? Session)> ResumeSessionAsync(string rememberToken);
    
    Task<(AuthResult Result, UserSession? Session)> CreateGuestSessionAsync();
    
    Task<AuthResult> LogoutAsync(string username);
}