using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.Networking.Interfaces;

public interface IFriendshipNotificationClient : IDisposable
{
    event EventHandler<FriendRequestEntry>? FriendRequestReceived;

    event EventHandler<FriendEntry>? FriendshipEstablished;

    event EventHandler<LobbyInvitationEntry>? LobbyInvitationReceived;

    event EventHandler<string>? LobbyInvitationWithdrawn;

    void Subscribe(Guid playerId);

    void Unsubscribe(Guid playerId);
}
