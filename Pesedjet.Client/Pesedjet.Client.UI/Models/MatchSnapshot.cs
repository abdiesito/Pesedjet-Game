using System.Collections.Generic;

namespace Pesedjet.Client.UI.Models;

public sealed record MatchSnapshot(
    int Round,
    int AmuletGoal,
    string TurnOwner,
    bool IsMyTurn,
    IReadOnlyList<PlayerSnapshot> Opponents,
    PlayerSnapshot Me,
    IReadOnlyList<int> Hand,
    int DeckCount,
    bool HasSetAsideCard,
    IReadOnlyList<int> FaceUpCards,
    bool IsTwoPlayerMatch,
    IReadOnlyList<UnseenDeity> Unseen,
    IReadOnlyList<MatchLogEntry> Log);
