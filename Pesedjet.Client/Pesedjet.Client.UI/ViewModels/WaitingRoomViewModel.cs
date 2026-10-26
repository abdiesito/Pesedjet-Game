using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Clipboard;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Rooms;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Social;

namespace Pesedjet.Client.UI.ViewModels;

public partial class WaitingRoomViewModel : ViewModelBase, INavigable
{
    private const int MinimumPlayersToStart = 2;
    private const int MinimumCapacity = 2;
    private const int MaximumCapacity = 6;
    private const int MaximumMessageLength = 200;
    private const string SubtitleKey = "WaitingRoom.Subtitle";
    private const string PublicKey = "GameSetup.Public";
    private const string PrivateKey = "GameSetup.Private";
    private const string ParticipantsCountKey = "WaitingRoom.ParticipantsCount";
    private const string WaitingForHostKey = "WaitingRoom.WaitingForHost";
    private const string ReadOnlyKey = "WaitingRoom.ReadOnlyNotice";
    private const string GoalKey = "WaitingRoom.GoalAmulets";
    private const string NeedMorePlayersKey = "WaitingRoom.NeedMorePlayers";
    private const string GuestCannotInviteKey = "WaitingRoom.GuestCannotInvite";

    // Amulets needed to win for each capacity from 2 to 6 players (RJ-03).
    private static readonly int[] AmuletGoals = [6, 5, 4, 3, 3];

    private static readonly TimeSpan CopiedDuration = TimeSpan.FromSeconds(2);

