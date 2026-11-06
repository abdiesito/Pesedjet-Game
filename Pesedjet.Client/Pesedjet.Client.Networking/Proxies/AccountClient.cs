using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Endpoints;
using Pesedjet.Server.Contracts.Interfaces;

namespace Pesedjet.Client.Networking.Proxies;

public sealed class AccountClient : IAccountClient
{
    private readonly IServiceCaller _serviceCaller;

    public AccountClient(IServiceCaller serviceCaller)
    {
        _serviceCaller = serviceCaller;
    }

    public ClientResult<bool> RegisterAccount(UserRegisterRequest request)
    {
        return _serviceCaller.Call<IAccountService, UserRegisterRequest, bool>(
            ServiceEndpoints.AccountServiceAddress,
            RegisterAccountOperation,
            request);
    }

    private static bool RegisterAccountOperation(IAccountService channel, UserRegisterRequest request)
    {
        return channel.RegisterAccount(request);
    }
}
