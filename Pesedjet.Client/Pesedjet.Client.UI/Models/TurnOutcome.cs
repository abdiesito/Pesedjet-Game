namespace Pesedjet.Client.UI.Models;

public sealed record TurnOutcome(
    MatchSnapshot Snapshot,
    MatchNotice? Notice = null,
    RoundSummary? Summary = null,
    MatchResult? Result = null);
