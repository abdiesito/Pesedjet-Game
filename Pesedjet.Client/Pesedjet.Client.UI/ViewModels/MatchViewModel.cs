using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Matches;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Rooms;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Timing;

namespace Pesedjet.Client.UI.ViewModels;

// GUI-009: the table. It is the only screen that fills the whole window; it has no top bar of its own (the guest
// indicator is part of its status bar, RN22).
public partial class MatchViewModel : ViewModelBase, INavigable, IDisposable
{
    private const int TurnSeconds = 180;
    private const int WarningSeconds = 30;
    private const int SecondsPerMinute = 60;
    private const int ObserverAdvanceSeconds = 3;
    private const int MaximumMessageLength = 200;
    private const int MaximumAmulets = 6;
    private const string YourTurnKey = "Match.YourTurn";
    private const string TurnOfKey = "Match.TurnOf";
    private const string RoundKey = "Match.Round";
    private const string DeckKey = "Match.Deck";
    private const string PointsShortKey = "Match.Points";
    private const string YouKey = "Match.You";
    private const string ScoreboardKey = "Match.Scoreboard";
    private const string KeyKey = "Match.Key";
    private const string HintChooseKey = "Match.HintChoose";
    private const string HintWaitKey = "Match.HintWait";
    private const string HintOutKey = "Match.HintOut";
    private const string AmuletsOfKey = "Match.AmuletsOf";
    private const string PointsKey = "Match.GamePoints";
    private const string StatusProtectedKey = "Match.Status.Protected";
    private const string StatusImmuneKey = "Match.Status.ImmuneToNeftis";
    private const string StatusOutKey = "Match.Status.OutOfRound";
    private const string StatusNotRespondingKey = "Match.Status.NotResponding";
    private const string StatusExpelledKey = "Match.Status.Expelled";
    private const string StatusDisconnectedKey = "Match.Status.Disconnected";

    private readonly INavigator _navigator;
    private readonly IDialogManager _dialogManager;
    private readonly IPlayerSession _playerSession;
    private readonly IRoomSession _roomSession;
    private readonly IMatchGateway _matchGateway;
    private readonly TimeProvider _timeProvider;
    private readonly Countdown _turnCountdown;

    private MatchSnapshot? _snapshot;

    [ObservableProperty]
    private string _roundText = string.Empty;

    [ObservableProperty]
    private string _turnText = string.Empty;

    [ObservableProperty]
    private string _timerText = string.Empty;

    [ObservableProperty]
    private double _timerFraction = 1;

