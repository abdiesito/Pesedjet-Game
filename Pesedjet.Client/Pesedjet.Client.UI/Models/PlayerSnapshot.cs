using System.Collections.Generic;

namespace Pesedjet.Client.UI.Models;

public sealed record PlayerSnapshot(
    string Username,
    int? AvatarId,
    int Points,
    int Amulets,
    PlayerStatus Status,
    IReadOnlyList<int> Discards,
    bool HasHiddenCard,
    bool IsInTurn);
