using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.ServiceModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Client.UI.Utilities.Lobbies;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Notifications;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Social;
using Pesedjet.Client.UI.Utilities.Threading;
using Pesedjet.Client.UI.Utilities.Toasts;
using Pesedjet.Client.UI.ViewModels.Dialogs.Dependencies;
using Pesedjet.Client.UI.Models.Lobbies;
using Pesedjet.Client.UI.Models.Social;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Faults;

namespace Pesedjet.Client.UI.ViewModels.Dialogs;

public partial class NotificationsViewModel : DialogViewModelBase, IDisposable
{
    private const string PendingOneKey = "Notifications.PendingOne";
    private const string PendingManyKey = "Notifications.PendingMany";
    private const string FriendRequestKey = "Notifications.FriendRequest";
    private const string LobbyInvitationKey = "Notifications.LobbyInvitation";
    private const string AcceptKey = "Notifications.Accept";
    private const string AcceptingKey = "Notifications.Accepting";
    private const string JoinKey = "Notifications.Join";
    private const string JoiningKey = "Notifications.Joining";
    private const string FriendAcceptedToastKey = "Notifications.FriendAcceptedToast";
    private const string AlreadyInLobbyKey = "Notifications.AlreadyInLobby";
    private const string ConnectionErrorKey = "Common.ConnectionError";

    private readonly INotificationInbox _inbox;
    private readonly IFriendshipClient _friendshipClient;
    private readonly ILobbyInvitationClient _lobbyInvitationClient;
    private readonly ILobbyClient _lobbyClient;
    private readonly ILobbySession _lobbySession;
    private readonly INavigator _navigator;
    private readonly IPlayerSession _playerSession;
    private readonly ISocialCircle _socialCircle;
    private readonly IToastManager _toastManager;
    private readonly TimeProvider _timeProvider;

    public ObservableCollection<NotificationItem> Items => _inbox.Items;

    public bool IsEmpty => Items.Count == 0;

    public string PendingText => Items.Count == 1
        ? LocalizationManager.Instance[PendingOneKey]
        : string.Format(CultureInfo.CurrentCulture, LocalizationManager.Instance[PendingManyKey], Items.Count);

    public NotificationsViewModel(NotificationsDependencies dependencies)
    {
        _inbox = dependencies.Inbox;
        _friendshipClient = dependencies.FriendshipClient;
        _lobbyInvitationClient = dependencies.LobbyInvitationClient;
        _lobbyClient = dependencies.LobbyClient;
        _lobbySession = dependencies.LobbySession;
        _navigator = dependencies.Navigator;
        _playerSession = dependencies.PlayerSession;
        _socialCircle = dependencies.SocialCircle;
        _toastManager = dependencies.ToastManager;
        _timeProvider = dependencies.TimeProvider;

        foreach (NotificationItem item in Items)
        {
            FillTexts(item);
        }

        Items.CollectionChanged += HandleItemsChanged;
        _inbox.RefreshFriendRequests();
        _inbox.RefreshLobbyInvitations();
    }

    public void Dispose()
    {
        Items.CollectionChanged -= HandleItemsChanged;
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose();
    }

    [RelayCommand]
    private void Accept(NotificationItem item)
    {
        Answer(item, true);
    }

    [RelayCommand]
    private void Reject(NotificationItem item)
    {
        Answer(item, false);
    }

    private void Answer(NotificationItem item, bool isAccepted)
    {
        if (item.IsFriendRequest)
        {
            RespondFriendRequestOnServer(item, isAccepted);
        }
        else if (isAccepted)
        {
            JoinLobbyFromInvitation(item);
        }
        else
        {
            DeclineLobbyInvitationOnServer(item);
        }
    }

    private void JoinLobbyFromInvitation(NotificationItem item)
    {
        if (_lobbySession.Current is not null)
        {
            _toastManager.Show(LocalizationManager.Instance[AlreadyInLobbyKey]);
        }
        else if (!item.IsProcessing)
        {
            item.IsProcessing = true;

            LobbyInvitationJoin join = new(item, LobbyModelFactory.CreateLocalMember(_playerSession));

            BackgroundJob<LobbyInvitationJoin, LobbyJoinResponse> job = new()
            {
                Work = JoinLobbyOnWorker,
                State = join,
                OnCompleted = OnJoinCompleted,
                OnFailed = OnJoinFailed
            };

            BackgroundWork.Start(job);
        }
    }

    private LobbyJoinResponse JoinLobbyOnWorker(LobbyInvitationJoin join)
    {
        return _lobbyClient.JoinLobby(join.Item.LobbyCode, join.Player);
    }

    private void OnJoinCompleted(LobbyJoinResponse response, LobbyInvitationJoin join)
    {
        join.Item.IsProcessing = false;
        Items.Remove(join.Item);

        _lobbySession.Current = LobbyModelFactory.CreateJoinedLobby(response, _playerSession);
        RequestClose();
        _navigator.NavigateTo<LobbyViewModel>();
    }

