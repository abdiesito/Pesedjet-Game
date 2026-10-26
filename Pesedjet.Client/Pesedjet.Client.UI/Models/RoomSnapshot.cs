using System.Collections.Generic;

namespace Pesedjet.Client.UI.Models;

public sealed record RoomSnapshot(
    string Code,
    int Capacity,
    RoomVisibility Visibility,
    bool IsHost,
    IReadOnlyList<RoomMember> Members,
    IReadOnlyList<RoomChatEntry> Chat);
