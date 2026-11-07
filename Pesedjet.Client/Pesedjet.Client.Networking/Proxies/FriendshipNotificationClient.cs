using System.ServiceModel;
using Pesedjet.Client.Networking.Callbacks;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Endpoints;
using Pesedjet.Server.Contracts.Interfaces;

namespace Pesedjet.Client.Networking.Proxies;

public sealed class FriendshipNotificationClient : IFriendshipNotificationClient
{
    private const int OperationTimeoutSeconds = 15;

    private static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(OperationTimeoutSeconds);

    private readonly FriendshipCallbackHandler _callbackHandler;
    private readonly DuplexChannelFactory<IFriendshipNotificationService> _channelFactory;
    private readonly object _syncRoot = new();
    private IFriendshipNotificationService? _channel;
    private bool _isDisposed;

    public FriendshipNotificationClient()
    {
        _callbackHandler = new FriendshipCallbackHandler();

        NetTcpBinding binding = new(SecurityMode.None)
        {
            OpenTimeout = OperationTimeout,
            SendTimeout = OperationTimeout
        };
        InstanceContext callbackContext = new(_callbackHandler);
        _channelFactory = new DuplexChannelFactory<IFriendshipNotificationService>(
            callbackContext,
            binding,
            new EndpointAddress(ServiceEndpoints.FriendshipNotificationServiceAddress));
    }

    public event EventHandler<FriendRequestEntry>? FriendRequestReceived
    {
        add => _callbackHandler.FriendRequestReceived += value;
        remove => _callbackHandler.FriendRequestReceived -= value;
    }

    public event EventHandler<FriendEntry>? FriendshipEstablished
    {
        add => _callbackHandler.FriendshipEstablished += value;
        remove => _callbackHandler.FriendshipEstablished -= value;
    }

    public event EventHandler<LobbyInvitationEntry>? LobbyInvitationReceived
    {
        add => _callbackHandler.LobbyInvitationReceived += value;
        remove => _callbackHandler.LobbyInvitationReceived -= value;
    }

    public event EventHandler<string>? LobbyInvitationWithdrawn
    {
        add => _callbackHandler.LobbyInvitationWithdrawn += value;
        remove => _callbackHandler.LobbyInvitationWithdrawn -= value;
    }

    public void Subscribe(Guid playerId)
    {
        IFriendshipNotificationService channel = GetOrCreateChannel();

        try
        {
            channel.Subscribe(playerId);
        }
        catch (Exception exception) when (IsTransportFailure(exception))
        {
            AbortChannel(channel);
            throw;
        }
    }

    public void Unsubscribe(Guid playerId)
    {
        IFriendshipNotificationService channel = GetOrCreateChannel();

        try
        {
            channel.Unsubscribe(playerId);
        }
        catch (Exception exception) when (IsTransportFailure(exception))
        {
            AbortChannel(channel);
        }
    }

    public void Dispose()
    {
        IFriendshipNotificationService? channel;
        bool wasDisposed;

        lock (_syncRoot)
        {
            wasDisposed = _isDisposed;
            channel = _channel;
            _channel = null;
            _isDisposed = true;
        }

        if (!wasDisposed)
        {
            if (channel is not null)
            {
                CloseSafely((ICommunicationObject)channel);
            }

            CloseSafely(_channelFactory);
        }
    }

    private static bool IsTransportFailure(Exception exception)
    {
        bool isCommunicationFailure = exception is CommunicationException and not FaultException;
        bool isTimeout = exception is TimeoutException;

        return isCommunicationFailure || isTimeout;
    }

    private static bool IsUnusable(ICommunicationObject communicationObject)
    {
        bool isUnusable = communicationObject.State is CommunicationState.Faulted
            or CommunicationState.Closing
            or CommunicationState.Closed;

        return isUnusable;
    }

    private static void CloseSafely(ICommunicationObject communicationObject)
    {
        try
        {
            communicationObject.Close();
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

    private IFriendshipNotificationService GetOrCreateChannel()
    {
        IFriendshipNotificationService channel;

        lock (_syncRoot)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);

            if (_channel is null || IsUnusable((ICommunicationObject)_channel))
            {
                if (_channel is not null)
                {
                    ((ICommunicationObject)_channel).Abort();
                }

                _channel = _channelFactory.CreateChannel();
            }

            channel = _channel;
        }

        return channel;
    }

    private void AbortChannel(IFriendshipNotificationService channel)
    {
        ((ICommunicationObject)channel).Abort();

        lock (_syncRoot)
        {
            if (ReferenceEquals(_channel, channel))
            {
                _channel = null;
            }
        }
    }
}
