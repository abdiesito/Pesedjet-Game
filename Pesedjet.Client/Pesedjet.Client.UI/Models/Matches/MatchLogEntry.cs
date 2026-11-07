using Pesedjet.Client.UI.Models.Common;

namespace Pesedjet.Client.UI.Models.Matches;

public sealed record MatchLogEntry(MatchLogKind Kind, LocalizedText Message)
{
    public string Text => Message.Resolve();

    public bool IsNormal => Kind == MatchLogKind.Normal;

    public bool IsGain => Kind == MatchLogKind.Gain;

    public bool IsLoss => Kind == MatchLogKind.Loss;

    public bool IsHint => Kind == MatchLogKind.Hint;
}
