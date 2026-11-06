using System.ServiceModel;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Contracts.Callbacks;

[ServiceContract]
public interface IFriendshipCallback
{
    [OperationContract(IsOneWay = true)]
    void OnFriendRequestReceived(FriendRequestEntry request);

    [OperationContract(IsOneWay = true)]
    void OnFriendshipEstablished(FriendEntry friend);
}
