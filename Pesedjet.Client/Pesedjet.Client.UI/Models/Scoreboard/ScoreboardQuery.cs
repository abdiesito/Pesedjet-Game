namespace Pesedjet.Client.UI.Models.Scoreboard;

public sealed record ScoreboardQuery(
    string SearchText,
    ScoreboardPeriod Period,
    bool FriendsOnly,
    ScoreboardSortColumn SortColumn,
    bool IsDescending,
    int PageNumber,
    int PageSize);
