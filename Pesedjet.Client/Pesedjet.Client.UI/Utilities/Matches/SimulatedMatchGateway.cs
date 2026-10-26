using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Session;

namespace Pesedjet.Client.UI.Utilities.Matches;

// TODO: Replace this simulation with the real gateway that talks to the server (PLAY_CARD, round and match events,
// CU-023 to CU-029). The server decides everything (RNF-10); this class only lets the match screens be used and tested
// before the server exists: the first play ends round 3 and the second one ends the match.
public sealed class SimulatedMatchGateway : IMatchGateway
{
    private const int SimulatedDelayMilliseconds = 400;
    private const int StartingRound = 3;
    private const int FinalRound = 4;
    private const int TurnSeconds = 180;
    private const int MeStartingPoints = 13;
    private const int MeStartingAmulets = 2;
    private const int RoundWinPoints = 5;
    private const int RoundWinAmulets = 1;
    private const int NeftisHitPoints = 3;
    private const int SetAsideAndFaceUpTotal = 21;
    private const int MaximumOpponents = 5;
    private const int NextRoundDeckCount = 9;
    private const int WinnerPoints = 38;
    private const int WinnerAmulets = 4;

    private static readonly int[] OpponentPoints = [6, 11, 17, 9, 14];
    private static readonly int[][] OpponentDiscards = [[4], [3, 2], [0, 1], [5], [6, 1]];
    private static readonly int[] AmuletGoalsByPlayers = [6, 5, 4, 3, 3];
    private static readonly int[] StartingHand = [9, 1];
    private static readonly int[] NextRoundHand = [4, 7];

    private readonly IPlayerSession _playerSession;

    private RoomSnapshot? _room;
    private int _round = StartingRound;
    private int _goal;
    private int _mePoints = MeStartingPoints;
    private int _meAmulets = MeStartingAmulets;
    private bool _isMeOut;
    private List<PlayerSnapshot> _opponents = new();
    private List<MatchLogEntry> _log = new();

    public SimulatedMatchGateway(IPlayerSession playerSession)
    {
        _playerSession = playerSession;
    }

    public Task<MatchSnapshot> StartAsync(RoomSnapshot room)
    {
        _room = room;
        _round = StartingRound;
        _goal = AmuletGoalsByPlayers[Math.Clamp(room.Members.Count, 2, 6) - 2];
        _mePoints = MeStartingPoints;
        _meAmulets = MeStartingAmulets;
        _isMeOut = false;
        _opponents = BuildOpponents(room);
        _log =
        [
            new MatchLogEntry(MatchLogKind.Normal, Format("Match.Log.Start", _round, OwnName())),
            new MatchLogEntry(MatchLogKind.Hint, Text("Match.Log.DrewCard"))
        ];

        return Task.FromResult(BuildSnapshot(StartingHand, true));
    }

    public async Task<TurnOutcome> PlayCardAsync(PlayRequest request)
    {
        await Task.Delay(SimulatedDelayMilliseconds);

        _log = RemoveHints(_log);

        TurnOutcome outcome;

        if (request.CardValue == DeityCatalog.AtumRaValue)
        {
            _isMeOut = true;
            _log.Add(new MatchLogEntry(MatchLogKind.Loss, Format("Match.Log.AtumRa", OwnName())));
            outcome = new TurnOutcome(BuildSnapshot([], false));
        }
        else
        {
            MatchNotice? notice = ApplyPlay(request);

            outcome = BuildRoundEnd(notice);
        }

        return outcome;
    }

    public async Task<TurnOutcome> AdvanceAsync()
    {
        await Task.Delay(SimulatedDelayMilliseconds);

        return BuildRoundEnd(null);
    }

    public Task<MatchSnapshot> StartNextRoundAsync()
    {
        _round++;
        _isMeOut = false;
        _opponents = BuildOpponents(_room!);
        _log =
        [
            new MatchLogEntry(MatchLogKind.Normal, Format("Match.Log.Start", _round, OwnName())),
            new MatchLogEntry(MatchLogKind.Hint, Text("Match.Log.DrewCard"))
        ];

        return Task.FromResult(BuildSnapshot(NextRoundHand, true, NextRoundDeckCount));
    }

