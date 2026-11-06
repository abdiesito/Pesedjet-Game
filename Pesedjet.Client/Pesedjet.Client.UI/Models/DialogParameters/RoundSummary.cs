using System.Collections.Generic;
using Pesedjet.Client.UI.Models.Common;
using Pesedjet.Client.UI.Models.Matches;

namespace Pesedjet.Client.UI.Models.DialogParameters;

public sealed record RoundSummary(
    int Round,
    LocalizedText Cause,
    IReadOnlyList<RoundSummaryRow> Rows,
    LocalizedText NextRoundNote);
