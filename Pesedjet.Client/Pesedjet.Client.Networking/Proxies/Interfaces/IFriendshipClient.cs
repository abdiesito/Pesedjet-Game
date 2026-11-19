using Pesedjet.Client.Networking.Results;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.Networking.Proxies.Interfaces;

public interface IFriendshipClient
{
    ClientResult<List<FriendEntry>> GetFriends(Guid playerId);

    ClientResult<List<PlayerSearchEntry>> SearchPlayers(PlayerSearchRequest request);

    ClientResult<PlayerRelation> SendFriendRequest(FriendRequest request);

    ClientResult<List<FriendRequestEntry>> GetPendingFriendRequests(Guid playerId);

    ClientResult<bool> RespondFriendRequest(RespondFriendRequest request);
}
