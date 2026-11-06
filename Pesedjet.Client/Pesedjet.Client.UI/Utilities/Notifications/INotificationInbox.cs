using System.Collections.ObjectModel;
using System;
using Pesedjet.Client.UI.Models.Social;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.UI.Utilities.Notifications;

public interface INotificationInbox
{
    ObservableCollection<NotificationItem> Items { get; }

    void RefreshFriendRequests();

    void AddFriendRequest(FriendRequestEntry entry);

    void RemoveFriendRequest(Guid senderId);

    void RefreshLobbyInvitations();

    void AddLobbyInvitation(LobbyInvitationEntry entry);

    void RemoveLobbyInvitation(string lobbyCode);
}