    private readonly INavigator _navigator;
    private readonly IDialogManager _dialogManager;
    private readonly IPlayerSession _playerSession;
    private readonly IRoomSession _roomSession;
    private readonly IClipboardManager _clipboardManager;
    private readonly ISocialCircle _socialCircle;
    private readonly TimeProvider _timeProvider;
    private readonly RoomSnapshot _room;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtitle), nameof(IsPublicSelected), nameof(IsPrivateSelected))]
    private RoomVisibility _visibility;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtitle), nameof(ReadOnlySettings), nameof(GoalText), nameof(AmuletSlots))]
    private int _capacity;

    [ObservableProperty]
    private bool _isCodeCopied;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
    private string _chatText = string.Empty;

    [ObservableProperty]
    private bool _isStarting;

    public ObservableCollection<RoomParticipantItem> Participants { get; } = new();

    public ObservableCollection<RoomChatEntry> Messages { get; } = new();

    public IReadOnlyList<CapacityOption> Capacities { get; }

    public IReadOnlyList<int> EmptySlots => BuildEmptySlots();

    public bool ShowsTopBar => true;

    public string Code => _room.Code;

    public bool IsHost => _room.IsHost;

    public bool IsParticipant => !_room.IsHost;

    public bool IsGuest => _playerSession.IsGuest;

    public bool IsGuestHost => _room.IsHost && _playerSession.IsGuest;

    public bool CanInvite => !_playerSession.IsGuest;

    public bool IsPrivateAvailable => !_playerSession.IsGuest;

    public bool IsPublicSelected => Visibility == RoomVisibility.Public;

    public bool IsPrivateSelected => Visibility == RoomVisibility.Private;

    public int AmuletGoal => AmuletGoals[Capacity - MinimumCapacity];

    public IReadOnlyList<bool> AmuletSlots => BuildAmuletSlots();

    public bool CanStart => _room.IsHost && Participants.Count >= MinimumPlayersToStart && !IsStarting;

    public bool NeedsMorePlayers => _room.IsHost && Participants.Count < MinimumPlayersToStart;

    public string Subtitle => string.Format(
        CultureInfo.CurrentCulture,
        LocalizationManager.Instance[SubtitleKey],
        LocalizationManager.Instance[Visibility == RoomVisibility.Public ? PublicKey : PrivateKey],
        Participants.Count,
        Capacity);

    public string ParticipantsText => string.Format(
        CultureInfo.CurrentCulture,
        LocalizationManager.Instance[ParticipantsCountKey],
        Participants.Count,
        Capacity);

    public string FooterNote => BuildFooterNote();

    public bool HasFooterNote => FooterNote.Length > 0;

    public string HostName => FindHostName();

    public string WaitingForHostText => string.Format(
        CultureInfo.CurrentCulture,
        LocalizationManager.Instance[WaitingForHostKey],
        FindHostName());

    public string ReadOnlySettings => string.Format(
        CultureInfo.CurrentCulture,
        LocalizationManager.Instance[ReadOnlyKey],
        Capacity,
        LocalizationManager.Instance[Visibility == RoomVisibility.Public ? PublicKey : PrivateKey]);

    public string GoalText => string.Format(
        CultureInfo.CurrentCulture,
        LocalizationManager.Instance[GoalKey],
        AmuletGoal);

    public WaitingRoomViewModel(
        INavigator navigator,
        IDialogManager dialogManager,
        IPlayerSession playerSession,
        IRoomSession roomSession,
        IClipboardManager clipboardManager,
        ISocialCircle socialCircle,
        TimeProvider timeProvider)
    {
        _navigator = navigator;
        _dialogManager = dialogManager;
        _playerSession = playerSession;
        _roomSession = roomSession;
        _clipboardManager = clipboardManager;
        _socialCircle = socialCircle;
        _timeProvider = timeProvider;
        _room = roomSession.Current ?? throw new InvalidOperationException("There is no room to show.");
        _visibility = _room.Visibility;
        _capacity = _room.Capacity;

        Capacities = BuildCapacities();
        LoadMembers();

        foreach (RoomChatEntry entry in _room.Chat)
        {
            Messages.Add(entry);
        }
    }

    [RelayCommand]
    private async Task CopyCodeAsync()
    {
        await _clipboardManager.CopyTextAsync(Code);

        IsCodeCopied = true;
        await Task.Delay(CopiedDuration, _timeProvider);
        IsCodeCopied = false;
    }

    [RelayCommand]
    private void SelectCapacity(CapacityOption option)
    {
        if (IsHost && option.IsAvailable)
        {
            foreach (CapacityOption capacityOption in Capacities)
            {
                capacityOption.IsSelected = capacityOption == option;
            }

            Capacity = option.Value;
            RefreshParticipantTexts();
            // TODO: Send the new capacity to the server (CU-014 FA-01).
        }
    }

    [RelayCommand]
    private void SelectPublic()
    {
        if (IsHost)
        {
            Visibility = RoomVisibility.Public;
            // TODO: Send the new visibility to the server (CU-014).
        }
    }

    [RelayCommand]
    private void SelectPrivate()
    {
        if (IsHost && IsPrivateAvailable)
        {
            Visibility = RoomVisibility.Private;
        }
    }

    [RelayCommand]
    private async Task KickAsync(RoomParticipantItem participant)
    {
        PlayerActionConfirmationViewModel dialog =
            await _dialogManager.ShowDialogAsync<PlayerActionConfirmationViewModel, PlayerActionRequest>(
                new PlayerActionRequest(PlayerAction.KickFromRoom, participant.Username));

        if (dialog.IsConfirmed)
        {
            // TODO: Tell the server to remove the participant (CU-014 FA-02, MSG-ROOM-003).
            Participants.Remove(participant);
            RefreshParticipantTexts();
        }
    }

    [RelayCommand]
    private void ProposeKick(RoomParticipantItem participant)
    {
        // TODO: Open the expulsion proposal (MOD-007, CU-016) when the shared voting dialog exists.
    }

    [RelayCommand(CanExecute = nameof(CanInvite))]
    private async Task InviteFriendsAsync()
    {
        int freeSlots = Capacity - Participants.Count;

        await _dialogManager.ShowDialogAsync<InviteFriendsViewModel, int>(freeSlots);
    }

    [RelayCommand]
    private async Task LeaveRoomAsync()
    {
        string successor = FindSuccessorName();

        PlayerActionConfirmationViewModel dialog =
            await _dialogManager.ShowDialogAsync<PlayerActionConfirmationViewModel, PlayerActionRequest>(
                new PlayerActionRequest(PlayerAction.LeaveRoom, successor));

        if (dialog.IsConfirmed)
        {
            // TODO: Tell the server that the player left; if he was the host the role passes to the longest
            // participant (CU-014 FA-04).
            _roomSession.Current = null;

            if (IsGuest)
            {
                _navigator.NavigateAndClear<GameMenuViewModel>();
            }
            else
            {
                _navigator.NavigateAndClear<MainMenuViewModel>();
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartMatchAsync()
    {
        IsStarting = true;
        StartMatchCommand.NotifyCanExecuteChanged();

        // TODO: Send START_MATCH to the server (CU-023); it starts the match of every participant.
        await Task.CompletedTask;

        IsStarting = false;
        StartMatchCommand.NotifyCanExecuteChanged();
        _navigator.NavigateTo<MatchViewModel>();
    }

    [RelayCommand(CanExecute = nameof(CanSendMessage))]
    private void SendMessage()
    {
        // TODO: Send the message to the server (CU-032); a filtered text is not sent and is kept for correction
        // (MSG-CHAT-001).
        Messages.Add(new RoomChatEntry(RoomChatKind.Own, FindOwnName(), ChatText.Trim()));
        ChatText = string.Empty;
    }

    private bool CanSendMessage()
    {
        return ChatText.Trim().Length > 0 && ChatText.Length <= MaximumMessageLength;
    }

    private void LoadMembers()
    {
        string ownName = FindOwnName();

        foreach (RoomMember member in _room.Members)
        {
            bool isSelf = member.Username == ownName;

            Participants.Add(new RoomParticipantItem
            {
                Username = member.Username,
                AvatarId = member.AvatarId,
                IsHost = member.IsHost,
                IsCurrentPlayer = isSelf,
                CanKick = _room.IsHost && !isSelf,
                CanProposeKick = !_room.IsHost && !isSelf,
                KickCommand = KickCommand,
                ProposeKickCommand = ProposeKickCommand
            });
        }
    }

    private List<CapacityOption> BuildCapacities()
    {
        List<CapacityOption> options = new();

        for (int value = MinimumCapacity; value <= MaximumCapacity; value++)
        {
            options.Add(new CapacityOption
            {
                Value = value,
                IsSelected = value == _room.Capacity,
                IsAvailable = value >= _room.Members.Count,
                SelectCommand = SelectCapacityCommand
            });
        }

        return options;
    }

    private void RefreshParticipantTexts()
    {
        OnPropertyChanged(nameof(Subtitle));
        OnPropertyChanged(nameof(ParticipantsText));
        OnPropertyChanged(nameof(EmptySlots));
        OnPropertyChanged(nameof(CanStart));
        OnPropertyChanged(nameof(NeedsMorePlayers));
        OnPropertyChanged(nameof(FooterNote));
        OnPropertyChanged(nameof(HasFooterNote));
        StartMatchCommand.NotifyCanExecuteChanged();

        foreach (CapacityOption option in Capacities)
        {
            option.IsAvailable = option.Value >= Participants.Count;
        }
    }

    // Only one note is shown at the bottom, in order of importance, so notes never pile up.
    private string BuildFooterNote()
    {
        string note = string.Empty;

        if (_room.IsHost && Participants.Count < MinimumPlayersToStart)
        {
            note = LocalizationManager.Instance[NeedMorePlayersKey];
        }
        else if (_room.IsHost && _playerSession.IsGuest)
        {
            note = LocalizationManager.Instance[GuestCannotInviteKey];
        }
        else if (!_room.IsHost)
        {
            note = WaitingForHostText;
        }

        return note;
    }

    // System notices of the chat are texts of the language: they are put again so they are redrawn.
    protected override void OnLanguageChanged()
    {
        List<RoomChatEntry> entries = new(Messages);

        Messages.Clear();

        foreach (RoomChatEntry entry in entries)
        {
            Messages.Add(entry);
        }

        base.OnLanguageChanged();
    }

    private string FindOwnName()
    {
        return _playerSession.GuestAlias ?? _playerSession.CurrentPlayer?.Username ?? string.Empty;
    }

    private string FindHostName()
    {
        string hostName = string.Empty;

        foreach (RoomParticipantItem participant in Participants)
        {
            if (participant.IsHost)
            {
                hostName = participant.Username;
            }
        }

        return hostName;
    }

    // The host role passes to the participant who has been longest in the room: the first one after the host.
    private string FindSuccessorName()
    {
        string successor = string.Empty;

        if (_room.IsHost)
        {
            foreach (RoomParticipantItem participant in Participants)
            {
                if (!participant.IsHost && successor.Length == 0)
                {
                    successor = participant.Username;
                }
            }
        }

        return successor;
    }

    private List<int> BuildEmptySlots()
    {
        List<int> slots = new();

        for (int slot = Participants.Count; slot < Capacity; slot++)
        {
            slots.Add(slot);
        }

        return slots;
    }

    private List<bool> BuildAmuletSlots()
    {
        List<bool> slots = new();

        for (int slot = 0; slot < MaximumCapacity; slot++)
        {
            slots.Add(slot < AmuletGoal);
        }

        return slots;
    }
}
