using System.Collections.Generic;

namespace Pesedjet.Client.UI.Models;

public sealed record MatchResult(
    string WinnerName,
    int WinningRound,
    int AmuletGoal,
    IReadOnlyList<MatchResultRow> Rows);
