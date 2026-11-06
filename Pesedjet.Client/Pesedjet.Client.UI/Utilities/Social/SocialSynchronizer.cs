using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Avalonia.Threading;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Client.UI.Models.Social;
using Pesedjet.Client.UI.Utilities.Notifications;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Threading;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.UI.Utilities.Social;

public sealed class SocialSynchronizer : ISocialSynchronizer
{
    private readonly IFriendshipClient _friendshipClient;
    private readonly IFriendshipNotificationClient _notificationClient;
    private readonly IPlayerSession _playerSession;
    private readonly ISocialCircle _socialCircle;
    private readonly INotificationInbox _inbox;
    private readonly ConcurrentQueue<PushedSocialEvent> _pendingEvents = new();

    private Guid _activePlayerId;
    private int _lastLoadNumber;

    public SocialSynchronizer(SocialSynchronizerDependencies dependencies)
    {
        _friendshipClient = dependencies.FriendshipClient;
        _notificationClient = dependencies.NotificationClient;
        _playerSession = dependencies.PlayerSession;
        _socialCircle = dependencies.SocialCircle;
        _inbox = dependencies.Inbox;

        _notificationClient.FriendRequestReceived += OnFriendRequestReceived;
        _notificationClient.FriendshipEstablished += OnFriendshipEstablished;
        _notificationClient.LobbyInvitationReceived += OnLobbyInvitationReceived;
        _notificationClient.LobbyInvitationWithdrawn += OnLobbyInvitationWithdrawn;
    }

    public void Synchronize()
    {
        bool isSignedIn = _playerSession.CurrentPlayer is not null
                          && !_playerSession.IsGuest
                          && _playerSession.PlayerId != Guid.Empty;
        Guid currentPlayerId = isSignedIn ? _playerSession.PlayerId : Guid.Empty;

        if (currentPlayerId != _activePlayerId)
        {
            SubscriptionChange change = new(_activePlayerId, currentPlayerId);
            _activePlayerId = currentPlayerId;
            _socialCircle.Friends.Clear();

            StartSubscriptionChange(change);
            StartFriendListLoad(currentPlayerId);
        }

        _inbox.RefreshFriendRequests();
        _inbox.RefreshLobbyInvitations();
    }

    private void StartSubscriptionChange(SubscriptionChange change)
    {
        BackgroundAction<SubscriptionChange> action = new()
        {
            Work = ChangeSubscriptionOnWorker,
            State = change
        };

        BackgroundWork.Start(action);
    }

    private void ChangeSubscriptionOnWorker(SubscriptionChange change)
    {
        if (change.PreviousPlayerId != Guid.Empty)
        {
            _notificationClient.Unsubscribe(change.PreviousPlayerId);
        }

        if (change.CurrentPlayerId != Guid.Empty)
        {
            _notificationClient.Subscribe(change.CurrentPlayerId);
        }
    }

    private void StartFriendListLoad(Guid playerId)
    {
        int loadNumber = ++_lastLoadNumber;

        if (playerId != Guid.Empty)
        {
            FriendListLoad load = new(playerId, loadNumber);

            BackgroundJob<FriendListLoad, ClientResult<List<FriendEntry>>> job = new()
            {
                Work = GetFriendsOnWorker,
                State = load,
                OnCompleted = OnFriendListLoaded,
                OnFailed = OnFriendListFailed
            };

            BackgroundWork.Start(job);
        }
    }

    private ClientResult<List<FriendEntry>> GetFriendsOnWorker(FriendListLoad load)
    {
        return _friendshipClient.GetFriends(load.PlayerId);
    }

    private void OnFriendListLoaded(ClientResult<List<FriendEntry>> result, FriendListLoad load)
    {
        if (load.LoadNumber == _lastLoadNumber && result.IsSuccess)
        {
            foreach (FriendEntry friend in result.Value!)
            {
                _socialCircle.AddFriend(friend.PlayerId, friend.Username);
            }
        }
    }

    private void OnFriendListFailed(Exception error, FriendListLoad load)
    {
    }

    private void OnFriendRequestReceived(object? sender, FriendRequestEntry request)
    {
        _pendingEvents.Enqueue(new PushedSocialEvent(request, null, null, null));
        Dispatcher.UIThread.Post(ApplyPendingEvents);
    }

    private void OnFriendshipEstablished(object? sender, FriendEntry friend)
    {
        _pendingEvents.Enqueue(new PushedSocialEvent(null, friend, null, null));
        Dispatcher.UIThread.Post(ApplyPendingEvents);
    }

    private void OnLobbyInvitationReceived(object? sender, LobbyInvitationEntry invitation)
    {
        _pendingEvents.Enqueue(new PushedSocialEvent(null, null, invitation, null));
        Dispatcher.UIThread.Post(ApplyPendingEvents);
    }

    private void OnLobbyInvitationWithdrawn(object? sender, string lobbyCode)
    {
        _pendingEvents.Enqueue(new PushedSocialEvent(null, null, null, lobbyCode));
        Dispatcher.UIThread.Post(ApplyPendingEvents);
    }

    private void ApplyPendingEvents()
    {
        while (_pendingEvents.TryDequeue(out PushedSocialEvent? pushedEvent))
        {
            if (pushedEvent.FriendRequest is not null)
            {
                _inbox.AddFriendRequest(pushedEvent.FriendRequest);
            }

            if (pushedEvent.Friend is not null)
            {
                _socialCircle.AddFriend(pushedEvent.Friend.PlayerId, pushedEvent.Friend.Username);
                _inbox.RemoveFriendRequest(pushedEvent.Friend.PlayerId);
            }

            if (pushedEvent.LobbyInvitation is not null)
            {
                _inbox.AddLobbyInvitation(pushedEvent.LobbyInvitation);
            }

            if (pushedEvent.WithdrawnLobbyCode is not null)
            {
                _inbox.RemoveLobbyInvitation(pushedEvent.WithdrawnLobbyCode);
            }
        }
    }
}
