using System;

namespace Pesedjet.Client.UI.Models;

public sealed record ScoreboardEntry(
    int Position,
    string Username,
    int Points,
    int GamesPlayed,
    int Victories,
    double AveragePerGame,
    DateTimeOffset LastGameAt,
    bool IsCurrentPlayer,
    int? AvatarId = null)
{
    private const int InitialsLength = 2;

    public string Initials => Username[..Math.Min(InitialsLength, Username.Length)];

    public string? AvatarIconData => AvatarCatalog.Find(AvatarId)?.IconData;

    public bool HasAvatar => AvatarIconData is not null;
}
