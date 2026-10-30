#nullable enable

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Pesedjet.Server.Contracts.Callbacks;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.Networking.Interfaces;

[ServiceContract(Name = "IRoomService", CallbackContract = typeof(IRoomCallback), SessionMode = SessionMode.Required)]
public interface IRoomServiceAsync
{
    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    Task<string> CreateRoomAsync(RoomMember host, bool isPublic, int maxPlayers);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    Task<RoomJoinResponse> JoinRoomAsync(string roomCode, RoomMember player);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    Task LeaveRoomAsync(string roomCode, Guid playerId);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    Task<List<RoomSummary>> GetPublicRoomsAsync();
}