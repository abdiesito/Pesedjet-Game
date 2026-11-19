using Pesedjet.Client.Networking.Results;
using Pesedjet.Server.Contracts.DTO;

namespace Pesedjet.Client.Networking.Proxies.Interfaces;

public interface IAccountClient
{
    ClientResult<bool> RegisterAccount(UserRegistration request);
}
