using Pesedjet.Client.UI.Models.DialogParameters;

namespace Pesedjet.Client.UI.Models.Matches;

public sealed record TurnOutcome(
    MatchSnapshot Snapshot,
    MatchNotice? Notice = null,
    RoundSummary? Summary = null,
    MatchResult? Result = null);
