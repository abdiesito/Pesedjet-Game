using Pesedjet.Client.Networking.Results;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.Networking.Interfaces;

public interface IAccountClient
{
    ClientResult<bool> RegisterAccount(UserRegisterRequest request);
}
