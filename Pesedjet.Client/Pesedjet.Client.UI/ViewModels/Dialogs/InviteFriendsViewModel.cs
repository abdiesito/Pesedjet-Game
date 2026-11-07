using System;
using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Client.UI.Models.DialogParameters;
using Pesedjet.Client.UI.Models.Lobbies;
using Pesedjet.Client.UI.Models.Social;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Social;
using Pesedjet.Client.UI.Utilities.Threading;
using Pesedjet.Client.UI.Utilities.Toasts;
using Pesedjet.Client.UI.ViewModels.Dialogs.Dependencies;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.UI.ViewModels.Dialogs;

public partial class InviteFriendsViewModel : DialogViewModelBase, IDialogParameter<InviteFriendsRequest>
{
    private const string FreeSlotsOneKey = "InviteFriends.FreeSlotOne";
    private const string FreeSlotsManyKey = "InviteFriends.FreeSlotsMany";
    private const string SendOneKey = "InviteFriends.SendOne";
    private const string SendManyKey = "InviteFriends.SendMany";
    private const string SendNoneKey = "InviteFriends.Send";
    private const string UnavailableKey = "InviteFriends.InMatchUnavailable";
    private const string SentToastKey = "InviteFriends.SentToast";

    private readonly ILobbyInvitationClient _lobbyInvitationClient;
    private readonly IPlayerSession _playerSession;
    private readonly IToastManager _toastManager;
    private readonly TimeProvider _timeProvider;

    private string _lobbyCode = string.Empty;
    private int _freeSlots;
    private bool _isSending;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SendLabel))]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private int _selectedCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCandidates))]
    private IReadOnlyList<InviteCandidate> _candidates = [];

    public bool HasCandidates => Candidates.Count > 0;

    public string FreeSlotsText => _freeSlots == 1
        ? LocalizationManager.Instance[FreeSlotsOneKey]
        : string.Format(CultureInfo.CurrentCulture, LocalizationManager.Instance[FreeSlotsManyKey], _freeSlots);

    public string SendLabel => SelectedCount switch
    {
        0 => LocalizationManager.Instance[SendNoneKey],
        1 => LocalizationManager.Instance[SendOneKey],
        _ => string.Format(CultureInfo.CurrentCulture, LocalizationManager.Instance[SendManyKey], SelectedCount)
    };

    public InviteFriendsViewModel(InviteFriendsDependencies dependencies)
    {
        _lobbyInvitationClient = dependencies.LobbyInvitationClient;
        _playerSession = dependencies.PlayerSession;
        _toastManager = dependencies.ToastManager;
        _timeProvider = dependencies.TimeProvider;
    }

    public void Initialize(InviteFriendsRequest parameter)
    {
        _lobbyCode = parameter.LobbyCode;
        _freeSlots = parameter.FreeSlots;

        InvitableFriendsRequest request = new InvitableFriendsRequest
        {
            PlayerId = _playerSession.PlayerId,
            LobbyCode = _lobbyCode
        };

        BackgroundJob<InvitableFriendsRequest, ClientResult<List<FriendEntry>>> job = new()
        {
            Work = GetInvitableFriendsOnWorker,
            State = request,
            OnCompleted = OnInvitableFriendsLoaded,
            OnFailed = OnInvitableFriendsFailed
        };

        BackgroundWork.Start(job);
    }

    private ClientResult<List<FriendEntry>> GetInvitableFriendsOnWorker(InvitableFriendsRequest request)
    {
        return _lobbyInvitationClient.GetInvitableFriends(request);
    }

    private void OnInvitableFriendsLoaded(ClientResult<List<FriendEntry>> result, InvitableFriendsRequest request)
    {
        if (result.IsSuccess)
        {
            ShowCandidates(result.Value!);
        }
        else
        {
            _toastManager.Show(FriendshipErrors.Describe(result));
        }
    }

    private void OnInvitableFriendsFailed(Exception error, InvitableFriendsRequest request)
    {
        OnInvitableFriendsLoaded(ClientResult<List<FriendEntry>>.TechnicalFailure(FailureKind.Unexpected), request);
    }

    private void ShowCandidates(List<FriendEntry> friends)
    {
        List<InviteCandidate> candidates = new();

        foreach (FriendEntry friend in friends)
        {
            candidates.Add(new InviteCandidate
            {
                PlayerId = friend.PlayerId,
                Username = friend.Username,
                IsAvailable = true,
                FriendPresence = PresenceStatus.Online
            });
        }

        foreach (InviteCandidate candidate in candidates)
        {
            candidate.PropertyChanged += HandleCandidateChanged;
        }

        Candidates = candidates;
        FillStatusTexts();
    }

    protected override void OnLanguageChanged()
    {
        FillStatusTexts();
        base.OnLanguageChanged();
    }

    private void FillStatusTexts()
    {
        foreach (InviteCandidate candidate in Candidates)
        {
            candidate.StatusText = candidate.IsAvailable
                ? PresenceFormatter.Format(
                    candidate.FriendPresence,
                    candidate.FriendLastSeenAt,
                    _timeProvider.GetUtcNow())
                : LocalizationManager.Instance[UnavailableKey];
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose();
    }

    [RelayCommand(CanExecute = nameof(CanSend))]
    private void Send()
    {
        if (!_isSending)
        {
            _isSending = true;

            LobbyInvitationRequest request = new LobbyInvitationRequest
            {
                SenderId = _playerSession.PlayerId,
                LobbyCode = _lobbyCode,
                TargetIds = FindSelectedIds()
            };

            BackgroundJob<LobbyInvitationRequest, ClientResult<List<Guid>>> job = new()
            {
                Work = SendInvitationsOnWorker,
                State = request,
                OnCompleted = OnInvitationsSent,
                OnFailed = OnInvitationsFailed
            };

            BackgroundWork.Start(job);
        }
    }

    private ClientResult<List<Guid>> SendInvitationsOnWorker(LobbyInvitationRequest request)
    {
        return _lobbyInvitationClient.SendLobbyInvitations(request);
    }

    private void OnInvitationsSent(ClientResult<List<Guid>> result, LobbyInvitationRequest request)
    {
        _isSending = false;

        if (result.IsSuccess)
        {
            _toastManager.Show(string.Format(
                CultureInfo.CurrentCulture,
                LocalizationManager.Instance[SentToastKey],
                result.Value!.Count));
            IsConfirmed = true;
            RequestClose();
        }
        else
        {
            _toastManager.Show(FriendshipErrors.Describe(result));
        }
    }

    private void OnInvitationsFailed(Exception error, LobbyInvitationRequest request)
    {
        OnInvitationsSent(ClientResult<List<Guid>>.TechnicalFailure(FailureKind.Unexpected), request);
    }

    private List<Guid> FindSelectedIds()
    {
        List<Guid> selectedIds = new();

        foreach (InviteCandidate candidate in Candidates)
        {
            if (candidate.IsSelected)
            {
                selectedIds.Add(candidate.PlayerId);
            }
        }

        return selectedIds;
    }

    private bool CanSend()
    {
        return SelectedCount > 0 && SelectedCount <= _freeSlots;
    }

    private void HandleCandidateChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs eventArgs)
    {
        int selected = 0;

        foreach (InviteCandidate candidate in Candidates)
        {
            if (candidate.IsSelected)
            {
                selected++;
            }
        }

        SelectedCount = selected;
    }
}
