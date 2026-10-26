namespace Pesedjet.Client.UI.Models;

public sealed record DeityCard(
    int Value,
    string Name,
    CardMetal Metal,
    int Copies,
    string IconData,
    bool NeedsTarget,
    bool NeedsNamedDeity);
