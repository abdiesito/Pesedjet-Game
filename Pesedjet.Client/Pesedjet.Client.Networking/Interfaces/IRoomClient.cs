#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.Networking.Interfaces;

public interface IRoomClient : IAsyncDisposable
{
    event EventHandler<RoomMember>? PlayerJoined;

    event EventHandler<Guid>? PlayerLeft;

    Task<string> CreateRoomAsync(Guid hostPlayerId, bool isPublic, int maxPlayers);

    Task JoinRoomAsync(string roomCode, Guid playerId);

    Task LeaveRoomAsync(string roomCode, Guid playerId);

    Task<List<RoomSummary>> GetPublicRoomsAsync();
}