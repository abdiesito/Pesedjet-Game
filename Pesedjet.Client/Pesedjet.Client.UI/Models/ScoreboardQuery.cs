namespace Pesedjet.Client.UI.Models;

public sealed record ScoreboardQuery(
    string SearchText,
    ScoreboardPeriod Period,
    bool FriendsOnly,
    ScoreboardSortColumn SortColumn,
    bool IsDescending,
    int PageNumber,
    int PageSize);
