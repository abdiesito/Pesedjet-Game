using Pesedjet.Client.Networking.Proxies.Interfaces;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Server.Contracts.DTO;
using Pesedjet.Server.Contracts.Endpoints;
using Pesedjet.Server.Contracts.Interfaces;

namespace Pesedjet.Client.Networking.Proxies.Implementations;

public sealed class AccountClient : IAccountClient
{
    private readonly IServiceCaller _serviceCaller;

    public AccountClient(IServiceCaller serviceCaller)
    {
        _serviceCaller = serviceCaller;
    }

    public ClientResult<bool> RegisterAccount(UserRegistration request)
    {
        return _serviceCaller.Call<IAccountService, UserRegistration, bool>(
            ServiceEndpoints.AccountServiceAddress,
            RegisterAccountOperation,
            request);
    }

    private static bool RegisterAccountOperation(IAccountService channel, UserRegistration request)
    {
        return channel.RegisterAccount(request);
    }
}
