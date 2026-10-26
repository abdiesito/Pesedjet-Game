using System;
using System.Collections.Generic;

namespace Pesedjet.Client.UI.Models;

public sealed record ScoreboardPage(
    IReadOnlyList<ScoreboardEntry> Entries,
    int TotalPlayers,
    int PageCount,
    int? OwnPageNumber,
    bool HasAnyGames,
    DateTimeOffset UpdatedAt);
