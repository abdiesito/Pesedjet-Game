using Pesedjet.Client.Networking.Results;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.Networking.Proxies;

public interface IAccountClient
{
    Task<AccountRegistrationResult> RegisterAccountAsync(UserRegisterRequest request);
}
