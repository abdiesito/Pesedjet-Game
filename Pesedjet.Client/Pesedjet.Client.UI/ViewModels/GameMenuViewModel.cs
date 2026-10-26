using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Rooms;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Validation;

namespace Pesedjet.Client.UI.ViewModels;

public partial class GameMenuViewModel : ViewModelBase, INavigable
{
    private const string InvalidFormatKey = "GameMenu.CodeInvalidFormat";
    private const string NotFoundKey = "GameMenu.CodeNotFound";
    private const string FullKey = "GameMenu.CodeFull";
    private const string StartedKey = "GameMenu.CodeStarted";

    private readonly INavigator _navigator;
    private readonly IDialogManager _dialogManager;
    private readonly IPlayerSession _playerSession;
    private readonly IRoomGateway _roomGateway;
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

    public bool ShowsTopBar => true;

    public bool IsGuest => _playerSession.IsGuest;

    public bool IsRegistered => !_playerSession.IsGuest;

    public string CodeErrorText => BuildCodeError();

    public bool HasCodeError => CodeErrorText.Length > 0;

    public GameMenuViewModel(
        INavigator navigator,
        IDialogManager dialogManager,
        IPlayerSession playerSession,
        IRoomGateway roomGateway,
        IRoomSession roomSession)
    {
        _roomGateway = roomGateway;
        _roomSession = roomSession;
        _navigator = navigator;
        _dialogManager = dialogManager;
        _playerSession = playerSession;
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
        RoomJoinResult result;

        try
        {
            result = await _roomGateway.JoinRoomAsync(RoomCode);
        }
        finally
        {
            IsJoining = false;
        }

        ShowJoinError(result);

        if (result.Room is not null)
        {
            _roomSession.Current = result.Room;
            _navigator.NavigateTo<WaitingRoomViewModel>();
        }

        // TODO: On a communication failure (MSG-GEN-001) set HasConnectionError and offer "Retry".
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

    private void ShowJoinError(RoomJoinResult result)
    {
        _lastJoinError = result.Error == RoomCodeError.None ? null : result;

        RefreshCodeError();
    }

    private void RefreshCodeError()
    {
        OnPropertyChanged(nameof(CodeErrorText));
        OnPropertyChanged(nameof(HasCodeError));
    }

    private string BuildCodeError()
    {
        string text = string.Empty;

        if (_hasInvalidFormat)
        {
            text = LocalizationManager.Instance[InvalidFormatKey];
        }
        else if (_lastJoinError?.Error == RoomCodeError.NotFound)
        {
            text = LocalizationManager.Instance[NotFoundKey];
        }
        else if (_lastJoinError?.Error == RoomCodeError.Full)
        {
            text = string.Format(
                CultureInfo.CurrentCulture,
                LocalizationManager.Instance[FullKey],
                _lastJoinError.Occupancy,
                _lastJoinError.Capacity);
        }
        else if (_lastJoinError?.Error == RoomCodeError.Started)
        {
            text = LocalizationManager.Instance[StartedKey];
        }

        return text;
    }
}
