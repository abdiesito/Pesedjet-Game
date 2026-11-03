using System.ServiceModel;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Endpoints;
using Pesedjet.Server.Contracts.Faults;
using Pesedjet.Server.Contracts.Interfaces;

namespace Pesedjet.Client.Networking.Proxies;

public class AuthClient : IAuthClient
{
    private const int OperationTimeoutSeconds = 15;

    private static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(OperationTimeoutSeconds);

    public async Task<(AuthResult Result, UserSession? Session)> LoginAsync(UserLoginRequest request)
    {
        AuthResult result = AuthResult.Failed;
        UserSession? session = null;

        var channelFactory = new ChannelFactory<IAuthService>(CreateBinding(), CreateEndpointAddress());
        IAuthService channel = channelFactory.CreateChannel();

        try
        {
            session = await channel.LoginAsync(request);
            result = AuthResult.Success;
        }
        catch (FaultException<ServiceFault> ex)
        {
            result = MapFault(ex.Detail);
        }
        catch (CommunicationException)
        {
            result = AuthResult.ServerUnreachable;
        }
        catch (TimeoutException)
        {
            result = AuthResult.ServerUnreachable;
        }
        finally
        {
            CloseCommunicationObject(channel as ICommunicationObject);
            CloseCommunicationObject(channelFactory);
        }

        return (result, session);
    }

    public async Task<(AuthResult Result, UserSession? Session)> ResumeSessionAsync(string rememberToken)
    {
        AuthResult result = AuthResult.Failed;
        UserSession? session = null;

        var channelFactory = new ChannelFactory<IAuthService>(CreateBinding(), CreateEndpointAddress());
        IAuthService channel = channelFactory.CreateChannel();

        try
        {
            session = await channel.ResumeSessionAsync(rememberToken);
            result = AuthResult.Success;
        }
        catch (FaultException<ServiceFault> ex)
        {
            result = MapFault(ex.Detail);
        }
        catch (CommunicationException)
        {
            result = AuthResult.ServerUnreachable;
        }
        catch (TimeoutException)
        {
            result = AuthResult.ServerUnreachable;
        }
        finally
        {
            CloseCommunicationObject(channel as ICommunicationObject);
            CloseCommunicationObject(channelFactory);
        }

        return (result, session);
    }

    public async Task<(AuthResult Result, UserSession? Session)> CreateGuestSessionAsync()
    {
        AuthResult result = AuthResult.Failed;
        UserSession? session = null;

        var channelFactory = new ChannelFactory<IAuthService>(CreateBinding(), CreateEndpointAddress());
        IAuthService channel = channelFactory.CreateChannel();

        try
        {
            session = await channel.CreateGuestSessionAsync();
            result = AuthResult.Success;
        }
        catch (FaultException<ServiceFault>)
        {
            result = AuthResult.Failed;
        }
        catch (CommunicationException)
        {
            result = AuthResult.ServerUnreachable;
        }
        catch (TimeoutException)
        {
            result = AuthResult.ServerUnreachable;
        }
        finally
        {
            CloseCommunicationObject(channel as ICommunicationObject);
            CloseCommunicationObject(channelFactory);
        }

        return (result, session);
    }

    public async Task<AuthResult> LogoutAsync(string username)
    {
        AuthResult result = AuthResult.Failed;

        var channelFactory = new ChannelFactory<IAuthService>(CreateBinding(), CreateEndpointAddress());
        IAuthService channel = channelFactory.CreateChannel();

        try
        {
            await channel.LogoutAsync(username);
            result = AuthResult.Success;
        }
        catch (FaultException<ServiceFault> ex)
        {
            result = MapFault(ex.Detail);
        }
        catch (CommunicationException)
        {
            result = AuthResult.ServerUnreachable;
        }
        catch (TimeoutException)
        {
            result = AuthResult.ServerUnreachable;
        }
        finally
        {
            CloseCommunicationObject(channel as ICommunicationObject);
            CloseCommunicationObject(channelFactory);
        }

        return result;
    }

    private static NetTcpBinding CreateBinding()
    {
        return new NetTcpBinding(SecurityMode.None)
        {
            OpenTimeout = OperationTimeout,
            SendTimeout = OperationTimeout
        };
    }

    private static EndpointAddress CreateEndpointAddress()
    {
        return new EndpointAddress(ServiceEndpoints.AuthServiceAddress);
    }

    private static AuthResult MapFault(ServiceFault fault)
    {
        AuthResult result = AuthResult.Failed;

        if (fault.ErrorCode == ServiceErrorCodes.InvalidCredentials)
        {
            result = AuthResult.InvalidCredentials;
        }
        else if (fault.ErrorCode == ServiceErrorCodes.UserNotFound)
        {
            result = AuthResult.UserNotFound;
        }
        else if (fault.ErrorCode == ServiceErrorCodes.InvalidData)
        {
            result = AuthResult.InvalidData;
        }

        return result;
    }

    private static void CloseCommunicationObject(ICommunicationObject? communicationObject)
    {
        if (communicationObject is not null)
        {
            try
            {
                if (communicationObject.State == CommunicationState.Faulted)
                {
                    communicationObject.Abort();
                }
                else
                {
                    communicationObject.Close();
                }
            }
            catch (CommunicationException)
            {
                communicationObject.Abort();
            }
            catch (TimeoutException)
            {
                communicationObject.Abort();
            }
        }
    }
}