using System.Diagnostics;
using System.ServiceModel;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.Networking.Proxies;

public sealed class ServiceCaller : IServiceCaller
{
    private const int OperationTimeoutSeconds = 15;

    private static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(OperationTimeoutSeconds);

    public ClientResult<TResult> Call<TChannel, TArgument, TResult>(
        string address,
        Func<TChannel, TArgument, TResult> operation,
        TArgument argument)
        where TChannel : class
    {
        ChannelFactory<TChannel> channelFactory = new(CreateBinding(), new EndpointAddress(address));
        TChannel channel = channelFactory.CreateChannel();
        ClientResult<TResult> result;

        try
        {
            TResult value = operation(channel, argument);
            result = ClientResult<TResult>.Success(value);
        }
        catch (FaultException<ServiceFault> exception)
        {
            result = ClientResult<TResult>.BusinessFailure(exception.Detail.ErrorCode, exception.Detail.MessageKey);
        }
        catch (FaultException exception)
        {
            Debug.WriteLine($"ServiceCaller: server fault at {address}: {exception}");
            result = ClientResult<TResult>.TechnicalFailure(FailureKind.Unexpected);
        }
        catch (TimeoutException exception)
        {
            Debug.WriteLine($"ServiceCaller: timeout at {address}: {exception.Message}");
            result = ClientResult<TResult>.TechnicalFailure(FailureKind.Timeout);
        }
        catch (CommunicationException exception)
        {
            Debug.WriteLine($"ServiceCaller: unreachable {address}: {exception.Message}");
            result = ClientResult<TResult>.TechnicalFailure(FailureKind.ServerUnreachable);
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"ServiceCaller: unexpected failure at {address}: {exception}");
            result = ClientResult<TResult>.TechnicalFailure(FailureKind.Unexpected);
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
