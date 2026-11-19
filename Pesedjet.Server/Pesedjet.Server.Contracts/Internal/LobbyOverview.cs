using Pesedjet.Server.Contracts.DTO;

namespace Pesedjet.Server.Contracts.Internal;

public sealed record LobbyOverview(
    string LobbyCode,
    Guid HostPlayerId,
    int MaxPlayers,
    bool IsWaiting,
    List<LobbyMember> Members,
    Dictionary<Guid, Guid> Invitations);
