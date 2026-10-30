#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Navigation;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.UI.ViewModels;

public partial class PublicRoomsViewModel : ViewModelBase
{
    private const string ConnectionErrorKey = "MSG-GEN-001";

    private readonly INavigator _navigator;
    private readonly IRoomClient _roomClient;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(JoinSelectedRoomCommand))]
    private RoomSummary? _selectedRoom;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyCanExecuteChangedFor(nameof(RefreshRoomsCommand))]
    [NotifyCanExecuteChangedFor(nameof(JoinSelectedRoomCommand))]
    private bool _isBusy;

    public PublicRoomsViewModel(INavigator navigator, IRoomClient roomClient)
    {
        _navigator = navigator;
        _roomClient = roomClient;
    }

    public ObservableCollection<RoomSummary> Rooms { get; } = new();

    public RoomMember CurrentPlayerMember { get; private set; } = new();

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool IsNotBusy => !IsBusy;

    public bool IsEmpty => !IsBusy && !HasError && Rooms.Count == 0;

    public bool CanJoinSelectedRoom => !IsBusy && SelectedRoom is not null;

    public void Initialize(RoomMember currentPlayerMember)
    {
        CurrentPlayerMember = currentPlayerMember;
        ErrorMessage = string.Empty;
        RefreshRoomsCommand.Execute(null);
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task RefreshRoomsAsync()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            await LoadRoomsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanJoinSelectedRoom))]
    private async Task JoinSelectedRoomAsync()
    {
        string roomCode = SelectedRoom?.RoomCode ?? string.Empty;
        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            RoomJoinResponse response = await _roomClient.JoinRoomAsync(roomCode, CurrentPlayerMember);
            _navigator.NavigateToJoinedLobby(response, CurrentPlayerMember);
        }
        catch (FaultException<ServiceFault> ex)
        {
            ErrorMessage = Localize(ex.Detail.MessageKey);
            await LoadRoomsAsync();
        }
        catch (CommunicationException)
        {
            ErrorMessage = Localize(ConnectionErrorKey);
        }
        catch (TimeoutException)
        {
            ErrorMessage = Localize(ConnectionErrorKey);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadRoomsAsync()
    {
        try
        {
            List<RoomSummary> rooms = await _roomClient.GetPublicRoomsAsync();
            ReplaceRooms(rooms);
        }
        catch (FaultException<ServiceFault> ex)
        {
            ErrorMessage = Localize(ex.Detail.MessageKey);
        }
        catch (CommunicationException)
        {
            ErrorMessage = Localize(ConnectionErrorKey);
        }
        catch (TimeoutException)
        {
            ErrorMessage = Localize(ConnectionErrorKey);
        }
    }

    private void ReplaceRooms(IEnumerable<RoomSummary> rooms)
    {
        string? selectedCode = SelectedRoom?.RoomCode;
        RoomSummary? reselectedRoom = null;

        Rooms.Clear();
        foreach (RoomSummary room in rooms)
        {
            Rooms.Add(room);
            if (room.RoomCode == selectedCode)
            {
                reselectedRoom = room;
            }
        }

        SelectedRoom = reselectedRoom;
        OnPropertyChanged(nameof(IsEmpty));
    }

    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(IsEmpty));
    }

    partial void OnSelectedRoomChanged(RoomSummary? value)
    {
        OnPropertyChanged(nameof(CanJoinSelectedRoom));
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(CanJoinSelectedRoom));
    }

    private static string Localize(string key)
    {
        string text = LocalizationManager.Instance[key];

        return text;
    }
}