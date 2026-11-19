#nullable enable

using System.ServiceModel;
using Pesedjet.Server.Contracts.DTO;

namespace Pesedjet.Server.Contracts.Callbacks;

[ServiceContract]
public interface ILobbyCallback
{
    [OperationContract(IsOneWay = true)]
    void PlayerJoined(LobbyMember player);

    [OperationContract(IsOneWay = true)]
    void PlayerLeft(Guid playerId);
}