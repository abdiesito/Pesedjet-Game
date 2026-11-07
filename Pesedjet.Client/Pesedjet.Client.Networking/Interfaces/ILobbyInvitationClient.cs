using Pesedjet.Client.Networking.Results;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.Networking.Interfaces;

public interface ILobbyInvitationClient
{
    ClientResult<List<FriendEntry>> GetInvitableFriends(InvitableFriendsRequest request);

    ClientResult<List<Guid>> SendLobbyInvitations(LobbyInvitationRequest request);

    ClientResult<List<LobbyInvitationEntry>> GetPendingLobbyInvitations(Guid playerId);

    ClientResult<bool> DeclineLobbyInvitation(DeclineLobbyInvitationRequest request);
}
