using System.ServiceModel;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Endpoints;
using Pesedjet.Server.Contracts.Faults;
using Pesedjet.Server.Contracts.Interfaces;

namespace Pesedjet.Client.Networking.Proxies;

public class AccountClient : IAccountClient
{
    private const int OperationTimeoutSeconds = 15;

    private static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(OperationTimeoutSeconds);

    public async Task<AccountRegistrationResult> RegisterAccountAsync(UserRegisterRequest request)
    {
        AccountRegistrationResult result = AccountRegistrationResult.Failed;
        var channelFactory = new ChannelFactory<IAccountService>(CreateBinding(), CreateEndpointAddress());
        IAccountService channel = channelFactory.CreateChannel();

        try
        {
            bool isRegistered = await channel.RegisterAccountAsync(request);

            if (isRegistered)
            {
                result = AccountRegistrationResult.Registered;
            }
        }
        catch (FaultException<ServiceFault> ex)
        {
            result = MapFault(ex.Detail);
        }
        catch (CommunicationException)
        {
            result = AccountRegistrationResult.ServerUnreachable;
        }
        catch (TimeoutException)
        {
            result = AccountRegistrationResult.ServerUnreachable;
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
        return new EndpointAddress(ServiceEndpoints.AccountServiceAddress);
    }

    private static AccountRegistrationResult MapFault(ServiceFault fault)
    {
        AccountRegistrationResult result = AccountRegistrationResult.Failed;

        if (fault.ErrorCode == ServiceErrorCodes.UsernameAlreadyRegistered)
        {
            result = AccountRegistrationResult.UsernameAlreadyRegistered;
        }
        else if (fault.ErrorCode == ServiceErrorCodes.EmailAlreadyRegistered)
        {
            result = AccountRegistrationResult.EmailAlreadyRegistered;
        }
        else if (fault.ErrorCode == ServiceErrorCodes.InvalidRegistrationData)
        {
            result = AccountRegistrationResult.InvalidData;
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
