using Pesedjet.Server.DataAccess.Entities.Identity;

namespace Pesedjet.Server.DataAccess.Repositories.Interfaces;

public interface IFriendshipRepository
{
    Friendship? GetFriendshipBetween(Guid playerId, Guid otherPlayerId);

    List<Friendship> GetFriendshipsBetween(Guid playerId, List<Guid> otherPlayerIds);

    List<Friendship> GetAcceptedFriendships(Guid playerId);

    List<Friendship> GetPendingRequestsReceivedBy(Guid addresseeId);

    bool CreatePendingFriendship(Guid requesterId, Guid addresseeId);

    bool AcceptPendingFriendship(Guid requesterId, Guid addresseeId);

    bool DeletePendingFriendship(Guid requesterId, Guid addresseeId);
}