    private static List<MatchLogEntry> RemoveHints(List<MatchLogEntry> log)
    {
        List<MatchLogEntry> kept = new();

        foreach (MatchLogEntry entry in log)
        {
            if (!entry.IsHint)
            {
                kept.Add(entry);
            }
        }

        return kept;
    }

    private MatchNotice? ApplyPlay(PlayRequest request)
    {
        DeityCard card = DeityCatalog.Find(request.CardValue);
        MatchNotice? notice = null;

        if (request.Target is null)
        {
            _log.Add(new MatchLogEntry(MatchLogKind.Normal, Format("Match.Log.Played", OwnName(), card.Name)));
        }
        else if (request.NamedDeity is not null)
        {
            bool isHit = request.NamedDeity == HiddenCardOf(request.Target);
            LocalizedText result = Text(isHit ? "Match.Log.Hit" : "Match.Log.Miss");

            _log.Add(new MatchLogEntry(
                MatchLogKind.Normal,
                Format(
                    "Match.Log.PlayedNamed",
                    OwnName(),
                    request.Target,
                    DeityCatalog.Find(request.NamedDeity.Value).Name,
                    result)));

            if (isHit)
            {
                _mePoints += NeftisHitPoints;
                _log.Add(new MatchLogEntry(MatchLogKind.Gain, Format("Match.Log.Points", NeftisHitPoints, OwnName())));
            }
        }
        else
        {
            _log.Add(new MatchLogEntry(
                MatchLogKind.Normal,
                Format("Match.Log.PlayedOn", OwnName(), card.Name, request.Target)));
            notice = BuildSecretNotice(card, request.Target);
        }

        return notice;
    }

    private MatchNotice? BuildSecretNotice(DeityCard card, string target)
    {
        MatchNotice? notice = null;
        int hidden = HiddenCardOf(target);
        string cardName = DeityCatalog.Find(hidden).Name;

        if (card.Value == 2)
        {
            notice = new MatchNotice(
                Format("Match.Notice.SethTitle", target),
                Format("Match.Notice.SethMessage", target, cardName, hidden),
                hidden);
        }
        else if (card.Value == 3)
        {
            notice = new MatchNotice(
                Format("Match.Notice.IsisTitle", target),
                Format("Match.Notice.IsisMessage", cardName, hidden),
                hidden);
        }

        return notice;
    }

    private TurnOutcome BuildRoundEnd(MatchNotice? notice)
    {
        bool isFinal = _round >= FinalRound;

        if (!_isMeOut)
        {
            _mePoints += RoundWinPoints;
            _meAmulets = isFinal ? WinnerAmulets : _meAmulets + RoundWinAmulets;
        }

        MatchSnapshot snapshot = BuildSnapshot([], false);
        RoundSummary summary = BuildSummary();
        MatchResult? result = isFinal ? BuildResult() : null;

        return new TurnOutcome(snapshot, notice, summary, result);
    }

    private RoundSummary BuildSummary()
    {
        string me = OwnName();
        List<RoundSummaryRow> rows = new();
        string winner = _isMeOut ? FirstOpponentName() : me;

        rows.Add(new RoundSummaryRow(
            me,
            _playerSession.CurrentPlayer?.AvatarId,
            _isMeOut ? null : 8,
            _isMeOut ? Text("Match.Summary.OutOfRound") : Format("Match.Summary.WinsRound", "Shu"),
            _isMeOut ? 0 : RoundWinPoints,
            _meAmulets,
            _goal,
            _mePoints,
            true,
            !_isMeOut,
            _isMeOut));

        foreach (PlayerSnapshot opponent in _opponents)
        {
            bool isOut = opponent.Status == PlayerStatus.OutOfRound;

            rows.Add(new RoundSummaryRow(
                opponent.Username,
                opponent.AvatarId,
                isOut ? null : HiddenCardOf(opponent.Username),
                isOut ? Format("Match.Summary.OutAtTurn", 4) : Text("Match.Summary.KeptHand"),
                0,
                opponent.Amulets,
                _goal,
                opponent.Points,
                false,
                !_isMeOut && false,
                isOut));
        }

        return new RoundSummary(
            _round,
            Text("Match.Summary.CauseDeckEmpty"),
            rows,
            Format("Match.Summary.NextRound", winner));
    }

