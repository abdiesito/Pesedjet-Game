#nullable enable

using System.ServiceModel;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Contracts.Callbacks;

[ServiceContract]
public interface IRoomCallback
{
    [OperationContract(IsOneWay = true)]
    void PlayerJoined(RoomMember player);

    [OperationContract(IsOneWay = true)]
    void PlayerLeft(Guid playerId);
}