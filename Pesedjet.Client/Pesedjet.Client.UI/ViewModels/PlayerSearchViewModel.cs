using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Toasts;

namespace Pesedjet.Client.UI.ViewModels;

public partial class PlayerSearchViewModel : ViewModelBase
{
    private const int MinimumSearchLength = 3;
    private const int SearchDelayMilliseconds = 300;
    private const string SendKey = "Friends.SendRequest";
    private const string SendingKey = "Friends.Sending";
    private const string NoMatchesTitleKey = "Friends.NoMatchesTitle";
    private const string RequestSentToastKey = "Friends.RequestSentToast";

    private static readonly TimeSpan SearchDelay = TimeSpan.FromMilliseconds(SearchDelayMilliseconds);

    private readonly IToastManager _toastManager;
    private readonly TimeProvider _timeProvider;

    private int _lastSearchNumber;
    private string _lastTerm = string.Empty;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(IsPromptVisible),
        nameof(IsTooShortVisible),
        nameof(IsSearching),
        nameof(HasResults),
        nameof(HasNoResults),
        nameof(HasSearchText))]
    private SearchState _state;

    [ObservableProperty]
    private string _noMatchesTitle = string.Empty;

    public ObservableCollection<PlayerSearchResult> Results { get; } = new();

    public bool IsPromptVisible => State == SearchState.Prompt;

    public bool IsTooShortVisible => State == SearchState.TooShort;

    public bool IsSearching => State == SearchState.Searching;

    public bool HasResults => State == SearchState.Results;

    public bool HasNoResults => State == SearchState.NoResults;

    public bool HasSearchText => SearchText.Length > 0;

    public PlayerSearchViewModel(IToastManager toastManager, TimeProvider timeProvider)
    {
        _toastManager = toastManager;
        _timeProvider = timeProvider;
    }

    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(HasSearchText));
        StartSearch(value.Trim());
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    [RelayCommand]
    private async Task SendRequestAsync(PlayerSearchResult result)
    {
        result.IsSending = true;

        try
        {
            await SendRequestToServerAsync(result);
        }
        finally
        {
            result.IsSending = false;
        }

        result.Relation = FriendshipRelation.RequestSent;
        _toastManager.Show(string.Format(
            CultureInfo.CurrentCulture,
            LocalizationManager.Instance[RequestSentToastKey],
            result.Username));
    }

    private static Task SendRequestToServerAsync(PlayerSearchResult result)
    {
        // TODO: Send FRIEND_REQUEST to the server (CU-008); a double click must not send it twice. If the other player
        // already sent one, the server accepts it directly.
        return Task.CompletedTask;
    }

    private static Task<IReadOnlyList<PlayerSearchResult>> SearchOnServerAsync(string term)
    {
        // TODO: Search by username on the server (CU-008): case insensitive, never the player himself or guests, with
        // the relation (none, request sent, friends, blocked) of each result.
        IReadOnlyList<PlayerSearchResult> none = new List<PlayerSearchResult>();

        return Task.FromResult(none);
    }

    private void StartSearch(string term)
    {
        int searchNumber = ++_lastSearchNumber;

        Results.Clear();

        if (term.Length == 0)
        {
            State = SearchState.Prompt;
        }
        else if (term.Length < MinimumSearchLength)
        {
            State = SearchState.TooShort;
        }
        else
        {
            State = SearchState.Searching;
            _ = SearchAsync(term, searchNumber);
        }
    }

    // Waits for the player to stop typing; a newer search makes this one obsolete and its answer is dropped.
    private async Task SearchAsync(string term, int searchNumber)
    {
        await Task.Delay(SearchDelay, _timeProvider);

        if (searchNumber == _lastSearchNumber)
        {
            IReadOnlyList<PlayerSearchResult> found = await SearchOnServerAsync(term);

            if (searchNumber == _lastSearchNumber)
            {
                ShowResults(term, found);
            }
        }
    }

    protected override void OnLanguageChanged()
    {
        foreach (PlayerSearchResult result in Results)
        {
            FillResultTexts(result);
        }

        NoMatchesTitle = string.Format(
            CultureInfo.CurrentCulture,
            LocalizationManager.Instance[NoMatchesTitleKey],
            _lastTerm);

        base.OnLanguageChanged();
    }

    private void FillResultTexts(PlayerSearchResult result)
    {
        result.SendLabel = LocalizationManager.Instance[SendKey];
        result.SendingLabel = LocalizationManager.Instance[SendingKey];
        result.StatusText = PresenceFormatter.Format(
            result.Presence,
            result.LastSeenAt,
            _timeProvider.GetUtcNow());
    }

    private void ShowResults(string term, IReadOnlyList<PlayerSearchResult> found)
    {
        _lastTerm = term;
        foreach (PlayerSearchResult result in found)
        {
            result.SendRequestCommand = SendRequestCommand;
            FillResultTexts(result);
            Results.Add(result);
        }

        NoMatchesTitle = string.Format(
            CultureInfo.CurrentCulture,
            LocalizationManager.Instance[NoMatchesTitleKey],
            term);
        State = found.Count == 0 ? SearchState.NoResults : SearchState.Results;
    }
}
