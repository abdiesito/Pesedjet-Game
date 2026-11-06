namespace Pesedjet.Client.UI.Models.Cards;

public sealed record DeityCard(
    int Value,
    string Name,
    CardMetal Metal,
    int Copies,
    string IconData,
    bool NeedsTarget,
    bool NeedsNamedDeity);
