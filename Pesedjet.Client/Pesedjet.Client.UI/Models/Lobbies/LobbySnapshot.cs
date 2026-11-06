using System.Collections.Generic;

namespace Pesedjet.Client.UI.Models.Lobbies;

public sealed record LobbySnapshot(
    string Code,
    int Capacity,
    LobbyVisibility Visibility,
    bool IsHost,
    IReadOnlyList<LobbyMember> Members,
    IReadOnlyList<LobbyChatEntry> Chat);