    private void OnJoinFailed(Exception error, LobbyInvitationJoin join)
    {
        join.Item.IsProcessing = false;

        if (error is FaultException<ServiceFault> fault)
        {
            LobbyJoinResult joinResult = LobbyJoinErrors.FromMessageKey(
                fault.Detail.MessageKey,
                join.Item.Occupancy,
                join.Item.Capacity);

            _toastManager.Show(LobbyJoinErrors.Describe(joinResult));

            if (joinResult.Error == LobbyCodeError.Started)
            {
                join.Item.IsExpired = true;
            }
            else if (joinResult.Error == LobbyCodeError.NotFound)
            {
                Items.Remove(join.Item);
            }
        }
        else
        {
            _toastManager.Show(LocalizationManager.Instance[ConnectionErrorKey]);
        }
    }

    private void DeclineLobbyInvitationOnServer(NotificationItem item)
    {
        DeclineLobbyInvitationRequest request = new DeclineLobbyInvitationRequest
        {
            PlayerId = _playerSession.PlayerId,
            LobbyCode = item.LobbyCode
        };

        BackgroundAction<DeclineLobbyInvitationRequest> action = new()
        {
            Work = DeclineLobbyInvitationOnWorker,
            State = request
        };

        Items.Remove(item);
        BackgroundWork.Start(action);
    }

    private void DeclineLobbyInvitationOnWorker(DeclineLobbyInvitationRequest request)
    {
        _lobbyInvitationClient.DeclineLobbyInvitation(request);
    }

    private void RespondFriendRequestOnServer(NotificationItem item, bool isAccepted)
    {
        if (!item.IsProcessing)
        {
            item.IsProcessing = true;

            RespondFriendRequest request = new RespondFriendRequest
            {
                ResponderId = _playerSession.PlayerId,
                SenderId = item.Id,
                Accept = isAccepted
            };
            FriendResponseSubmission submission = new(item, request);

            BackgroundJob<FriendResponseSubmission, ClientResult<bool>> job = new()
            {
                Work = RespondFriendRequestOnWorker,
                State = submission,
                OnCompleted = OnRespondCompleted,
                OnFailed = OnRespondFailed
            };

            BackgroundWork.Start(job);
        }
    }

    private ClientResult<bool> RespondFriendRequestOnWorker(FriendResponseSubmission submission)
    {
        return _friendshipClient.RespondFriendRequest(submission.Request);
    }

    private void OnRespondCompleted(ClientResult<bool> result, FriendResponseSubmission submission)
    {
        submission.Item.IsProcessing = false;

        bool isAnswered = result.IsSuccess && result.Value;
        bool isNoLongerAvailable = result.Failure == FailureKind.Business
                                   && result.ErrorCode == ServiceErrorCodes.FriendRequestNotFound;

        if (isAnswered && submission.Request.Accept)
        {
            _socialCircle.AddFriend(submission.Item.Id, submission.Item.SenderName);
            _toastManager.Show(string.Format(
                CultureInfo.CurrentCulture,
                LocalizationManager.Instance[FriendAcceptedToastKey],
                submission.Item.SenderName));
        }

        if (isAnswered || isNoLongerAvailable)
        {
            Items.Remove(submission.Item);
        }

        if (!isAnswered)
        {
            _toastManager.Show(FriendshipErrors.Describe(result));
        }
    }

    private void OnRespondFailed(Exception error, FriendResponseSubmission submission)
    {
        OnRespondCompleted(ClientResult<bool>.TechnicalFailure(FailureKind.Unexpected), submission);
    }

    protected override void OnLanguageChanged()
    {
        foreach (NotificationItem item in Items)
        {
            FillTexts(item);
        }

        base.OnLanguageChanged();
    }

    private void HandleItemsChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        if (eventArgs.NewItems is not null)
        {
            foreach (NotificationItem item in eventArgs.NewItems)
            {
                FillTexts(item);
            }
        }

        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(PendingText));
    }

    private void FillTexts(NotificationItem item)
    {
        item.AcceptCommand = AcceptCommand;
        item.RejectCommand = RejectCommand;

        if (item.IsLobbyInvitation)
        {
            item.Message = string.Format(
                CultureInfo.CurrentCulture,
                LocalizationManager.Instance[LobbyInvitationKey],
                item.LobbyCode,
                item.Occupancy,
                item.Capacity);
            item.PrimaryLabel = LocalizationManager.Instance[JoinKey];
            item.ProcessingLabel = LocalizationManager.Instance[JoiningKey];
        }
        else
        {
            item.Message = LocalizationManager.Instance[FriendRequestKey];
            item.PrimaryLabel = LocalizationManager.Instance[AcceptKey];
            item.ProcessingLabel = LocalizationManager.Instance[AcceptingKey];
        }

        item.TimeAgoText = RelativeTimeFormatter.Format(item.ReceivedAt, _timeProvider.GetUtcNow());
    }
}
