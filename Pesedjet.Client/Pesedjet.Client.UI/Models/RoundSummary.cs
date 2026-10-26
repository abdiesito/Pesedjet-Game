using System.Collections.Generic;

namespace Pesedjet.Client.UI.Models;

public sealed record RoundSummary(
    int Round,
    LocalizedText Cause,
    IReadOnlyList<RoundSummaryRow> Rows,
    LocalizedText NextRoundNote);
