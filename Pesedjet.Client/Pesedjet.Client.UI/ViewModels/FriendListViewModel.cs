using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Social;
using Pesedjet.Client.UI.Utilities.Toasts;

namespace Pesedjet.Client.UI.ViewModels;

public partial class FriendListViewModel : ViewModelBase, IDisposable
{
    private const string SummaryKey = "Friends.Summary";
    private const string SummaryWithMatchesKey = "Friends.SummaryWithMatches";
    private const string RemovedToastKey = "Friends.RemovedToast";
    private const string BlockedToastKey = "Friends.BlockedToast";

    private readonly ISocialCircle _socialCircle;
    private readonly IDialogManager _dialogManager;
    private readonly IToastManager _toastManager;
    private readonly TimeProvider _timeProvider;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowsList), nameof(ShowsEmptyState))]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowsList), nameof(ShowsEmptyState))]
    private bool _hasLoadError;

    public event EventHandler? SearchRequested;

    public ObservableCollection<FriendItem> Friends => _socialCircle.Friends;

    public bool ShowsList => Friends.Count > 0 && !IsLoading && !HasLoadError;

    public bool ShowsEmptyState => Friends.Count == 0 && !IsLoading && !HasLoadError;

    public string Summary => BuildSummary();

    public FriendListViewModel(
        ISocialCircle socialCircle,
        IDialogManager dialogManager,
        IToastManager toastManager,
        TimeProvider timeProvider)
    {
        _socialCircle = socialCircle;
        _dialogManager = dialogManager;
        _toastManager = toastManager;
        _timeProvider = timeProvider;

        foreach (FriendItem friend in Friends)
        {
            FillFriend(friend);
        }

        Friends.CollectionChanged += HandleFriendsChanged;
    }

    public void Dispose()
    {
        Friends.CollectionChanged -= HandleFriendsChanged;
    }

    [RelayCommand]
    private void GoToSearch()
    {
        SearchRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Retry()
    {
        // TODO: Ask the server for the friends again (CU-007); show the loading skeleton while it answers (RNF-05).
        HasLoadError = false;
    }

    [RelayCommand]
    private async Task RemoveFriendAsync(FriendItem friend)
    {
        PlayerActionConfirmationViewModel dialog =
            await _dialogManager.ShowDialogAsync<PlayerActionConfirmationViewModel, PlayerActionRequest>(
                new PlayerActionRequest(PlayerAction.RemoveFriend, friend.Username));

        if (dialog.IsConfirmed)
        {
            // TODO: Send the removal to the server (CU-007); if it fails keep the row and show MSG-GEN-001.
            Friends.Remove(friend);
            _toastManager.Show(FormatName(RemovedToastKey, friend.Username));
        }
    }

    [RelayCommand]
    private async Task BlockFriendAsync(FriendItem friend)
    {
        PlayerActionConfirmationViewModel dialog =
            await _dialogManager.ShowDialogAsync<PlayerActionConfirmationViewModel, PlayerActionRequest>(
                new PlayerActionRequest(PlayerAction.BlockPlayer, friend.Username));

        if (dialog.IsConfirmed)
        {
            // TODO: Send BLOCK_REQUEST to the server (CU-007); if it fails keep the row and show MSG-GEN-001.
            Friends.Remove(friend);
            _socialCircle.BlockedPlayers.Insert(
                0,
                new BlockedPlayerItem
                {
                    Id = friend.Id,
                    Username = friend.Username,
                    BlockedAt = _timeProvider.GetUtcNow()
                });
            _toastManager.Show(FormatName(BlockedToastKey, friend.Username));
        }
    }

    protected override void OnLanguageChanged()
    {
        foreach (FriendItem friend in Friends)
        {
            FillFriend(friend);
        }

        base.OnLanguageChanged();
    }

    private static string FormatName(string key, string username)
    {
        return string.Format(CultureInfo.CurrentCulture, LocalizationManager.Instance[key], username);
    }

    private void HandleFriendsChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        if (eventArgs.NewItems is not null)
        {
            foreach (FriendItem friend in eventArgs.NewItems)
            {
                FillFriend(friend);
            }
        }

        OnPropertyChanged(nameof(Summary));
        OnPropertyChanged(nameof(ShowsList));
        OnPropertyChanged(nameof(ShowsEmptyState));
    }

    private void FillFriend(FriendItem friend)
    {
        friend.RemoveCommand = RemoveFriendCommand;
        friend.BlockCommand = BlockFriendCommand;
        friend.StatusText = PresenceFormatter.Format(friend.Presence, friend.LastSeenAt, _timeProvider.GetUtcNow());
    }

    private string BuildSummary()
    {
        int online = 0;
        int inMatch = 0;

        foreach (FriendItem friend in Friends)
        {
            if (friend.IsOnline)
            {
                online++;
            }
            else if (friend.IsInMatch)
            {
                inMatch++;
            }
        }

        string summary = string.Format(
            CultureInfo.CurrentCulture,
            LocalizationManager.Instance[SummaryKey],
            online,
            Friends.Count);

        if (inMatch > 0)
        {
            summary = string.Format(
                CultureInfo.CurrentCulture,
                LocalizationManager.Instance[SummaryWithMatchesKey],
                online,
                inMatch,
                Friends.Count);
        }

        return summary;
    }
}
