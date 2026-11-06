using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Endpoints;
using Pesedjet.Server.Contracts.Interfaces;

namespace Pesedjet.Client.Networking.Proxies;

public sealed class LobbyInvitationClient : ILobbyInvitationClient
{
    private readonly IServiceCaller _serviceCaller;

    public LobbyInvitationClient(IServiceCaller serviceCaller)
    {
        _serviceCaller = serviceCaller;
    }

    public ClientResult<List<FriendEntry>> GetInvitableFriends(InvitableFriendsRequest request)
    {
        return _serviceCaller.Call<ILobbyInvitationService, InvitableFriendsRequest, List<FriendEntry>>(
            ServiceEndpoints.LobbyInvitationServiceAddress,
            GetInvitableFriendsOperation,
            request);
    }

    public ClientResult<List<Guid>> SendLobbyInvitations(LobbyInvitationRequest request)
    {
        return _serviceCaller.Call<ILobbyInvitationService, LobbyInvitationRequest, List<Guid>>(
            ServiceEndpoints.LobbyInvitationServiceAddress,
            SendLobbyInvitationsOperation,
            request);
    }

    public ClientResult<List<LobbyInvitationEntry>> GetPendingLobbyInvitations(Guid playerId)
    {
        return _serviceCaller.Call<ILobbyInvitationService, Guid, List<LobbyInvitationEntry>>(
            ServiceEndpoints.LobbyInvitationServiceAddress,
            GetPendingLobbyInvitationsOperation,
            playerId);
    }

    public ClientResult<bool> DeclineLobbyInvitation(DeclineLobbyInvitationRequest request)
    {
        return _serviceCaller.Call<ILobbyInvitationService, DeclineLobbyInvitationRequest, bool>(
            ServiceEndpoints.LobbyInvitationServiceAddress,
            DeclineLobbyInvitationOperation,
            request);
    }

    private static List<FriendEntry> GetInvitableFriendsOperation(
        ILobbyInvitationService channel,
        InvitableFriendsRequest request)
    {
        return channel.GetInvitableFriends(request);
    }

    private static List<Guid> SendLobbyInvitationsOperation(
        ILobbyInvitationService channel,
        LobbyInvitationRequest request)
    {
        return channel.SendLobbyInvitations(request);
    }

    private static List<LobbyInvitationEntry> GetPendingLobbyInvitationsOperation(
        ILobbyInvitationService channel,
        Guid playerId)
    {
        return channel.GetPendingLobbyInvitations(playerId);
    }

    private static bool DeclineLobbyInvitationOperation(
        ILobbyInvitationService channel,
        DeclineLobbyInvitationRequest request)
    {
        return channel.DeclineLobbyInvitation(request);
    }
}
