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

    Task<string> CreateRoomAsync(RoomMember host, bool isPublic, int maxPlayers);

    Task<RoomJoinResponse> JoinRoomAsync(string roomCode, RoomMember player);

    Task LeaveRoomAsync(string roomCode, Guid playerId);

    Task<List<RoomSummary>> GetPublicRoomsAsync();

    void ReplayPendingEvents();
}