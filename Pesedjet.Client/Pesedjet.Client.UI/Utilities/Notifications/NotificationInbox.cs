using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Client.UI.Models.Social;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Threading;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.UI.Utilities.Notifications;

public sealed class NotificationInbox : INotificationInbox
{
    private readonly IFriendshipClient _friendshipClient;
    private readonly ILobbyInvitationClient _lobbyInvitationClient;
    private readonly IPlayerSession _playerSession;
    private readonly TimeProvider _timeProvider;

    private int _lastRefreshNumber;
    private int _lastInvitationRefreshNumber;

    public ObservableCollection<NotificationItem> Items { get; } = new();

    public NotificationInbox(NotificationInboxDependencies dependencies)
    {
        _friendshipClient = dependencies.FriendshipClient;
        _lobbyInvitationClient = dependencies.LobbyInvitationClient;
        _playerSession = dependencies.PlayerSession;
        _timeProvider = dependencies.TimeProvider;
    }

    public void RefreshFriendRequests()
    {
        int refreshNumber = ++_lastRefreshNumber;
        bool canHaveFriendRequests = _playerSession.CurrentPlayer is not null
                                     && !_playerSession.IsGuest
                                     && _playerSession.PlayerId != Guid.Empty;

        if (canHaveFriendRequests)
        {
            FriendRequestRefresh refresh = new(_playerSession.PlayerId, refreshNumber);

            BackgroundJob<FriendRequestRefresh, ClientResult<List<FriendRequestEntry>>> job = new()
            {
                Work = GetPendingFriendRequestsOnWorker,
                State = refresh,
                OnCompleted = OnRefreshCompleted,
                OnFailed = OnRefreshFailed
            };

            BackgroundWork.Start(job);
        }
        else
        {
            ApplyFriendRequests(new List<FriendRequestEntry>());
        }
    }

    public void RefreshLobbyInvitations()
    {
        int refreshNumber = ++_lastInvitationRefreshNumber;
        bool canHaveInvitations = _playerSession.CurrentPlayer is not null
                                  && !_playerSession.IsGuest
                                  && _playerSession.PlayerId != Guid.Empty;

        if (canHaveInvitations)
        {
            LobbyInvitationRefresh refresh = new(_playerSession.PlayerId, refreshNumber);

            BackgroundJob<LobbyInvitationRefresh, ClientResult<List<LobbyInvitationEntry>>> job = new()
            {
                Work = GetPendingLobbyInvitationsOnWorker,
                State = refresh,
                OnCompleted = OnInvitationRefreshCompleted,
                OnFailed = OnInvitationRefreshFailed
            };

            BackgroundWork.Start(job);
        }
        else
        {
            ApplyLobbyInvitations(new List<LobbyInvitationEntry>());
        }
    }

    public void AddLobbyInvitation(LobbyInvitationEntry entry)
    {
        bool isShown = Items.Any(item => item.IsLobbyInvitation && item.LobbyCode == entry.LobbyCode);

        if (!isShown)
        {
            Items.Add(CreateLobbyInvitationItem(entry));
        }
    }

    public void RemoveLobbyInvitation(string lobbyCode)
    {
        List<NotificationItem> staleItems = Items
            .Where(item => item.IsLobbyInvitation && item.LobbyCode == lobbyCode)
            .ToList();

        foreach (NotificationItem staleItem in staleItems)
        {
            Items.Remove(staleItem);
        }
    }

    public void AddFriendRequest(FriendRequestEntry entry)
    {
        bool isShown = Items.Any(item => item.IsFriendRequest && item.Id == entry.SenderId);

        if (!isShown)
        {
            Items.Add(CreateFriendRequestItem(entry));
        }
    }

    public void RemoveFriendRequest(Guid senderId)
    {
        List<NotificationItem> answeredItems = Items
            .Where(item => item.IsFriendRequest && item.Id == senderId)
            .ToList();

        foreach (NotificationItem answeredItem in answeredItems)
        {
            Items.Remove(answeredItem);
        }
    }

    private ClientResult<List<LobbyInvitationEntry>> GetPendingLobbyInvitationsOnWorker(
        LobbyInvitationRefresh refresh)
    {
        return _lobbyInvitationClient.GetPendingLobbyInvitations(refresh.PlayerId);
    }

    private void OnInvitationRefreshCompleted(
        ClientResult<List<LobbyInvitationEntry>> result,
        LobbyInvitationRefresh refresh)
    {
        if (refresh.RefreshNumber == _lastInvitationRefreshNumber && result.IsSuccess)
        {
            ApplyLobbyInvitations(result.Value!);
        }
    }

    private void OnInvitationRefreshFailed(Exception error, LobbyInvitationRefresh refresh)
    {
    }

    private void ApplyLobbyInvitations(List<LobbyInvitationEntry> entries)
    {
        List<NotificationItem> staleItems = Items
            .Where(item => item.IsLobbyInvitation && entries.All(entry => entry.LobbyCode != item.LobbyCode))
            .ToList();

        foreach (NotificationItem staleItem in staleItems)
        {
            Items.Remove(staleItem);
        }

        foreach (LobbyInvitationEntry entry in entries)
        {
            AddLobbyInvitation(entry);
        }
    }

    private NotificationItem CreateLobbyInvitationItem(LobbyInvitationEntry entry)
    {
        return new NotificationItem
        {
            Id = entry.SenderId,
            Kind = NotificationKind.LobbyInvitation,
            SenderName = entry.SenderUsername,
            ReceivedAt = _timeProvider.GetUtcNow(),
            IsNew = true,
            LobbyCode = entry.LobbyCode,
            Occupancy = entry.CurrentPlayers,
            Capacity = entry.MaxPlayers
        };
    }

    private ClientResult<List<FriendRequestEntry>> GetPendingFriendRequestsOnWorker(FriendRequestRefresh refresh)
    {
        return _friendshipClient.GetPendingFriendRequests(refresh.PlayerId);
    }

    private void OnRefreshCompleted(ClientResult<List<FriendRequestEntry>> result, FriendRequestRefresh refresh)
    {
        if (refresh.RefreshNumber == _lastRefreshNumber && result.IsSuccess)
        {
            ApplyFriendRequests(result.Value!);
        }
    }

    private void OnRefreshFailed(Exception error, FriendRequestRefresh refresh)
    {
    }

    private void ApplyFriendRequests(List<FriendRequestEntry> entries)
    {
        List<NotificationItem> staleItems = Items
            .Where(item => item.IsFriendRequest && entries.All(entry => entry.SenderId != item.Id))
            .ToList();

        foreach (NotificationItem staleItem in staleItems)
        {
            Items.Remove(staleItem);
        }

        foreach (FriendRequestEntry entry in entries)
        {
            AddFriendRequest(entry);
        }
    }

    private NotificationItem CreateFriendRequestItem(FriendRequestEntry entry)
    {
        return new NotificationItem
        {
            Id = entry.SenderId,
            Kind = NotificationKind.FriendRequest,
            SenderName = entry.SenderUsername,
            ReceivedAt = _timeProvider.GetUtcNow(),
            IsNew = true
        };
    }
}
