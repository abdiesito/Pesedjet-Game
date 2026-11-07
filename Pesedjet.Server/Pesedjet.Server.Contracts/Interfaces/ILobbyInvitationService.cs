using System.ServiceModel;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Contracts.Interfaces;

[ServiceContract]
public interface ILobbyInvitationService
{
    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    List<FriendEntry> GetInvitableFriends(InvitableFriendsRequest request);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    List<Guid> SendLobbyInvitations(LobbyInvitationRequest request);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    List<LobbyInvitationEntry> GetPendingLobbyInvitations(Guid playerId);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    bool DeclineLobbyInvitation(DeclineLobbyInvitationRequest request);
}
