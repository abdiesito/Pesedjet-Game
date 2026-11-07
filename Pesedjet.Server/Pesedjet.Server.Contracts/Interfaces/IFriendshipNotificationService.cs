using System.ServiceModel;
using Pesedjet.Server.Contracts.Callbacks;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Contracts.Interfaces;

[ServiceContract(CallbackContract = typeof(IFriendshipCallback), SessionMode = SessionMode.Required)]
public interface IFriendshipNotificationService
{
    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    bool Subscribe(Guid playerId);

    [OperationContract(IsOneWay = true)]
    void Unsubscribe(Guid playerId);
}
