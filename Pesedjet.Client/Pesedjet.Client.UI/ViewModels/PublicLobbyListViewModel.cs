#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Lobbies;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Client.UI.Utilities.Threading;
using Pesedjet.Client.UI.ViewModels.Dependencies;
using Pesedjet.Client.UI.Models.Lobbies;

namespace Pesedjet.Client.UI.ViewModels;

// The public lobbies of the game menu: the lobbies that are open to anyone and still waiting for players.
public partial class PublicLobbyListViewModel : ViewModelBase
{
    private readonly INavigator _navigator;
    private readonly ILobbyClient _lobbyClient;
    private readonly IPlayerSession _playerSession;
    private readonly ILobbySession _lobbySession;

    private int _lastLoadNumber;
    private BackgroundJob<int, List<LobbySummary>>? _loadJob;
    private BackgroundJob<PublicLobbyJoinRequest, LobbyJoinResponse>? _joinJob;
    private LobbyJoinResult? _lastJoinError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowsList), nameof(ShowsEmptyState))]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowsList), nameof(ShowsEmptyState))]
    private bool _hasLoadError;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(JoinLobbyCommand))]
    private bool _isJoining;

    [ObservableProperty]
    private bool _hasConnectionError;

    public ObservableCollection<PublicLobbyItem> Lobbies { get; } = new();

    public bool ShowsList => Lobbies.Count > 0 && !IsLoading && !HasLoadError;

    public bool ShowsEmptyState => Lobbies.Count == 0 && !IsLoading && !HasLoadError;

    public string JoinErrorText => LobbyJoinErrors.Describe(_lastJoinError);

    public bool HasJoinError => JoinErrorText.Length > 0;

    public PublicLobbyListViewModel(PublicLobbyListDependencies dependencies)
    {
        _navigator = dependencies.Navigator;
        _lobbyClient = dependencies.LobbyClient;
        _lobbySession = dependencies.LobbySession;
        _playerSession = dependencies.PlayerSession;
    }

    // A newer load makes the previous one obsolete: only the last answer is shown.
    public void Load()
    {
        int loadNumber = ++_lastLoadNumber;

        IsLoading = true;
        HasLoadError = false;
        ShowJoinError(null);

        BackgroundJob<int, List<LobbySummary>> job = new()
        {
            Work = GetPublicLobbiesOnWorker,
            State = loadNumber,
            OnCompleted = OnLoadCompleted,
            OnFailed = OnLoadFailed
        };

        _loadJob = job;
        BackgroundWork.Start(job);
    }

    private List<LobbySummary> GetPublicLobbiesOnWorker(int loadNumber)
    {
        return _lobbyClient.GetPublicLobbies();
    }

    private void OnLoadCompleted(List<LobbySummary> summaries, int loadNumber)
    {
        if (loadNumber == _lastLoadNumber)
        {
            _loadJob = null;
            ShowLobbies(summaries);
            IsLoading = false;
        }
    }

    private void OnLoadFailed(Exception error, int loadNumber)
    {
        if (loadNumber == _lastLoadNumber)
        {
            _loadJob = null;
            HasLoadError = true;
            IsLoading = false;
        }
    }

    protected override void OnLanguageChanged()
    {
        foreach (PublicLobbyItem lobby in Lobbies)
        {
            lobby.Refresh();
        }

        base.OnLanguageChanged();
    }

    [RelayCommand]
    private void Refresh()
    {
        Load();
    }

    [RelayCommand(CanExecute = nameof(CanJoinLobby))]
    private void JoinLobby(PublicLobbyItem lobby)
    {
        IsJoining = true;
        lobby.IsJoining = true;
        HasConnectionError = false;
        ShowJoinError(null);

        PublicLobbyJoinRequest request = new(lobby, LobbyModelFactory.CreateLocalMember(_playerSession));

        BackgroundJob<PublicLobbyJoinRequest, LobbyJoinResponse> job = new()
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

    private LobbyJoinResponse JoinLobbyOnWorker(PublicLobbyJoinRequest request)
    {
        return _lobbyClient.JoinLobby(request.Lobby.Code, request.Player);
    }

    private void OnJoinLobbyCompleted(LobbyJoinResponse response, PublicLobbyJoinRequest request)
    {
        _joinJob = null;
        FinishJoin(request.Lobby);
        _lobbySession.Current = LobbyModelFactory.CreateJoinedLobby(response, _playerSession);
        _navigator.NavigateTo<LobbyViewModel>();
    }

    private void OnJoinLobbyFailed(Exception error, PublicLobbyJoinRequest request)
    {
        _joinJob = null;
        FinishJoin(request.Lobby);

        if (error is FaultException<ServiceFault> fault)
        {
            ShowJoinError(LobbyJoinErrors.FromMessageKey(
                fault.Detail.MessageKey,
                request.Lobby.CurrentPlayers,
                request.Lobby.MaxPlayers));
        }
        else
        {
            HasConnectionError = true;
        }
    }

    // The user left the screen before the answer arrived, but the server already accepted the join: undo it.
    private void OnJoinLobbyDiscarded(LobbyJoinResponse response, PublicLobbyJoinRequest request)
    {
        LobbyLeaveRequest leave = new(request.Lobby.Code, request.Player.PlayerId);

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

    public void CancelPendingWork()
    {
        if (_loadJob is not null)
        {
            _loadJob.Cancel();
            _loadJob = null;
            IsLoading = false;
        }

        if (_joinJob is not null)
        {
            FinishJoin(_joinJob.State.Lobby);
            _joinJob.Cancel();
            _joinJob = null;
        }
    }

    private void FinishJoin(PublicLobbyItem lobby)
    {
        lobby.IsJoining = false;
        IsJoining = false;
    }

    private bool CanJoinLobby(PublicLobbyItem? lobby)
    {
        return lobby is not null && !lobby.IsFull && !IsJoining;
    }

    private void ShowLobbies(List<LobbySummary> summaries)
    {
        Lobbies.Clear();

        foreach (LobbySummary summary in summaries)
        {
            Lobbies.Add(new PublicLobbyItem
            {
                Code = summary.LobbyCode,
                HostUsername = summary.HostUsername,
                CurrentPlayers = summary.CurrentPlayers,
                MaxPlayers = summary.MaxPlayers,
                JoinCommand = JoinLobbyCommand
            });
        }

        OnPropertyChanged(nameof(ShowsList));
        OnPropertyChanged(nameof(ShowsEmptyState));
    }

    private void ShowJoinError(LobbyJoinResult? error)
    {
        _lastJoinError = error;

        OnPropertyChanged(nameof(JoinErrorText));
        OnPropertyChanged(nameof(HasJoinError));
    }
}
