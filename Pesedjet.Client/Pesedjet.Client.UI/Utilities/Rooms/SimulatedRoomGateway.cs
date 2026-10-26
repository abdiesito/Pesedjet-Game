using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Session;

namespace Pesedjet.Client.UI.Utilities.Rooms;

// TODO: Replace this simulation with the real gateway that talks to the server (CREATE_ROOM_REQUEST and
// JOIN_ROOM_REQUEST, CU-012, CU-013). It lets the waiting room be used, public and private, before the server exists.
// Codes to try when joining: PRIV01 (private room), NOEXIS (does not exist), LLENA1 (full), EMPEZO (already started);
// any other valid code joins a public room.
public sealed class SimulatedRoomGateway : IRoomGateway
{
    private const int SimulatedDelayMilliseconds = 600;
    private const string CreatedCode = "K7Q2PX";
    private const string PrivateCode = "PRIV01";
    private const string NotFoundCode = "NOEXIS";
    private const string FullCode = "LLENA1";
    private const string StartedCode = "EMPEZO";
    private const string HostName = "RaSol";
    private const string FirstJoinerName = "Anubis77";
    private const string SecondJoinerName = "IsisVera";
    private const int FullRoomMinimumCapacity = 5;

    private static readonly string[] ExtraJoinerNames = ["HorusX", "TotEscriba", "Nefer12", "MaatJusta"];
    private const string JoinedNoticeKey = "WaitingRoom.JoinedNotice";
    private const string HostMessageKey = "WaitingRoom.SampleHostMessage";
    private const string JoinerMessageKey = "WaitingRoom.SampleJoinerMessage";
    private const string SecondJoinerMessageKey = "WaitingRoom.SampleSecondJoinerMessage";

    private readonly IPlayerSession _playerSession;

    public SimulatedRoomGateway(IPlayerSession playerSession)
    {
        _playerSession = playerSession;
    }

    public async Task<RoomSnapshot> CreateRoomAsync(int capacity, RoomVisibility visibility)
    {
        await Task.Delay(SimulatedDelayMilliseconds);

        string me = GetOwnName();
        List<RoomMember> members =
        [
            new RoomMember(me, GetOwnAvatar(), true),
            new RoomMember(FirstJoinerName, 4, false)
        ];
        List<RoomChatEntry> chat = [BuildJoinNotice(FirstJoinerName)];

        if (!_playerSession.IsGuest && visibility == RoomVisibility.Public && capacity > members.Count)
        {
            members.Add(new RoomMember(SecondJoinerName, 5, false));
            chat.Add(new RoomChatEntry(RoomChatKind.Other, FirstJoinerName, string.Empty, new LocalizedText(JoinerMessageKey)));
            chat.Add(BuildJoinNotice(SecondJoinerName));
        }

        // Rooms of 5 or 6 players are filled to the top so the room and the table can be seen at their fullest.
        if (capacity >= FullRoomMinimumCapacity)
        {
            AddExtraJoiners(members, chat, capacity);
        }

        return new RoomSnapshot(CreatedCode, capacity, visibility, true, members, chat);
    }

    public async Task<RoomJoinResult> JoinRoomAsync(string code)
    {
        await Task.Delay(SimulatedDelayMilliseconds);

        RoomJoinResult result = new(RoomCodeError.None, 0, 0, BuildJoinedRoom(code));

        if (code == NotFoundCode)
        {
            result = new RoomJoinResult(RoomCodeError.NotFound, 0, 0);
        }
        else if (code == FullCode)
        {
            result = new RoomJoinResult(RoomCodeError.Full, 4, 4);
        }
        else if (code == StartedCode)
        {
            result = new RoomJoinResult(RoomCodeError.Started, 0, 0);
        }

        return result;
    }

    private static void AddExtraJoiners(List<RoomMember> members, List<RoomChatEntry> chat, int capacity)
    {
        bool hasSecondJoiner = false;

        foreach (RoomMember member in members)
        {
            hasSecondJoiner = hasSecondJoiner || member.Username == SecondJoinerName;
        }

        if (!hasSecondJoiner)
        {
            members.Add(new RoomMember(SecondJoinerName, 5, false));
            chat.Add(BuildJoinNotice(SecondJoinerName));
        }

        foreach (string name in ExtraJoinerNames)
        {
            if (members.Count < capacity)
            {
                members.Add(new RoomMember(name, null, false));
                chat.Add(BuildJoinNotice(name));
            }
        }
    }

    private RoomSnapshot BuildJoinedRoom(string code)
    {
        RoomVisibility visibility = code == PrivateCode ? RoomVisibility.Private : RoomVisibility.Public;
        List<RoomMember> members =
        [
            new RoomMember(HostName, 0, true),
            new RoomMember(GetOwnName(), GetOwnAvatar(), false),
            new RoomMember(SecondJoinerName, 5, false)
        ];
        List<RoomChatEntry> chat =
        [
            BuildJoinNotice(GetOwnName()),
            new RoomChatEntry(RoomChatKind.Other, HostName, string.Empty, new LocalizedText(HostMessageKey)),
            BuildJoinNotice(SecondJoinerName),
            new RoomChatEntry(RoomChatKind.Other, SecondJoinerName, string.Empty, new LocalizedText(SecondJoinerMessageKey))
        ];

        return new RoomSnapshot(code, 4, visibility, false, members, chat);
    }

    private string GetOwnName()
    {
        return _playerSession.GuestAlias ?? _playerSession.CurrentPlayer?.Username ?? string.Empty;
    }

    private int? GetOwnAvatar()
    {
        return _playerSession.CurrentPlayer?.AvatarId;
    }

    private static RoomChatEntry BuildJoinNotice(string username)
    {
        return new RoomChatEntry(RoomChatKind.System, string.Empty, string.Empty, new LocalizedText(JoinedNoticeKey, username));
    }

}
