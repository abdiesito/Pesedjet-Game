using System;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Social;

namespace Pesedjet.Client.UI.ViewModels;

public partial class FriendsViewModel : ViewModelBase, INavigable, IDisposable
{
    private readonly INavigator _navigator;
    private readonly ISocialCircle _socialCircle;
    private readonly FriendListViewModel _friendList;
    private readonly PlayerSearchViewModel _playerSearch;
    private readonly BlockedPlayersViewModel _blockedPlayers;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(CurrentTabViewModel),
        nameof(IsFriendsTabSelected),
        nameof(IsSearchTabSelected),
        nameof(IsBlockedTabSelected))]
    private FriendsTab _selectedTab = FriendsTab.Friends;

    public bool ShowsTopBar => true;

    public int FriendCount => _socialCircle.Friends.Count;

    public int BlockedCount => _socialCircle.BlockedPlayers.Count;

    public bool IsFriendsTabSelected => SelectedTab == FriendsTab.Friends;

    public bool IsSearchTabSelected => SelectedTab == FriendsTab.Search;

    public bool IsBlockedTabSelected => SelectedTab == FriendsTab.Blocked;

    public ViewModelBase CurrentTabViewModel => SelectedTab switch
    {
        FriendsTab.Search => _playerSearch,
        FriendsTab.Blocked => _blockedPlayers,
        _ => _friendList
    };

    public FriendsViewModel(
        INavigator navigator,
        ISocialCircle socialCircle,
        FriendListViewModel friendList,
        PlayerSearchViewModel playerSearch,
        BlockedPlayersViewModel blockedPlayers)
    {
        _navigator = navigator;
        _socialCircle = socialCircle;
        _friendList = friendList;
        _playerSearch = playerSearch;
        _blockedPlayers = blockedPlayers;

        _friendList.SearchRequested += HandleSearchRequested;
        _socialCircle.Friends.CollectionChanged += HandleFriendsChanged;
        _socialCircle.BlockedPlayers.CollectionChanged += HandleBlockedPlayersChanged;
    }

    public void Dispose()
    {
        _friendList.SearchRequested -= HandleSearchRequested;
        _socialCircle.Friends.CollectionChanged -= HandleFriendsChanged;
        _socialCircle.BlockedPlayers.CollectionChanged -= HandleBlockedPlayersChanged;
        _friendList.Dispose();
        _blockedPlayers.Dispose();
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigator.GoBack();
    }

    [RelayCommand]
    private void ShowFriends()
    {
        SelectedTab = FriendsTab.Friends;
    }

    [RelayCommand]
    private void ShowSearch()
    {
        SelectedTab = FriendsTab.Search;
    }

    [RelayCommand]
    private void ShowBlocked()
    {
        SelectedTab = FriendsTab.Blocked;
    }

    private void HandleSearchRequested(object? sender, EventArgs eventArgs)
    {
        SelectedTab = FriendsTab.Search;
    }

    private void HandleFriendsChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        OnPropertyChanged(nameof(FriendCount));
    }

    private void HandleBlockedPlayersChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        OnPropertyChanged(nameof(BlockedCount));
    }
}
