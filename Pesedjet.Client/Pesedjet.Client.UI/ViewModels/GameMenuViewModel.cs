#nullable enable

using System;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Lobbies;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Validation;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Faults;
using Pesedjet.Client.UI.Utilities.Threading;
using Pesedjet.Client.UI.ViewModels.Dependencies;
using Pesedjet.Client.UI.ViewModels.Dialogs;
using Pesedjet.Client.UI.Models.Lobbies;

namespace Pesedjet.Client.UI.ViewModels;

public partial class GameMenuViewModel : ViewModelBase, INavigable
{
    private const string InvalidFormatKey = "GameMenu.CodeInvalidFormat";

    private readonly INavigator _navigator;
    private readonly IDialogManager _dialogManager;
    private readonly IPlayerSession _playerSession;
    private readonly ILobbyClient _lobbyClient;
    private readonly ILobbySession _lobbySession;

    [ObservableProperty]
    private string _lobbyCode = string.Empty;

    private bool _hasInvalidFormat;
    private LobbyJoinResult? _lastJoinError;
    private BackgroundJob<LobbyJoinRequest, LobbyJoinResponse>? _joinJob;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(JoinLobbyCommand))]
    private bool _canJoin;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(JoinLobbyCommand))]
    private bool _isJoining;

    [ObservableProperty]
    private bool _hasConnectionError;

    public PublicLobbyListViewModel PublicLobbies { get; }

    public bool ShowsTopBar => true;

    public bool IsGuest => _playerSession.IsGuest;

    public bool IsRegistered => !_playerSession.IsGuest;

    public string CodeErrorText => BuildCodeError();

    public bool HasCodeError => CodeErrorText.Length > 0;

    public GameMenuViewModel(GameMenuDependencies dependencies)
    {
        PublicLobbies = dependencies.PublicLobbies;
        _lobbyClient = dependencies.LobbyClient;
        _lobbySession = dependencies.LobbySession;
        _navigator = dependencies.Navigator;
        _dialogManager = dependencies.DialogManager;
        _playerSession = dependencies.PlayerSession;
    }

    // The list is read again every time the menu is shown, so it never keeps lobbies that closed in the meantime.
    public void OnNavigatedTo()
    {
        PublicLobbies.Load();
    }

    public void OnNavigatedFrom()
    {
        CancelPendingWork();
    }

    partial void OnLobbyCodeChanged(string value)
    {
        string normalized = new(value.Where(character => !char.IsWhiteSpace(character)).ToArray());

        normalized = normalized.ToUpperInvariant();

        if (normalized != value)
        {
            LobbyCode = normalized;
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
    private void CreateLobby()
    {
        _navigator.NavigateTo<GameSetupViewModel>();
    }

    [RelayCommand(CanExecute = nameof(CanJoinLobby))]
    private void JoinLobby()
    {
        IsJoining = true;
        HasConnectionError = false;
        _lastJoinError = null;
        RefreshCodeError();

        LobbyJoinRequest request = new(LobbyCode, LobbyModelFactory.CreateLocalMember(_playerSession));

        BackgroundJob<LobbyJoinRequest, LobbyJoinResponse> job = new()
        {
            Work = JoinLobbyOnWorker,
            State = request,
            OnCompleted = OnJoinLobbyCompleted,
            OnFailed = OnJoinLobbyFailed,
            OnCompletedAfterCancel = OnJoinLobbyDiscarded
        };

        _joinJob = job;
        BackgroundWork.Start(job);
    }

    private LobbyJoinResponse JoinLobbyOnWorker(LobbyJoinRequest request)
    {
        return _lobbyClient.JoinLobby(request.LobbyCode, request.Player);
    }

    private void OnJoinLobbyCompleted(LobbyJoinResponse response, LobbyJoinRequest request)
    {
        _joinJob = null;
        IsJoining = false;
        _lobbySession.Current = LobbyModelFactory.CreateJoinedLobby(response, _playerSession);
        _navigator.NavigateTo<LobbyViewModel>();
    }

    private void OnJoinLobbyFailed(Exception error, LobbyJoinRequest request)
    {
        _joinJob = null;
        IsJoining = false;

        if (error is FaultException<ServiceFault> fault)
        {
            _lastJoinError = LobbyJoinErrors.FromMessageKey(fault.Detail.MessageKey);
            RefreshCodeError();
        }
        else
        {
            HasConnectionError = true;
        }
    }

    // The user left the screen before the answer arrived, but the server already accepted the join: undo it.
    private void OnJoinLobbyDiscarded(LobbyJoinResponse response, LobbyJoinRequest request)
    {
        LobbyLeaveRequest leave = new(request.LobbyCode, request.Player.PlayerId);

        BackgroundAction<LobbyLeaveRequest> action = new()
        {
            Work = LeaveLobbyOnWorker,
            State = leave
        };

        BackgroundWork.Start(action);
    }

    private void LeaveLobbyOnWorker(LobbyLeaveRequest request)
    {
        _lobbyClient.LeaveLobby(request.LobbyCode, request.PlayerId);
    }

    private void CancelPendingWork()
    {
        if (_joinJob is not null)
        {
            _joinJob.Cancel();
            _joinJob = null;
            IsJoining = false;
        }

        PublicLobbies.CancelPendingWork();
    }

    private bool CanJoinLobby()
    {
        return CanJoin && !IsJoining;
    }

    private void ValidateCode(string code)
    {
        bool isComplete = code.Length == LobbyRules.CodeLength;
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
            : LobbyJoinErrors.Describe(_lastJoinError);
    }
}
