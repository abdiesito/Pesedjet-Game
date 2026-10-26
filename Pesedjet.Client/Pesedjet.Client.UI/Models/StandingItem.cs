using System;
using System.Collections.Generic;

namespace Pesedjet.Client.UI.Models;

public sealed class StandingItem
{
    private const int InitialsLength = 2;

    public required string Username { get; init; }

    public required int Points { get; init; }

    public string PointsText { get; init; } = string.Empty;

    public required bool IsCurrentPlayer { get; init; }

    public IReadOnlyList<bool> AmuletSlots { get; init; } = [];

    public string Initials => Username[..Math.Min(InitialsLength, Username.Length)];
}
