#nullable enable

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.UI.ViewModels;

public partial class MatchLobbyViewModel : ViewModelBase, IDisposable
{
    private const int MinimumPlayersToStart = 2;
    private const string ConnectionErrorKey = "Messages.MSG-GEN-001";
    private const string OccupancyKey = "Lobby.Occupancy";
    private const string StatusWaitingKey = "Lobby.StatusWaiting";
    private const string StatusFullKey = "Lobby.StatusFull";
    private const string StatusDisconnectedKey = "Lobby.StatusDisconnected";

    private readonly INavigator _navigator;
    private readonly IRoomClient _roomClient;
    private bool _isSubscribed;

    [ObservableProperty]
    private string _roomCode = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanStartMatch))]
    [NotifyPropertyChangedFor(nameof(ShowMinimumPlayersHint))]
    [NotifyPropertyChangedFor(nameof(IsWaitingForHost))]
    [NotifyCanExecuteChangedFor(nameof(StartMatchCommand))]
    private bool _isHost;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanStartMatch))]
    [NotifyPropertyChangedFor(nameof(CanLeaveLobby))]
    [NotifyCanExecuteChangedFor(nameof(StartMatchCommand))]
    [NotifyCanExecuteChangedFor(nameof(LeaveLobbyCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConnectionError))]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private string _connectionError = string.Empty;

    [ObservableProperty]
    private Guid _hostPlayerId;

    [ObservableProperty]
    private Guid _localPlayerId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OccupancyText))]
    [NotifyPropertyChangedFor(nameof(IsRoomFull))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private int _maxPlayers;

    public MatchLobbyViewModel(INavigator navigator, IRoomClient roomClient)
    {
        _navigator = navigator;
        _roomClient = roomClient;

        Members.CollectionChanged += OnMembersChanged;
        SubscribeToRoomEvents();
    }

    public ObservableCollection<RoomMember> Members { get; } = new();

    public ObservableCollection<RoomMember> Participants => Members;

    public bool CanStartMatch => IsHost && !IsBusy && Members.Count >= MinimumPlayersToStart;

    public bool CanLeaveLobby => !IsBusy;

    public bool HasConnectionError => !string.IsNullOrEmpty(ConnectionError);

    public string ErrorMessage => ConnectionError;

    public bool ShowMinimumPlayersHint => Members.Count < MinimumPlayersToStart;

    public bool IsWaitingForHost => !IsHost;

    public bool IsRoomFull => MaxPlayers > 0 && Members.Count >= MaxPlayers;

    public string OccupancyText
    {
        get
        {
            string occupancyText = string.Format(
                CultureInfo.CurrentCulture,
                Localize(OccupancyKey),
                Members.Count,
                MaxPlayers);

            return occupancyText;
        }
    }

    public string StatusText
    {
        get
        {
            string statusKey;
            if (HasConnectionError)
            {
                statusKey = StatusDisconnectedKey;
            }
            else if (IsRoomFull)
            {
                statusKey = StatusFullKey;
            }
            else
            {
                statusKey = StatusWaitingKey;
            }

            return Localize(statusKey);
        }
    }

    public void Initialize(string roomCode, RoomMember localPlayer, Guid hostPlayerId, int maxPlayers)
    {
        RoomCode = roomCode;
        LocalPlayerId = localPlayer.PlayerId;
        HostPlayerId = hostPlayerId;
        IsHost = localPlayer.PlayerId == hostPlayerId;
        MaxPlayers = maxPlayers;
        ConnectionError = string.Empty;

        Members.Clear();
        Members.Add(localPlayer);
    }

    public void Dispose()
    {
        UnsubscribeFromRoomEvents();
        Members.CollectionChanged -= OnMembersChanged;
        GC.SuppressFinalize(this);
    }

    [RelayCommand(CanExecute = nameof(CanStartMatch))]
    private Task StartMatchAsync()
    {
        IsBusy = true;
        ConnectionError = string.Empty;
        UnsubscribeFromRoomEvents();
        _navigator.NavigateTo<MatchViewModel>();

        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanLeaveLobby))]
    private async Task LeaveLobbyAsync()
    {
        bool hasLeft;
        IsBusy = true;
        ConnectionError = string.Empty;

        try
        {
            await _roomClient.LeaveRoomAsync(RoomCode, LocalPlayerId);
            hasLeft = true;
        }
        catch (FaultException<ServiceFault>)
        {
            hasLeft = true;
        }
        catch (CommunicationException)
        {
            hasLeft = false;
        }
        catch (TimeoutException)
        {
            hasLeft = false;
        }
        finally
        {
            IsBusy = false;
        }

        if (hasLeft)
        {
            UnsubscribeFromRoomEvents();
            _navigator.GoBack();
        }
        else
        {
            ConnectionError = Localize(ConnectionErrorKey);
        }
    }

    private void SubscribeToRoomEvents()
    {
        if (!_isSubscribed)
        {
            _roomClient.PlayerJoined += OnPlayerJoined;
            _roomClient.PlayerLeft += OnPlayerLeft;
            _isSubscribed = true;
        }
    }

    private void UnsubscribeFromRoomEvents()
    {
        if (_isSubscribed)
        {
            _roomClient.PlayerJoined -= OnPlayerJoined;
            _roomClient.PlayerLeft -= OnPlayerLeft;
            _isSubscribed = false;
        }
    }

    private void OnPlayerJoined(object? sender, RoomMember player)
    {
        Dispatcher.UIThread.Post(() => AddOrReplaceMember(player));
    }

    private void OnPlayerLeft(object? sender, Guid playerId)
    {
        Dispatcher.UIThread.Post(() => RemoveMember(playerId));
    }

    private void AddOrReplaceMember(RoomMember player)
    {
        RoomMember? existingMember = Members.FirstOrDefault(member => member.PlayerId == player.PlayerId);
        if (existingMember is null)
        {
            Members.Add(player);
        }
        else
        {
            Members[Members.IndexOf(existingMember)] = player;
        }
    }

    private void RemoveMember(Guid playerId)
    {
        RoomMember? leavingMember = Members.FirstOrDefault(member => member.PlayerId == playerId);
        if (leavingMember is not null)
        {
            Members.Remove(leavingMember);
        }
    }

    private void OnMembersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CanStartMatch));
        OnPropertyChanged(nameof(ShowMinimumPlayersHint));
        OnPropertyChanged(nameof(IsRoomFull));
        OnPropertyChanged(nameof(OccupancyText));
        OnPropertyChanged(nameof(StatusText));
        StartMatchCommand.NotifyCanExecuteChanged();
    }

    private static string Localize(string key)
    {
        string text = LocalizationManager.Instance[key];

        return text;
    }
}