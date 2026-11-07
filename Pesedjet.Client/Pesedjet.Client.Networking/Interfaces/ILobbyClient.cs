#nullable enable

using System;
using System.Collections.Generic;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.Networking.Interfaces;

public interface ILobbyClient : IDisposable
{
    event EventHandler<LobbyMember>? PlayerJoined;

    event EventHandler<Guid>? PlayerLeft;

    string CreateLobby(LobbyMember host, bool isPublic, int maxPlayers);

    LobbyJoinResponse JoinLobby(string lobbyCode, LobbyMember player);

    void LeaveLobby(string lobbyCode, Guid playerId);

    List<LobbySummary> GetPublicLobbies();

    void ReplayPendingEvents();
}