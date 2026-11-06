#nullable enable

using System;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Rooms;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Validation;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Faults;

namespace Pesedjet.Client.UI.ViewModels;

public partial class GameMenuViewModel : ViewModelBase, INavigable
{
    private const string InvalidFormatKey = "GameMenu.CodeInvalidFormat";

    private readonly INavigator _navigator;
    private readonly IDialogManager _dialogManager;
    private readonly IPlayerSession _playerSession;
    private readonly IRoomClient _roomClient;
    private readonly IRoomSession _roomSession;

    [ObservableProperty]
    private string _roomCode = string.Empty;

    private bool _hasInvalidFormat;
    private RoomJoinResult? _lastJoinError;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(JoinRoomCommand))]
    private bool _canJoin;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(JoinRoomCommand))]
    private bool _isJoining;

    [ObservableProperty]
    private bool _hasConnectionError;

    public PublicRoomListViewModel PublicRooms { get; }

    public bool ShowsTopBar => true;

    public bool IsGuest => _playerSession.IsGuest;

    public bool IsRegistered => !_playerSession.IsGuest;

    public string CodeErrorText => BuildCodeError();

    public bool HasCodeError => CodeErrorText.Length > 0;

    public GameMenuViewModel(
        INavigator navigator,
        IDialogManager dialogManager,
        IPlayerSession playerSession,
        IRoomClient roomClient,
        IRoomSession roomSession,
        PublicRoomListViewModel publicRooms)
    {
        PublicRooms = publicRooms;
        _roomClient = roomClient;
        _roomSession = roomSession;
        _navigator = navigator;
        _dialogManager = dialogManager;
        _playerSession = playerSession;
    }

    // The list is read again every time the menu is shown, so it never keeps rooms that closed in the meantime.
    public void OnNavigatedTo()
    {
        _ = PublicRooms.LoadAsync();
    }

    partial void OnRoomCodeChanged(string value)
    {
        string normalized = new(value.Where(character => !char.IsWhiteSpace(character)).ToArray());

        normalized = normalized.ToUpperInvariant();

        if (normalized != value)
        {
            RoomCode = normalized;
        }
        else
        {
            ValidateCode(normalized);
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        if (IsGuest)
        {
            await _dialogManager.ShowDialogAsync<GuestExitViewModel>();
        }
        else
        {
            _navigator.GoBack();
        }
    }

    [RelayCommand]
    private void CreateRoom()
    {
        _navigator.NavigateTo<GameSetupViewModel>();
    }

    [RelayCommand(CanExecute = nameof(CanJoinRoom))]
    private async Task JoinRoomAsync()
    {
        IsJoining = true;
        HasConnectionError = false;
        _lastJoinError = null;
        RefreshCodeError();

        try
        {
            RoomJoinResponse response = await _roomClient.JoinRoomAsync(
                RoomCode,
                RoomModelFactory.CreateLocalMember(_playerSession));

            _roomSession.Current = RoomModelFactory.CreateJoinedRoom(response, _playerSession);
            _navigator.NavigateTo<WaitingRoomViewModel>();
        }
        catch (FaultException<ServiceFault> ex)
        {
            _lastJoinError = RoomJoinErrors.FromMessageKey(ex.Detail.MessageKey);
            RefreshCodeError();
        }
        catch (CommunicationException)
        {
            HasConnectionError = true;
        }
        catch (TimeoutException)
        {
            HasConnectionError = true;
        }
        finally
        {
            IsJoining = false;
        }
    }

    private bool CanJoinRoom()
    {
        return CanJoin && !IsJoining;
    }

    private void ValidateCode(string code)
    {
        bool isComplete = code.Length == RoomRules.CodeLength;
        bool hasInvalidCharacters = code.Any(character => !char.IsLetterOrDigit(character));

        HasConnectionError = false;
        _lastJoinError = null;
        _hasInvalidFormat = isComplete && hasInvalidCharacters;
        CanJoin = isComplete && !hasInvalidCharacters;

        RefreshCodeError();
    }

    private void RefreshCodeError()
    {
        OnPropertyChanged(nameof(CodeErrorText));
        OnPropertyChanged(nameof(HasCodeError));
    }

    private string BuildCodeError()
    {
        return _hasInvalidFormat
            ? LocalizationManager.Instance[InvalidFormatKey]
            : RoomJoinErrors.Describe(_lastJoinError);
    }
}
