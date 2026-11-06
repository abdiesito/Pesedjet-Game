#nullable enable

using System;
using System.Collections.Generic;
using System.ServiceModel;
using Pesedjet.Server.Contracts.Callbacks;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Contracts.Interfaces;

// Request-reply operations block the caller until the reply arrives, so the client must call them from a worker
// thread. Server-initiated events travel through ILobbyCallback. One-way operations cannot return faults.
[ServiceContract(CallbackContract = typeof(ILobbyCallback), SessionMode = SessionMode.Required)]
public interface ILobbyService
{
    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    string CreateLobby(LobbyMember host, bool isPublic, int maxPlayers);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    LobbyJoinResponse JoinLobby(string lobbyCode, LobbyMember player);

    [OperationContract(IsOneWay = true)]
    void LeaveLobby(string lobbyCode, Guid playerId);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    List<LobbySummary> GetPublicLobbies();
}
