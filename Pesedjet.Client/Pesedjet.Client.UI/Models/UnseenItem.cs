using System.Collections.Generic;

namespace Pesedjet.Client.UI.Models;

public sealed class UnseenItem
{
    public required DeityCard Card { get; init; }

    public required int Remaining { get; init; }

    public int Total => Card.Copies;

    public bool IsDepleted => Remaining == 0;

    public string FractionText => $"{Remaining}/{Total}";

    public string Label => $"{Card.Value} {Card.Name}";

    public IReadOnlyList<bool> Slots { get; init; } = [];
}
