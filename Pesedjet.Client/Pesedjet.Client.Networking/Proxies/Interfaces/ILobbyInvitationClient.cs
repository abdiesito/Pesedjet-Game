using Pesedjet.Client.Networking.Results;
using Pesedjet.Server.Contracts.DTO;

namespace Pesedjet.Client.Networking.Proxies.Interfaces;

public interface ILobbyInvitationClient
{
    ClientResult<List<FriendEntry>> GetInvitableFriends(InvitableFriendsRequest request);

    ClientResult<List<Guid>> SendLobbyInvitations(LobbyInvitation lobbyInvitation);

    ClientResult<List<LobbyInvitationEntry>> GetPendingLobbyInvitations(Guid playerId);

    ClientResult<bool> DeclineLobbyInvitation(DeclineLobbyInvitationRequest request);
}
