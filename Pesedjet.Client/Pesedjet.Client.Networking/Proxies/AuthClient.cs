using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Endpoints;
using Pesedjet.Server.Contracts.Interfaces;

namespace Pesedjet.Client.Networking.Proxies;

public sealed class AuthClient : IAuthClient
{
    private readonly IServiceCaller _serviceCaller;

    public AuthClient(IServiceCaller serviceCaller)
    {
        _serviceCaller = serviceCaller;
    }

    public ClientResult<UserSession> Login(UserLoginRequest request)
    {
        return _serviceCaller.Call<IAuthService, UserLoginRequest, UserSession>(
            ServiceEndpoints.AuthServiceAddress,
            LoginOperation,
            request);
    }

    public ClientResult<UserSession> ResumeSession(string rememberToken)
    {
        return _serviceCaller.Call<IAuthService, string, UserSession>(
            ServiceEndpoints.AuthServiceAddress,
            ResumeSessionOperation,
            rememberToken);
    }

    public ClientResult<UserSession> CreateGuestSession()
    {
        return _serviceCaller.Call<IAuthService, object?, UserSession>(
            ServiceEndpoints.AuthServiceAddress,
            CreateGuestSessionOperation,
            null);
    }

    public ClientResult<bool> Logout(string sessionToken)
    {
        LogoutRequest request = new()
        {
            SessionToken = sessionToken
        };

        return _serviceCaller.Call<IAuthService, LogoutRequest, bool>(
            ServiceEndpoints.AuthServiceAddress,
            LogoutOperation,
            request);
    }

    private static UserSession LoginOperation(IAuthService channel, UserLoginRequest request)
    {
        return channel.Login(request);
    }

    private static UserSession ResumeSessionOperation(IAuthService channel, string rememberToken)
    {
        return channel.ResumeSession(rememberToken);
    }

    private static UserSession CreateGuestSessionOperation(IAuthService channel, object? unused)
    {
        return channel.CreateGuestSession();
    }

    private static bool LogoutOperation(IAuthService channel, LogoutRequest request)
    {
        return channel.Logout(request);
    }
}
