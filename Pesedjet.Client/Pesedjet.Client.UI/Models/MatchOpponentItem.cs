using System;
using System.Collections.Generic;

namespace Pesedjet.Client.UI.Models;

public sealed class MatchOpponentItem
{
    private const int InitialsLength = 2;

    public required string Username { get; init; }

    public int? AvatarId { get; init; }

    public int Points { get; init; }

    public string PointsText { get; init; } = string.Empty;

    public int Amulets { get; init; }

    public int AmuletGoal { get; init; }

    public string StatusText { get; init; } = string.Empty;

    public bool IsInTurn { get; init; }

    public bool IsOut { get; init; }

    public bool IsActive => !IsOut;

    public bool HasHiddenCard { get; init; }

    public bool HasStatus => StatusText.Length > 0;

    public IReadOnlyList<int> Discards { get; init; } = [];

    public IReadOnlyList<bool> AmuletSlots { get; init; } = [];

    public string Initials => Username[..Math.Min(InitialsLength, Username.Length)];

    public string? AvatarIconData => AvatarCatalog.Find(AvatarId)?.IconData;

    public bool HasAvatar => AvatarIconData is not null;
}
