#nullable enable

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Pesedjet.Server.Contracts.Callbacks;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Contracts.Interfaces;

[ServiceContract(CallbackContract = typeof(IRoomCallback), SessionMode = SessionMode.Required)]
public interface IRoomService
{
    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    string CreateRoom(RoomMember host, bool isPublic, int maxPlayers);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    Task<RoomJoinResponse> JoinRoomAsync(string roomCode, RoomMember player);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    void LeaveRoom(string roomCode, Guid playerId);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    List<RoomSummary> GetPublicRooms();
}