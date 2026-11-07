using System.ServiceModel;
using Pesedjet.Client.Networking.Callbacks;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Endpoints;
using Pesedjet.Server.Contracts.Interfaces;

namespace Pesedjet.Client.Networking.Proxies;

public sealed class LobbyClient : ILobbyClient
{
    private const int OperationTimeoutSeconds = 15;

    private static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(OperationTimeoutSeconds);

    private readonly LobbyCallbackHandler _callbackHandler;
    private readonly DuplexChannelFactory<ILobbyService> _channelFactory;
    private readonly object _syncRoot = new();
    private readonly object _eventLock = new();
    private readonly Queue<PendingLobbyEvent> _pendingEvents = new();
    private EventHandler<LobbyMember>? _playerJoined;
    private EventHandler<Guid>? _playerLeft;
    private ILobbyService? _channel;
    private bool _isDisposed;

    public LobbyClient()
    {
        _callbackHandler = new LobbyCallbackHandler();
        _callbackHandler.PlayerJoined += OnPlayerJoined;
        _callbackHandler.PlayerLeft += OnPlayerLeft;

        NetTcpBinding binding = new(SecurityMode.None)
        {
            OpenTimeout = OperationTimeout,
            SendTimeout = OperationTimeout
        };
        InstanceContext callbackContext = new(_callbackHandler);
        _channelFactory = new DuplexChannelFactory<ILobbyService>(
            callbackContext,
            binding,
            new EndpointAddress(ServiceEndpoints.LobbyServiceAddress));
    }

    public event EventHandler<LobbyMember>? PlayerJoined
    {
        add
        {
            lock (_eventLock)
            {
                _playerJoined += value;
            }
        }
        remove
        {
            lock (_eventLock)
            {
                _playerJoined -= value;
            }
        }
    }

    public event EventHandler<Guid>? PlayerLeft
    {
        add
        {
            lock (_eventLock)
            {
                _playerLeft += value;
            }
        }
        remove
        {
            lock (_eventLock)
            {
                _playerLeft -= value;
            }
        }
    }
    
    public string CreateLobby(LobbyMember host, bool isPublic, int maxPlayers)
    {
        ClearPendingEvents();

        ILobbyService channel = GetOrCreateChannel();
        string lobbyCode;

        try
        {
            lobbyCode = channel.CreateLobby(host, isPublic, maxPlayers);
        }
        catch (Exception exception) when (IsTransportFailure(exception))
        {
            AbortChannel(channel);
            throw;
        }

        return lobbyCode;
    }

    public LobbyJoinResponse JoinLobby(string lobbyCode, LobbyMember player)
    {
        ClearPendingEvents();

        ILobbyService channel = GetOrCreateChannel();
        LobbyJoinResponse response;

        try
        {
            response = channel.JoinLobby(lobbyCode, player);
        }
        catch (Exception exception) when (IsTransportFailure(exception))
        {
            AbortChannel(channel);
            throw;
        }

        return response;
    }

    public void LeaveLobby(string lobbyCode, Guid playerId)
    {
        ILobbyService channel = GetOrCreateChannel();

        try
        {
            channel.LeaveLobby(lobbyCode, playerId);
        }
        catch (Exception exception) when (IsTransportFailure(exception))
        {
            AbortChannel(channel);
            throw;
        }
    }

    public List<LobbySummary> GetPublicLobbies()
    {
        ILobbyService channel = GetOrCreateChannel();
        List<LobbySummary> lobbies;

        try
        {
            lobbies = channel.GetPublicLobbies();
        }
        catch (Exception exception) when (IsTransportFailure(exception))
        {
            AbortChannel(channel);
            throw;
        }

        return lobbies;
    }

    public void ReplayPendingEvents()
    {
        lock (_eventLock)
        {
            while (_pendingEvents.Count > 0)
            {
                PendingLobbyEvent pending = _pendingEvents.Dequeue();

                if (pending.Joined is not null)
                {
                    _playerJoined?.Invoke(this, pending.Joined);
                }
                else
                {
                    _playerLeft?.Invoke(this, pending.LeftPlayerId);
                }
            }
        }
    }

    public void Dispose()
    {
        ILobbyService? channel;
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
            _callbackHandler.PlayerJoined -= OnPlayerJoined;
            _callbackHandler.PlayerLeft -= OnPlayerLeft;
            ClearPendingEvents();

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

    private ILobbyService GetOrCreateChannel()
    {
        ILobbyService channel;

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

    private void AbortChannel(ILobbyService channel)
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

    private void ClearPendingEvents()
    {
        lock (_eventLock)
        {
            _pendingEvents.Clear();
        }
    }

    // Events are raised inside the lock so replayed and live events keep the server's order;
    // subscribers must only marshal to the UI thread (Dispatcher.Post) and return.
    private void OnPlayerJoined(object? sender, LobbyMember player)
    {
        lock (_eventLock)
        {
            if (_playerJoined is null)
            {
                _pendingEvents.Enqueue(new PendingLobbyEvent(player, Guid.Empty));
            }
            else
            {
                _playerJoined.Invoke(this, player);
            }
        }
    }

    private void OnPlayerLeft(object? sender, Guid playerId)
    {
        lock (_eventLock)
        {
            if (_playerLeft is null)
            {
                _pendingEvents.Enqueue(new PendingLobbyEvent(null, playerId));
            }
            else
            {
                _playerLeft.Invoke(this, playerId);
            }
        }
    }
}
