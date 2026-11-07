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

    // TODO: evaluate renaming this channel or create a new channel.
    [OperationContract(IsOneWay = true)]
    void OnLobbyInvitationReceived(LobbyInvitationEntry invitation);

    [OperationContract(IsOneWay = true)]
    void OnLobbyInvitationWithdrawn(string lobbyCode);
}
