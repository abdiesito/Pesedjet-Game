#nullable enable

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Pesedjet.Client.Networking.Callbacks;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.Networking.Proxies;

public sealed class RoomClient : IRoomClient
{
    private readonly RoomCallbackHandler _callbackHandler;
    private readonly DuplexChannelFactory<IRoomServiceAsync> _channelFactory;
    private readonly object _syncRoot = new();
    private readonly object _eventLock = new();
    private readonly Queue<Action> _pendingEvents = new();
    private EventHandler<RoomMember>? _playerJoined;
    private EventHandler<Guid>? _playerLeft;
    private IRoomServiceAsync? _channel;
    private bool _isDisposed;

    public RoomClient(Uri serviceAddress)
    {
        _callbackHandler = new RoomCallbackHandler();
        _callbackHandler.PlayerJoined += OnPlayerJoined;
        _callbackHandler.PlayerLeft += OnPlayerLeft;

        NetTcpBinding binding = new(SecurityMode.None);
        InstanceContext callbackContext = new(_callbackHandler);
        _channelFactory = new DuplexChannelFactory<IRoomServiceAsync>(
            callbackContext,
            binding,
            new EndpointAddress(serviceAddress));
    }

    public event EventHandler<RoomMember>? PlayerJoined
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

    public Task<string> CreateRoomAsync(RoomMember host, bool isPublic, int maxPlayers)
    {
        ClearPendingEvents();
        Task<string> operation = ExecuteAsync(channel => channel.CreateRoomAsync(host, isPublic, maxPlayers));

        return operation;
    }

    public Task<RoomJoinResponse> JoinRoomAsync(string roomCode, RoomMember player)
    {
        ClearPendingEvents();
        Task<RoomJoinResponse> operation = ExecuteAsync(channel => channel.JoinRoomAsync(roomCode, player));

        return operation;
    }

    public Task LeaveRoomAsync(string roomCode, Guid playerId)
    {
        Task operation = ExecuteAsync(channel => channel.LeaveRoomAsync(roomCode, playerId));

        return operation;
    }

    public Task<List<RoomSummary>> GetPublicRoomsAsync()
    {
        Task<List<RoomSummary>> operation = ExecuteAsync(channel => channel.GetPublicRoomsAsync());

        return operation;
    }

    public void ReplayPendingEvents()
    {
        lock (_eventLock)
        {
            while (_pendingEvents.Count > 0)
            {
                _pendingEvents.Dequeue().Invoke();
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        IRoomServiceAsync? channel;
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
                await CloseSafelyAsync((ICommunicationObject)channel).ConfigureAwait(false);
            }

            await CloseSafelyAsync(_channelFactory).ConfigureAwait(false);
        }
    }

    private async Task<TResult> ExecuteAsync<TResult>(Func<IRoomServiceAsync, Task<TResult>> operation)
    {
        IRoomServiceAsync channel = GetOrCreateChannel();
        TResult result;

        try
        {
            result = await operation(channel).ConfigureAwait(false);
        }
        catch (CommunicationException exception) when (exception is not FaultException)
        {
            AbortChannel(channel);
            throw;
        }
        catch (TimeoutException)
        {
            AbortChannel(channel);
            throw;
        }

        return result;
    }

    private async Task ExecuteAsync(Func<IRoomServiceAsync, Task> operation)
    {
        await ExecuteAsync(async channel =>
        {
            await operation(channel).ConfigureAwait(false);
            return true;
        }).ConfigureAwait(false);
    }

    private IRoomServiceAsync GetOrCreateChannel()
    {
        IRoomServiceAsync channel;

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

    private void AbortChannel(IRoomServiceAsync channel)
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

    private static async Task CloseSafelyAsync(ICommunicationObject communicationObject)
    {
        try
        {
            await Task.Factory
                .FromAsync(communicationObject.BeginClose, communicationObject.EndClose, null)
                .ConfigureAwait(false);
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
    private void OnPlayerJoined(object? sender, RoomMember player)
    {
        lock (_eventLock)
        {
            if (_playerJoined is null)
            {
                _pendingEvents.Enqueue(() => _playerJoined?.Invoke(this, player));
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
                _pendingEvents.Enqueue(() => _playerLeft?.Invoke(this, playerId));
            }
            else
            {
                _playerLeft.Invoke(this, playerId);
            }
        }
    }
}