    [ObservableProperty]
    private bool _isTimerWarning;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnreadMessages), nameof(UnreadText))]
    private int _unreadCount;

    [ObservableProperty]
    private bool _isChatOpen;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
    private string _chatText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPlay), nameof(ShowsAtumRaWarning), nameof(SelectedCardText))]
    [NotifyCanExecuteChangedFor(nameof(PlayCardCommand))]
    private HandCardItem? _selectedCard;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPlay))]
    [NotifyCanExecuteChangedFor(nameof(PlayCardCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPlay), nameof(IsObserving))]
    [NotifyCanExecuteChangedFor(nameof(PlayCardCommand))]
    private bool _isMyTurn;

    [ObservableProperty]
    private string _myName = string.Empty;

    [ObservableProperty]
    private string _myTitle = string.Empty;

    [ObservableProperty]
    private string _scoreboardTitle = string.Empty;

    [ObservableProperty]
    private string _myAmuletsText = string.Empty;

    [ObservableProperty]
    private string _myPointsText = string.Empty;

    [ObservableProperty]
    private string _myStatusText = string.Empty;

    [ObservableProperty]
    private string _handHint = string.Empty;

    [ObservableProperty]
    private string _deckText = string.Empty;

    [ObservableProperty]
    private bool _hasSetAside;

    [ObservableProperty]
    private bool _isTwoPlayerMatch;

    public ObservableCollection<MatchOpponentItem> Opponents { get; } = new();

    public ObservableCollection<HandCardItem> Hand { get; } = new();

    public ObservableCollection<MatchLogEntry> Log { get; } = new();

    public ObservableCollection<UnseenItem> Unseen { get; } = new();

    public ObservableCollection<StandingItem> Standings { get; } = new();

    public ObservableCollection<RoomChatEntry> ChatMessages { get; } = new();

    public IReadOnlyList<int> MyDiscards { get; private set; } = [];

    public IReadOnlyList<bool> MyAmuletSlots { get; private set; } = [];

    public int AmuletGoal => _snapshot?.AmuletGoal ?? 0;

    public TopBarViewModel TopBar { get; }

    // The table fills the window, so the guest indicator travels inside its status bar and the common bar is hidden.
    public bool ShowsTopBar => false;

    // The guest indicator is drawn inside the status bar of the table, so it scales with it.
    public bool ShowsGuestIndicator => false;

    public bool IsGuest => _playerSession.IsGuest;

    public bool IsObserving => !IsMyTurn;

    public bool IsOut => _snapshot?.Me.Status == PlayerStatus.OutOfRound;

    public bool HasMyStatus => MyStatusText.Length > 0;

    public bool HasUnreadMessages => UnreadCount > 0;

    public string UnreadText => UnreadCount.ToString(CultureInfo.CurrentCulture);

    public bool CanPlay => IsMyTurn && !IsBusy && SelectedCard is not null;

    // The warning about Atum-Ra appears while it is in the hand and a different card is selected: when Atum-Ra itself
    // is selected, the effect line already says it.
    public bool ShowsAtumRaWarning => HandHasAtumRa() && SelectedCard?.Card.Value != DeityCatalog.AtumRaValue;

    public string SelectedCardText => SelectedCard is null
        ? string.Empty
        : $"{SelectedCard.Card.Name} ({SelectedCard.Card.Value}) · "
          + LocalizationManager.Instance["Card.Effect." + SelectedCard.Card.Value];

    public MatchViewModel(
        INavigator navigator,
        IDialogManager dialogManager,
        IPlayerSession playerSession,
        IRoomSession roomSession,
        IMatchGateway matchGateway,
        TopBarViewModel topBar,
        TimeProvider timeProvider)
    {
        _navigator = navigator;
        _dialogManager = dialogManager;
        _playerSession = playerSession;
        _roomSession = roomSession;
        _matchGateway = matchGateway;
        _timeProvider = timeProvider;
        TopBar = topBar;
        _turnCountdown = new Countdown(timeProvider, HandleTurnTick, HandleTurnTimeout);
        LocalizationManager.Instance.LanguageChanged += HandleLanguageChanged;
    }

    public void OnNavigatedTo()
    {
        _ = StartMatchAsync();
    }

    public void Dispose()
    {
        _turnCountdown.Dispose();
        LocalizationManager.Instance.LanguageChanged -= HandleLanguageChanged;
    }

    [RelayCommand]
    private void SelectCard(HandCardItem item)
    {
        if (IsMyTurn && !IsBusy)
        {
            foreach (HandCardItem card in Hand)
            {
                card.IsSelected = card == item;
            }

            SelectedCard = item;
        }
    }

    [RelayCommand]
    private void ToggleChat()
    {
        IsChatOpen = !IsChatOpen;

        if (IsChatOpen)
        {
            UnreadCount = 0;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSendMessage))]
    private void SendMessage()
    {
        // TODO: Send the message to the server (CU-032); a guest has the chat of his match, not a general one.
        ChatMessages.Add(new RoomChatEntry(RoomChatKind.Own, MyName, ChatText.Trim()));
        ChatText = string.Empty;
    }

    [RelayCommand]
    private async Task ShowCardHelpAsync()
    {
        if (SelectedCard is not null)
        {
            int remaining = FindRemaining(SelectedCard.Card.Value);

            await _dialogManager.ShowDialogAsync<CardHelpViewModel, CardHelpRequest>(
                new CardHelpRequest(SelectedCard.Card.Value, remaining));
        }
    }

    [RelayCommand(CanExecute = nameof(CanPlay))]
    private async Task PlayCardAsync()
    {
        PlayRequest? request = await BuildRequestAsync(SelectedCard!.Card);

        if (request is not null)
        {
            IsBusy = true;

            // TODO: If the server does not answer in 5 seconds the card returns to the hand and MSG-GEN-001 shows.
            TurnOutcome outcome = await _matchGateway.PlayCardAsync(request);

            IsBusy = false;
            await HandleOutcomeAsync(outcome);
        }
    }

    [RelayCommand]
    private async Task OpenMenuAsync()
    {
        MatchMenuViewModel menu = await _dialogManager.ShowDialogAsync<MatchMenuViewModel>();

        if (menu.Choice == MatchChoice.Settings)
        {
            await _dialogManager.ShowDialogAsync<SettingsViewModel>();
        }
        else if (menu.Choice == MatchChoice.Abandon)
        {
            await AbandonAsync();
        }
    }

    private bool CanSendMessage()
    {
        return ChatText.Trim().Length > 0 && ChatText.Length <= MaximumMessageLength;
    }

    private async Task StartMatchAsync()
    {
        IsBusy = true;

        MatchSnapshot snapshot = await _matchGateway.StartAsync(_roomSession.Current!);

        IsBusy = false;
        ApplySnapshot(snapshot);
    }

    private async Task<PlayRequest?> BuildRequestAsync(DeityCard card)
    {
        PlayRequest? request = null;

        if (card.Value == DeityCatalog.AtumRaValue)
        {
            PlayerActionConfirmationViewModel confirmation =
                await _dialogManager.ShowDialogAsync<PlayerActionConfirmationViewModel, PlayerActionRequest>(
                    new PlayerActionRequest(PlayerAction.PlayAtumRa, card.Name));

            request = confirmation.IsConfirmed ? new PlayRequest(card.Value, null, null) : null;
        }
        else if (card.NeedsTarget)
        {
            ObjectiveSelectionViewModel selection =
                await _dialogManager.ShowDialogAsync<ObjectiveSelectionViewModel, ObjectiveRequest>(
                    new ObjectiveRequest(card.Value, _snapshot!.Opponents));

            request = selection.IsConfirmed
                ? new PlayRequest(card.Value, selection.TargetName, selection.NamedDeity)
                : null;
        }
        else
        {
            request = new PlayRequest(card.Value, null, null);
        }

        return request;
    }

    private async Task HandleOutcomeAsync(TurnOutcome outcome)
    {
        ApplySnapshot(outcome.Snapshot);

        if (outcome.Notice is not null)
        {
            await _dialogManager.ShowDialogAsync<MatchNoticeViewModel, MatchNotice>(outcome.Notice);
        }

        if (outcome.Summary is not null)
        {
            await FinishRoundAsync(outcome);
        }
        else if (IsOut)
        {
            // Out of the round the player only observes: the server moves the match on.
            await Task.Delay(TimeSpan.FromSeconds(ObserverAdvanceSeconds), _timeProvider);
            await HandleOutcomeAsync(await _matchGateway.AdvanceAsync());
        }
    }

    private async Task FinishRoundAsync(TurnOutcome outcome)
    {
        _turnCountdown.Stop();

        await _dialogManager.ShowDialogAsync<RoundSummaryViewModel, RoundSummary>(outcome.Summary!);

        if (outcome.Result is not null)
        {
            await _dialogManager.ShowDialogAsync<MatchResultViewModel, MatchResult>(outcome.Result);
        }
        else
        {
            ApplySnapshot(await _matchGateway.StartNextRoundAsync());
        }
    }

    private async Task AbandonAsync()
    {
        PlayerActionConfirmationViewModel confirmation =
            await _dialogManager.ShowDialogAsync<PlayerActionConfirmationViewModel, PlayerActionRequest>(
                new PlayerActionRequest(PlayerAction.AbandonMatch, string.Empty));

        if (confirmation.IsConfirmed)
        {
            // TODO: Tell the server the player abandoned: he keeps the points obtained and cannot come back (CU-031).
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

    private void ApplySnapshot(MatchSnapshot snapshot, bool isRefreshOnly = false)
    {
        _snapshot = snapshot;

        RoundText = string.Format(CultureInfo.CurrentCulture, LocalizationManager.Instance[RoundKey], snapshot.Round);
        TurnText = snapshot.IsMyTurn
            ? LocalizationManager.Instance[YourTurnKey]
            : string.Format(CultureInfo.CurrentCulture, LocalizationManager.Instance[TurnOfKey], snapshot.TurnOwner);
        IsMyTurn = snapshot.IsMyTurn && snapshot.Me.Status != PlayerStatus.OutOfRound;
        MyName = snapshot.Me.Username;
        MyTitle = string.Format(CultureInfo.CurrentCulture, LocalizationManager.Instance[YouKey], MyName);
        ScoreboardTitle = string.Format(
            CultureInfo.CurrentCulture,
            LocalizationManager.Instance[ScoreboardKey],
            snapshot.AmuletGoal);
        MyAmuletsText = string.Format(
            CultureInfo.CurrentCulture,
            LocalizationManager.Instance[AmuletsOfKey],
            snapshot.Me.Amulets,
            snapshot.AmuletGoal);
        MyPointsText = string.Format(
            CultureInfo.CurrentCulture,
            LocalizationManager.Instance[PointsKey],
            snapshot.Me.Points);
        MyStatusText = StatusText(snapshot.Me.Status);
        MyDiscards = snapshot.Me.Discards;
        MyAmuletSlots = BuildSlots(snapshot.Me.Amulets);
        DeckText = string.Format(CultureInfo.CurrentCulture, LocalizationManager.Instance[DeckKey], snapshot.DeckCount);
        HasSetAside = snapshot.HasSetAsideCard;
        IsTwoPlayerMatch = snapshot.IsTwoPlayerMatch;

        LoadOpponents(snapshot);
        LoadHand(snapshot);
        LoadUnseen(snapshot);
        LoadLog(snapshot);
        LoadStandings(snapshot);

        HandHint = BuildHint(snapshot);

        OnPropertyChanged(nameof(AmuletGoal));
        OnPropertyChanged(nameof(MyDiscards));
        OnPropertyChanged(nameof(MyAmuletSlots));
        OnPropertyChanged(nameof(HasMyStatus));
        OnPropertyChanged(nameof(IsOut));
        OnPropertyChanged(nameof(IsObserving));
        OnPropertyChanged(nameof(ShowsAtumRaWarning));
        OnPropertyChanged(nameof(SelectedCardText));

        if (isRefreshOnly)
        {
            // Only the texts were rebuilt: the running turn time is left untouched.
        }
        else if (IsMyTurn)
        {
            _turnCountdown.Start(TurnSeconds);
        }
        else
        {
            _turnCountdown.Stop();
            TimerText = FormatTime(TurnSeconds);
            TimerFraction = 1;
            IsTimerWarning = false;
        }
    }

    private void LoadOpponents(MatchSnapshot snapshot)
    {
        Opponents.Clear();

        foreach (PlayerSnapshot opponent in snapshot.Opponents)
        {
            Opponents.Add(new MatchOpponentItem
            {
                Username = opponent.Username,
                AvatarId = opponent.AvatarId,
                Points = opponent.Points,
                PointsText = FormatPoints(opponent.Points),
                Amulets = opponent.Amulets,
                AmuletGoal = snapshot.AmuletGoal,
                AmuletSlots = BuildSlots(opponent.Amulets),
                StatusText = StatusText(opponent.Status),
                IsInTurn = opponent.IsInTurn,
                IsOut = opponent.Status == PlayerStatus.OutOfRound,
                HasHiddenCard = opponent.HasHiddenCard,
                Discards = opponent.Discards
            });
        }
    }

    private void LoadHand(MatchSnapshot snapshot)
    {
        Hand.Clear();
        SelectedCard = null;

        int index = 1;

        foreach (int value in snapshot.Hand)
        {
            Hand.Add(new HandCardItem
            {
                Card = DeityCatalog.Find(value),
                KeyHint = string.Format(CultureInfo.CurrentCulture, LocalizationManager.Instance[KeyKey], index),
                SelectCommand = SelectCardCommand
            });
            index++;
        }

        // The last card drawn is preselected so a single click on "Play" is enough.
        if (Hand.Count > 0 && snapshot.IsMyTurn)
        {
            SelectCard(Hand[^1]);
        }
    }

    private void LoadUnseen(MatchSnapshot snapshot)
    {
        Unseen.Clear();

        foreach (UnseenDeity deity in snapshot.Unseen)
        {
            DeityCard card = DeityCatalog.Find(deity.Value);

            Unseen.Add(new UnseenItem
            {
                Card = card,
                Remaining = deity.Remaining,
                Slots = BuildSlots(deity.Remaining, card.Copies)
            });
        }
    }

    private void LoadLog(MatchSnapshot snapshot)
    {
        Log.Clear();

        foreach (MatchLogEntry entry in snapshot.Log)
        {
            Log.Add(entry);
        }
    }

    private void LoadStandings(MatchSnapshot snapshot)
    {
        Standings.Clear();
        Standings.Add(BuildStanding(snapshot.Me, true));

        foreach (PlayerSnapshot opponent in snapshot.Opponents)
        {
            Standings.Add(BuildStanding(opponent, false));
        }
    }

    private static StandingItem BuildStanding(PlayerSnapshot player, bool isCurrentPlayer)
    {
        return new StandingItem
        {
            Username = player.Username,
            Points = player.Points,
            PointsText = FormatPoints(player.Points),
            IsCurrentPlayer = isCurrentPlayer,
            AmuletSlots = BuildSlots(player.Amulets)
        };
    }

    private string BuildHint(MatchSnapshot snapshot)
    {
        string hintKey = HintWaitKey;

        if (snapshot.Me.Status == PlayerStatus.OutOfRound)
        {
            hintKey = HintOutKey;
        }
        else if (snapshot.IsMyTurn)
        {
            hintKey = HintChooseKey;
        }

        return LocalizationManager.Instance[hintKey];
    }

    private string StatusText(PlayerStatus status)
    {
        string key = status switch
        {
            PlayerStatus.Protected => StatusProtectedKey,
            PlayerStatus.ImmuneToNeftis => StatusImmuneKey,
            PlayerStatus.OutOfRound => StatusOutKey,
            PlayerStatus.NotResponding => StatusNotRespondingKey,
            PlayerStatus.Expelled => StatusExpelledKey,
            PlayerStatus.Disconnected => StatusDisconnectedKey,
            _ => string.Empty
        };

        return key.Length == 0 ? string.Empty : LocalizationManager.Instance[key];
    }

    private static List<bool> BuildSlots(int filled)
    {
        return BuildSlots(filled, MaximumAmulets);
    }

    private static List<bool> BuildSlots(int filled, int total)
    {
        List<bool> slots = new();

        for (int slot = 0; slot < total; slot++)
        {
            slots.Add(slot < filled);
        }

        return slots;
    }

    private bool HandHasAtumRa()
    {
        bool found = false;

        foreach (HandCardItem item in Hand)
        {
            found = found || item.Card.Value == DeityCatalog.AtumRaValue;
        }

        return found;
    }

    private int FindRemaining(int value)
    {
        int remaining = 0;

        foreach (UnseenItem item in Unseen)
        {
            if (item.Card.Value == value)
            {
                remaining = item.Remaining;
            }
        }

        return remaining;
    }

    private static string FormatPoints(int points)
    {
        return string.Format(CultureInfo.CurrentCulture, LocalizationManager.Instance[PointsShortKey], points);
    }

    private static string FormatTime(int seconds)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0:00}:{1:00}",
            seconds / SecondsPerMinute,
            seconds % SecondsPerMinute);
    }

    // The texts of the table are built when a snapshot is applied, so a language change rebuilds them.
    private void HandleLanguageChanged(object? sender, EventArgs eventArgs)
    {
        if (_snapshot is not null)
        {
            int? selectedValue = SelectedCard?.Card.Value;

            ApplySnapshot(_snapshot, true);

            foreach (HandCardItem item in Hand)
            {
                if (item.Card.Value == selectedValue)
                {
                    SelectCard(item);
                }
            }
        }
    }

    private void HandleTurnTick(int remainingSeconds)
    {
        TimerText = FormatTime(Math.Max(0, remainingSeconds));
        TimerFraction = Math.Max(0, remainingSeconds) / (double)TurnSeconds;
        IsTimerWarning = remainingSeconds <= WarningSeconds;
    }

    private void HandleTurnTimeout()
    {
        _ = AskIfStillHereAsync();
    }

    private async Task AskIfStillHereAsync()
    {
        AreYouThereViewModel question = await _dialogManager.ShowDialogAsync<AreYouThereViewModel>();

        if (question.IsConfirmed && IsMyTurn)
        {
            _turnCountdown.Start(TurnSeconds);
        }
    }
}
