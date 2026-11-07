#nullable enable

using Pesedjet.Server.Contracts.Callbacks;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Matchmaking.Lobbies;

public sealed class LobbyConnection
{
    public LobbyConnection(LobbyMember member, ILobbyCallback callback, string sessionId)
    {
        Member = member;
        Callback = callback;
        SessionId = sessionId;
    }

    public string SessionId { get; }

    public LobbyMember Member { get; }

    public ILobbyCallback Callback { get; }
}