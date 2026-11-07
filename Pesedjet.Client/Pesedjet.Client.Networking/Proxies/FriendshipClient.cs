using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Endpoints;
using Pesedjet.Server.Contracts.Interfaces;

namespace Pesedjet.Client.Networking.Proxies;

public sealed class FriendshipClient : IFriendshipClient
{
    private readonly IServiceCaller _serviceCaller;

    public FriendshipClient(IServiceCaller serviceCaller)
    {
        _serviceCaller = serviceCaller;
    }

    public ClientResult<List<FriendEntry>> GetFriends(Guid playerId)
    {
        return _serviceCaller.Call<IFriendshipService, Guid, List<FriendEntry>>(
            ServiceEndpoints.FriendshipServiceAddress,
            GetFriendsOperation,
            playerId);
    }

    public ClientResult<List<PlayerSearchEntry>> SearchPlayers(PlayerSearchRequest request)
    {
        return _serviceCaller.Call<IFriendshipService, PlayerSearchRequest, List<PlayerSearchEntry>>(
            ServiceEndpoints.FriendshipServiceAddress,
            SearchPlayersOperation,
            request);
    }

    public ClientResult<PlayerRelation> SendFriendRequest(FriendRequest request)
    {
        return _serviceCaller.Call<IFriendshipService, FriendRequest, PlayerRelation>(
            ServiceEndpoints.FriendshipServiceAddress,
            SendFriendRequestOperation,
            request);
    }

    public ClientResult<List<FriendRequestEntry>> GetPendingFriendRequests(Guid playerId)
    {
        return _serviceCaller.Call<IFriendshipService, Guid, List<FriendRequestEntry>>(
            ServiceEndpoints.FriendshipServiceAddress,
            GetPendingFriendRequestsOperation,
            playerId);
    }

    public ClientResult<bool> RespondFriendRequest(RespondFriendRequest request)
    {
        return _serviceCaller.Call<IFriendshipService, RespondFriendRequest, bool>(
            ServiceEndpoints.FriendshipServiceAddress,
            RespondFriendRequestOperation,
            request);
    }

    private static List<FriendEntry> GetFriendsOperation(IFriendshipService channel, Guid playerId)
    {
        return channel.GetFriends(playerId);
    }

    private static List<PlayerSearchEntry> SearchPlayersOperation(
        IFriendshipService channel,
        PlayerSearchRequest request)
    {
        return channel.SearchPlayers(request);
    }

    private static PlayerRelation SendFriendRequestOperation(IFriendshipService channel, FriendRequest request)
    {
        return channel.SendFriendRequest(request);
    }

    private static List<FriendRequestEntry> GetPendingFriendRequestsOperation(
        IFriendshipService channel,
        Guid playerId)
    {
        return channel.GetPendingFriendRequests(playerId);
    }

    private static bool RespondFriendRequestOperation(IFriendshipService channel, RespondFriendRequest request)
    {
        return channel.RespondFriendRequest(request);
    }
}
