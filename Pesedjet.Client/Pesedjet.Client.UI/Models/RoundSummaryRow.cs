using System;

namespace Pesedjet.Client.UI.Models;

public sealed record RoundSummaryRow(
    string Username,
    int? AvatarId,
    int? HandValue,
    LocalizedText Result,
    int RoundPoints,
    int Amulets,
    int AmuletGoal,
    int TotalPoints,
    bool IsCurrentPlayer,
    bool IsWinner,
    bool IsOut)
{
    private const int InitialsLength = 2;

    public string Initials => Username[..Math.Min(InitialsLength, Username.Length)];

    public string ResultText => Result.Resolve();

    public bool HasHand => HandValue is not null;

    public string HandText => HandValue?.ToString() ?? string.Empty;

    public bool HasRoundPoints => RoundPoints > 0;

    public string RoundPointsText => "+" + RoundPoints;

    public bool IsNotOut => !IsOut;
}
