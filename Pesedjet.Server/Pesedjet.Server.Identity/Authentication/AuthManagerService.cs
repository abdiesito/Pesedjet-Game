using Pesedjet.Server.Contracts.Interfaces;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Identity.Authentication
{
    public class AuthManagerService : IAuthManager
    {
        public bool RegisterAccount(UserRegisterDto userRegisterDto)
        {
            // TODO: Real logic, database and identity verification email
            return true; 
        }
    }
}