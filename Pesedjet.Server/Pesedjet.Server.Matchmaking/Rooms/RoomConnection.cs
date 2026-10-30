#nullable enable

using Pesedjet.Server.Contracts.Callbacks;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Matchmaking.Rooms;

public sealed class RoomConnection
{
    public RoomConnection(RoomMember member, IRoomCallback callback)
    {
        Member = member;
        Callback = callback;
    }

    public RoomMember Member { get; }

    public IRoomCallback Callback { get; }
}