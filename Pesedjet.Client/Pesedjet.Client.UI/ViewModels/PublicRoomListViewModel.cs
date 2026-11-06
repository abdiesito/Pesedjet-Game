#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Rooms;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.UI.ViewModels;

// The public rooms of the game menu: the rooms that are open to anyone and still waiting for players.
public partial class PublicRoomListViewModel : ViewModelBase
{
    private readonly INavigator _navigator;
    private readonly IRoomClient _roomClient;
    private readonly IPlayerSession _playerSession;
    private readonly IRoomSession _roomSession;

    private int _lastLoadNumber;
    private RoomJoinResult? _lastJoinError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowsList), nameof(ShowsEmptyState))]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowsList), nameof(ShowsEmptyState))]
    private bool _hasLoadError;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(JoinRoomCommand))]
    private bool _isJoining;

    [ObservableProperty]
    private bool _hasConnectionError;

    public ObservableCollection<PublicRoomItem> Rooms { get; } = new();

    public bool ShowsList => Rooms.Count > 0 && !IsLoading && !HasLoadError;

    public bool ShowsEmptyState => Rooms.Count == 0 && !IsLoading && !HasLoadError;

    public string JoinErrorText => RoomJoinErrors.Describe(_lastJoinError);

    public bool HasJoinError => JoinErrorText.Length > 0;

    public PublicRoomListViewModel(
        INavigator navigator,
        IRoomClient roomClient,
        IRoomSession roomSession,
        IPlayerSession playerSession)
    {
        _navigator = navigator;
        _roomClient = roomClient;
        _roomSession = roomSession;
        _playerSession = playerSession;
    }

    // A newer load makes the previous one obsolete: only the last answer is shown.
    public async Task LoadAsync()
    {
        int loadNumber = ++_lastLoadNumber;

        IsLoading = true;
        HasLoadError = false;
        ShowJoinError(null);

        try
        {
            List<RoomSummary> summaries = await _roomClient.GetPublicRoomsAsync();

            if (loadNumber == _lastLoadNumber)
            {
                ShowRooms(summaries);
            }
        }
        catch (Exception ex) when (ex is CommunicationException or TimeoutException)
        {
            HasLoadError = true;
        }
        finally
        {
            if (loadNumber == _lastLoadNumber)
            {
                IsLoading = false;
            }
        }
    }

    protected override void OnLanguageChanged()
    {
        foreach (PublicRoomItem room in Rooms)
        {
            room.Refresh();
        }

        base.OnLanguageChanged();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanJoinRoom))]
    private async Task JoinRoomAsync(PublicRoomItem room)
    {
        IsJoining = true;
        room.IsJoining = true;
        HasConnectionError = false;
        ShowJoinError(null);

        try
        {
            RoomJoinResponse response = await _roomClient.JoinRoomAsync(
                room.Code,
                RoomModelFactory.CreateLocalMember(_playerSession));

            _roomSession.Current = RoomModelFactory.CreateJoinedRoom(response, _playerSession);
            _navigator.NavigateTo<WaitingRoomViewModel>();
        }
        catch (FaultException<ServiceFault> ex)
        {
            ShowJoinError(RoomJoinErrors.FromMessageKey(ex.Detail.MessageKey, room.CurrentPlayers, room.MaxPlayers));
        }
        catch (Exception ex) when (ex is CommunicationException or TimeoutException)
        {
            HasConnectionError = true;
        }
        finally
        {
            room.IsJoining = false;
            IsJoining = false;
        }
    }

    private bool CanJoinRoom(PublicRoomItem? room)
    {
        return room is not null && !room.IsFull && !IsJoining;
    }

    private void ShowRooms(List<RoomSummary> summaries)
    {
        Rooms.Clear();

        foreach (RoomSummary summary in summaries)
        {
            Rooms.Add(new PublicRoomItem
            {
                Code = summary.RoomCode,
                HostUsername = summary.HostUsername,
                CurrentPlayers = summary.CurrentPlayers,
                MaxPlayers = summary.MaxPlayers,
                JoinCommand = JoinRoomCommand
            });
        }

        OnPropertyChanged(nameof(ShowsList));
        OnPropertyChanged(nameof(ShowsEmptyState));
    }

    private void ShowJoinError(RoomJoinResult? error)
    {
        _lastJoinError = error;

        OnPropertyChanged(nameof(JoinErrorText));
        OnPropertyChanged(nameof(HasJoinError));
    }
}
