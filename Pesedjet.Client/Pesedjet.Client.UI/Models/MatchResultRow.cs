using System;

namespace Pesedjet.Client.UI.Models;

public sealed record MatchResultRow(
    int Place,
    string Username,
    int? AvatarId,
    int Amulets,
    int AmuletGoal,
    int Points,
    LocalizedText? Withdrawn,
    bool IsCurrentPlayer,
    bool IsWinner)
{
    private const int InitialsLength = 2;

    public string Initials => Username[..Math.Min(InitialsLength, Username.Length)];

    public string? WithdrawnReason => Withdrawn?.Resolve();

    public bool IsWithdrawn => Withdrawn is not null;
}
