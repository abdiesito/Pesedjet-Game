using System.Collections.Generic;
using Pesedjet.Client.UI.Models.Matches;

namespace Pesedjet.Client.UI.Models.DialogParameters;

public sealed record MatchResult(
    string WinnerName,
    int WinningRound,
    int AmuletGoal,
    IReadOnlyList<MatchResultRow> Rows);
