using Pesedjet.Server.Contracts.Callbacks;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Contracts.Internal;

public interface IFriendshipNotifier
{
    void Subscribe(Guid playerId, IFriendshipCallback callback, string sessionId);

    void Unsubscribe(Guid playerId);

    void UnsubscribeSession(string sessionId);

    void NotifyFriendRequestReceived(Guid targetPlayerId, FriendRequestEntry request);

    void NotifyFriendshipEstablished(Guid targetPlayerId, FriendEntry friend);
}
