using System.ServiceModel;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Contracts.Interfaces;

[ServiceContract]
public interface IFriendshipService
{
    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    List<FriendEntry> GetFriends(Guid playerId);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    List<PlayerSearchEntry> SearchPlayers(PlayerSearchRequest request);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    PlayerRelation SendFriendRequest(FriendRequest request);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    List<FriendRequestEntry> GetPendingFriendRequests(Guid playerId);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    bool RespondFriendRequest(RespondFriendRequest request);
}
