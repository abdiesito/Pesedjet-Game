#nullable enable

using Pesedjet.Server.Contracts.Callbacks;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Matchmaking.Rooms;

public sealed class RoomConnection
{
    public RoomConnection(RoomMember member, IRoomCallback callback, string sessionId)
    {
        Member = member;
        Callback = callback;
        SessionId = sessionId;
    }

    public string SessionId { get; }

    public RoomMember Member { get; }

    public IRoomCallback Callback { get; }
}