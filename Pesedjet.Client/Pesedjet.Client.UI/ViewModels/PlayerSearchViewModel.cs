using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Social;
using Pesedjet.Client.UI.Utilities.Threading;
using Pesedjet.Client.UI.Utilities.Toasts;
using Pesedjet.Client.UI.ViewModels.Dependencies;
using Pesedjet.Client.UI.Models.Social;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Faults;

namespace Pesedjet.Client.UI.ViewModels;

public partial class PlayerSearchViewModel : ViewModelBase
{
    private const int MinimumSearchLength = 3;
    private const int SearchDelayMilliseconds = 300;
    private const string SendKey = "Friends.SendRequest";
    private const string SendingKey = "Friends.Sending";
    private const string NoMatchesTitleKey = "Friends.NoMatchesTitle";
    private const string RequestSentToastKey = "Friends.RequestSentToast";
    private const string FriendAddedToastKey = "Friends.FriendAddedToast";

    private static readonly TimeSpan SearchDelay = TimeSpan.FromMilliseconds(SearchDelayMilliseconds);

    private readonly IFriendshipClient _friendshipClient;
    private readonly IPlayerSession _playerSession;
    private readonly ISocialCircle _socialCircle;
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

    public PlayerSearchViewModel(PlayerSearchDependencies dependencies)
    {
        _friendshipClient = dependencies.FriendshipClient;
        _playerSession = dependencies.PlayerSession;
        _socialCircle = dependencies.SocialCircle;
        _toastManager = dependencies.ToastManager;
        _timeProvider = dependencies.TimeProvider;
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
    private void SendRequest(PlayerSearchResult result)
    {
        result.IsSending = true;

        FriendRequest request = new FriendRequest
        {
            SenderId = _playerSession.PlayerId,
            TargetId = result.Id
        };
        FriendRequestSubmission submission = new(result, request);

        BackgroundJob<FriendRequestSubmission, ClientResult<PlayerRelation>> job = new()
        {
            Work = SendFriendRequestOnWorker,
            State = submission,
            OnCompleted = OnSendRequestCompleted,
            OnFailed = OnSendRequestFailed
        };

        BackgroundWork.Start(job);
    }

    private ClientResult<PlayerRelation> SendFriendRequestOnWorker(FriendRequestSubmission submission)
    {
        return _friendshipClient.SendFriendRequest(submission.Request);
    }

    private void OnSendRequestCompleted(ClientResult<PlayerRelation> result, FriendRequestSubmission submission)
    {
        submission.Player.IsSending = false;

        if (result.IsSuccess)
        {
            ApplySentRelation(submission.Player, result.Value);
        }
        else
        {
            ApplySendFailure(submission.Player, result);
        }
    }

    private void OnSendRequestFailed(Exception error, FriendRequestSubmission submission)
    {
        submission.Player.IsSending = false;
        ApplySendFailure(submission.Player, ClientResult<PlayerRelation>.TechnicalFailure(FailureKind.Unexpected));
    }

    private void ApplySentRelation(PlayerSearchResult player, PlayerRelation relation)
    {
        if (relation == PlayerRelation.Friends)
        {
            player.Relation = FriendshipRelation.Friends;
            _socialCircle.AddFriend(player.Id, player.Username);
            ShowToast(FriendAddedToastKey, player.Username);
        }
        else
        {
            player.Relation = FriendshipRelation.RequestSent;
            ShowToast(RequestSentToastKey, player.Username);
        }
    }

    private void ApplySendFailure(PlayerSearchResult player, ClientResult<PlayerRelation> result)
    {
        bool isAlreadySent = result.Failure == FailureKind.Business
                             && result.ErrorCode == ServiceErrorCodes.FriendRequestAlreadySent;
        bool isAlreadyFriend = result.Failure == FailureKind.Business
                               && result.ErrorCode == ServiceErrorCodes.AlreadyFriends;

        if (isAlreadySent)
        {
            player.Relation = FriendshipRelation.RequestSent;
        }
        else if (isAlreadyFriend)
        {
            player.Relation = FriendshipRelation.Friends;
        }
        else
        {
            _toastManager.Show(FriendshipErrors.Describe(result));
        }
    }

    private void ShowToast(string key, string username)
    {
        _toastManager.Show(string.Format(CultureInfo.CurrentCulture, LocalizationManager.Instance[key], username));
    }

    private ClientResult<List<PlayerSearchEntry>> SearchOnWorker(PlayerSearchCall call)
    {
        PlayerSearchRequest request = new PlayerSearchRequest
        {
            RequesterId = call.RequesterId,
            SearchTerm = call.Term
        };

        return _friendshipClient.SearchPlayers(request);
    }

    private void OnSearchCompleted(ClientResult<List<PlayerSearchEntry>> result, PlayerSearchCall call)
    {
        if (call.SearchNumber == _lastSearchNumber)
        {
            if (result.IsSuccess)
            {
                ShowResults(call.Term, CreateResults(result.Value!));
            }
            else
            {
                State = SearchState.Prompt;
                _toastManager.Show(FriendshipErrors.Describe(result));
            }
        }
    }

    private void OnSearchFailed(Exception error, PlayerSearchCall call)
    {
        OnSearchCompleted(ClientResult<List<PlayerSearchEntry>>.TechnicalFailure(FailureKind.Unexpected), call);
    }

    private static List<PlayerSearchResult> CreateResults(List<PlayerSearchEntry> entries)
    {
        List<PlayerSearchResult> results = new List<PlayerSearchResult>();

        foreach (PlayerSearchEntry entry in entries)
        {
            results.Add(new PlayerSearchResult
            {
                Id = entry.PlayerId,
                Username = entry.Username,
                Presence = PresenceStatus.Offline,
                Relation = MapRelation(entry.Relation)
            });
        }

        return results;
    }

    private static FriendshipRelation MapRelation(PlayerRelation relation)
    {
        return relation switch
        {
            PlayerRelation.RequestSent => FriendshipRelation.RequestSent,
            PlayerRelation.Friends => FriendshipRelation.Friends,
            PlayerRelation.Blocked => FriendshipRelation.Blocked,
            _ => FriendshipRelation.None
        };
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
            PlayerSearchCall call = new(_playerSession.PlayerId, term, searchNumber);

            BackgroundJob<PlayerSearchCall, ClientResult<List<PlayerSearchEntry>>> job = new()
            {
                Work = SearchOnWorker,
                State = call,
                OnCompleted = OnSearchCompleted,
                OnFailed = OnSearchFailed
            };

            BackgroundWork.Start(job);
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