    private MatchResult BuildResult()
    {
        string me = OwnName();
        List<MatchResultRow> rows = new()
        {
            new MatchResultRow(
                1,
                me,
                _playerSession.CurrentPlayer?.AvatarId,
                WinnerAmulets,
                _goal,
                WinnerPoints,
                null,
                true,
                true)
        };

        int place = 2;

        foreach (PlayerSnapshot opponent in _opponents)
        {
            LocalizedText? withdrawn = place == _opponents.Count + 1 ? Text("Match.Result.WithdrawnDisconnection") : null;

            rows.Add(new MatchResultRow(
                place,
                opponent.Username,
                opponent.AvatarId,
                Math.Max(1, WinnerAmulets - place + 1),
                _goal,
                opponent.Points + RoundWinPoints,
                withdrawn,
                false,
                false));
            place++;
        }

        return new MatchResult(me, _round + 1, _goal, rows);
    }

    private List<PlayerSnapshot> BuildOpponents(RoomSnapshot room)
    {
        List<PlayerSnapshot> opponents = new();
        string me = OwnName();
        int index = 0;

        foreach (RoomMember member in room.Members)
        {
            if (member.Username != me && index < MaximumOpponents)
            {
                PlayerStatus status = index switch
                {
                    0 => PlayerStatus.Protected,
                    1 => PlayerStatus.OutOfRound,
                    _ => PlayerStatus.Active
                };

                opponents.Add(new PlayerSnapshot(
                    member.Username,
                    member.AvatarId,
                    OpponentPoints[index],
                    index + 1,
                    status,
                    OpponentDiscards[index],
                    status != PlayerStatus.OutOfRound,
                    false));
                index++;
            }
        }

        return opponents;
    }

    private MatchSnapshot BuildSnapshot(IReadOnlyList<int> hand, bool isMyTurn, int deckCount = NextRoundDeckCount)
    {
        PlayerSnapshot me = new(
            OwnName(),
            _playerSession.CurrentPlayer?.AvatarId,
            _mePoints,
            _meAmulets,
            _isMeOut ? PlayerStatus.OutOfRound : PlayerStatus.ImmuneToNeftis,
            [1, 8],
            false,
            isMyTurn);

        return new MatchSnapshot(
            _round,
            _goal,
            OwnName(),
            isMyTurn,
            _opponents,
            me,
            hand,
            deckCount,
            true,
            [],
            _room!.Members.Count == 2,
            BuildUnseen(),
            new List<MatchLogEntry>(_log));
    }

    private static List<UnseenDeity> BuildUnseen()
    {
        int[] remaining = [1, 0, 1, 2, 2, 1, 1, 1, 4, 1];
        List<UnseenDeity> unseen = new();

        foreach (DeityCard card in DeityCatalog.All)
        {
            unseen.Add(new UnseenDeity(card.Value, remaining[DeityCatalog.MaximumValue - card.Value], card.Copies));
        }

        return unseen;
    }

    // Pretend hidden hands so that naming Nut against Nefer12 hits, like in the prototype.
    private static int HiddenCardOf(string username)
    {
        int value = username switch
        {
            "Anubis77" => 3,
            "Nefer12" => 5,
            "IsisVera" => 2,
            _ => 4
        };

        return value;
    }

    private string FirstOpponentName()
    {
        return _opponents.Count > 0 ? _opponents[0].Username : string.Empty;
    }

    private string OwnName()
    {
        return _playerSession.GuestAlias ?? _playerSession.CurrentPlayer?.Username ?? string.Empty;
    }

    private static LocalizedText Text(string key)
    {
        return new LocalizedText(key);
    }

    private static LocalizedText Format(string key, params object[] arguments)
    {
        return new LocalizedText(key, arguments);
    }
}
