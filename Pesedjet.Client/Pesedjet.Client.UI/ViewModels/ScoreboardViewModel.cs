using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;

namespace Pesedjet.Client.UI.ViewModels;

public partial class ScoreboardViewModel : ViewModelBase, INavigable
{
    private const int PageSize = 20;
    private const int SearchDelayMilliseconds = 300;
    private const string UpdatedAtKey = "Scoreboard.UpdatedAt";
    private const string ShowingKey = "Scoreboard.Showing";
    private const string ShowingOneKey = "Scoreboard.ShowingOne";
    private const string PeriodAllTimeKey = "Scoreboard.PeriodAllTime";
    private const string PeriodThirtyDaysKey = "Scoreboard.PeriodThirtyDays";
    private const string PeriodSevenDaysKey = "Scoreboard.PeriodSevenDays";

    private static readonly TimeSpan SearchDelay = TimeSpan.FromMilliseconds(SearchDelayMilliseconds);

    private readonly INavigator _navigator;
    private readonly TimeProvider _timeProvider;

    private int _lastLoadNumber;
    private ScoreboardPage? _lastPage;
    private bool _isResettingFilters;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ScoreboardPeriodOption _selectedPeriod;

    [ObservableProperty]
    private bool _friendsOnly;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(IsUsernameSorted),
        nameof(IsPointsSorted),
        nameof(IsGamesPlayedSorted),
        nameof(IsVictoriesSorted),
        nameof(IsAveragePerGameSorted),
        nameof(IsLastGameAtSorted))]
    private ScoreboardSortColumn _sortColumn = ScoreboardSortColumn.Points;

    [ObservableProperty]
    private bool _isSortDescending = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand), nameof(NextPageCommand))]
    private int _pageNumber = 1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand), nameof(NextPageCommand))]
    private int _pageCount = 1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoToMyPositionCommand))]
    private int? _ownPageNumber;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowsRows), nameof(ShowsNoGames), nameof(ShowsNoMatches))]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowsNoGames), nameof(ShowsNoMatches))]
    private bool _hasAnyGames = true;

    [ObservableProperty]
    private bool _hasLoadError;

    [ObservableProperty]
    private string _updatedText = string.Empty;

    [ObservableProperty]
    private string _showingText = string.Empty;

    [ObservableProperty]
    private string _pageText = string.Empty;

    public ObservableCollection<ScoreboardEntry> Entries { get; } = new();

    public IReadOnlyList<ScoreboardPeriodOption> Periods { get; }

    public bool ShowsTopBar => true;

    public bool IsUsernameSorted => SortColumn == ScoreboardSortColumn.Username;

    public bool IsPointsSorted => SortColumn == ScoreboardSortColumn.Points;

    public bool IsGamesPlayedSorted => SortColumn == ScoreboardSortColumn.GamesPlayed;

    public bool IsVictoriesSorted => SortColumn == ScoreboardSortColumn.Victories;

    public bool IsAveragePerGameSorted => SortColumn == ScoreboardSortColumn.AveragePerGame;

    public bool IsLastGameAtSorted => SortColumn == ScoreboardSortColumn.LastGameAt;

    public bool ShowsRows => !IsLoading && Entries.Count > 0;

    public bool ShowsNoGames => !IsLoading && !HasAnyGames;

    public bool ShowsNoMatches => !IsLoading && HasAnyGames && Entries.Count == 0;

    public ScoreboardViewModel(INavigator navigator, TimeProvider timeProvider)
    {
        _navigator = navigator;
        _timeProvider = timeProvider;
        Periods =
        [
            BuildPeriodOption(ScoreboardPeriod.AllTime, PeriodAllTimeKey),
            BuildPeriodOption(ScoreboardPeriod.LastThirtyDays, PeriodThirtyDaysKey),
            BuildPeriodOption(ScoreboardPeriod.LastSevenDays, PeriodSevenDaysKey)
        ];
        _selectedPeriod = Periods[0];
    }

    public void OnNavigatedTo()
    {
        _ = LoadAsync(false);
    }

    partial void OnSearchTextChanged(string value)
    {
        if (!_isResettingFilters)
        {
            PageNumber = 1;
            _ = LoadAsync(true);
        }
    }

    partial void OnSelectedPeriodChanged(ScoreboardPeriodOption value)
    {
        ReloadFromFirstPage();
    }

    partial void OnFriendsOnlyChanged(bool value)
    {
        ReloadFromFirstPage();
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigator.GoBack();
    }

    [RelayCommand]
    private void Refresh()
    {
        _ = LoadAsync(false);
    }

    [RelayCommand]
    private void SearchNow()
    {
        PageNumber = 1;
        _ = LoadAsync(false);
    }

    [RelayCommand]
    private void SortBy(ScoreboardSortColumn column)
    {
        if (SortColumn == column)
        {
            IsSortDescending = !IsSortDescending;
        }
        else
        {
            SortColumn = column;
            IsSortDescending = column != ScoreboardSortColumn.Username;
        }

        ReloadFromFirstPage();
    }

    [RelayCommand]
    private void ClearFilters()
    {
        _isResettingFilters = true;
        SearchText = string.Empty;
        SelectedPeriod = Periods[0];
        FriendsOnly = false;
        SortColumn = ScoreboardSortColumn.Points;
        IsSortDescending = true;
        _isResettingFilters = false;

        ReloadFromFirstPage();
    }

    [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
    private void PreviousPage()
    {
        PageNumber--;
        _ = LoadAsync(false);
    }

    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private void NextPage()
    {
        PageNumber++;
        _ = LoadAsync(false);
    }

    [RelayCommand(CanExecute = nameof(CanGoToMyPosition))]
    private void GoToMyPosition()
    {
        PageNumber = OwnPageNumber ?? 1;
        _ = LoadAsync(false);
    }

    [RelayCommand]
    private void Play()
    {
        // TODO: Navigate to the game menu (GUI-006) when it exists.
    }

    private bool CanGoToPreviousPage()
    {
        return PageNumber > 1;
    }

    private bool CanGoToNextPage()
    {
        return PageNumber < PageCount;
    }

    private bool CanGoToMyPosition()
    {
        return OwnPageNumber is not null;
    }

    private static ScoreboardPeriodOption BuildPeriodOption(ScoreboardPeriod period, string key)
    {
        return new ScoreboardPeriodOption(period, key);
    }

    private static Task<ScoreboardPage> LoadFromServerAsync(ScoreboardQuery query)
    {
        // TODO: Ask the server for the scoreboard (CU-011, CU-021, CU-022): official position of the period, sorting,
        // search, friends only and paging. Wrap communication failures (EX-01) to set HasLoadError and keep the rows.
        ScoreboardPage page = new([], 0, 1, null, false, DateTimeOffset.UtcNow);

        return Task.FromResult(page);
    }

    private void ReloadFromFirstPage()
    {
        if (!_isResettingFilters)
        {
            PageNumber = 1;
            _ = LoadAsync(false);
        }
    }
    
    private async Task LoadAsync(bool waitForTyping)
    {
        int loadNumber = ++_lastLoadNumber;

        IsLoading = true;
        HasLoadError = false;

        if (waitForTyping)
        {
            await Task.Delay(SearchDelay, _timeProvider);
        }

        if (loadNumber == _lastLoadNumber)
        {
            ScoreboardPage page = await LoadFromServerAsync(BuildQuery());

            if (loadNumber == _lastLoadNumber)
            {
                ApplyPage(page);
                IsLoading = false;
            }
        }
    }

    private ScoreboardQuery BuildQuery()
    {
        return new ScoreboardQuery(
            SearchText.Trim(),
            SelectedPeriod.Period,
            FriendsOnly,
            SortColumn,
            IsSortDescending,
            PageNumber,
            PageSize);
    }

    private void ApplyPage(ScoreboardPage page)
    {
        _lastPage = page;
        Entries.Clear();

        foreach (ScoreboardEntry entry in page.Entries)
        {
            Entries.Add(entry);
        }

        PageCount = Math.Max(1, page.PageCount);
        OwnPageNumber = page.OwnPageNumber;
        HasAnyGames = page.HasAnyGames;
        UpdatedText = string.Format(
            CultureInfo.CurrentCulture,
            LocalizationManager.Instance[UpdatedAtKey],
            page.UpdatedAt.ToLocalTime().ToString("g", CultureInfo.CurrentCulture));
        PageText = string.Format(CultureInfo.CurrentCulture, "{0} / {1}", PageNumber, PageCount);
        ShowingText = BuildShowingText(page);

        OnPropertyChanged(nameof(ShowsRows));
        OnPropertyChanged(nameof(ShowsNoMatches));
    }
    
    protected override void OnLanguageChanged()
    {
        foreach (ScoreboardPeriodOption period in Periods)
        {
            period.Refresh();
        }

        if (_lastPage is not null)
        {
            ScoreboardPage page = _lastPage;

            ApplyPage(page);
        }

        base.OnLanguageChanged();
    }

    private string BuildShowingText(ScoreboardPage page)
    {
        string text = string.Empty;

        if (page.Entries.Count > 0)
        {
            int first = ((PageNumber - 1) * PageSize) + 1;
            int last = first + page.Entries.Count - 1;
            string key = page.TotalPlayers == 1 ? ShowingOneKey : ShowingKey;

            text = string.Format(
                CultureInfo.CurrentCulture,
                LocalizationManager.Instance[key],
                first,
                last,
                page.TotalPlayers);
        }

        return text;
    }
}
