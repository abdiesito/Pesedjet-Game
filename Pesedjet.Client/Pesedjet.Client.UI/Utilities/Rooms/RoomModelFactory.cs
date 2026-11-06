using System.Collections.Generic;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Server.Contracts.DTOs;
using RoomMember = Pesedjet.Client.UI.Models.RoomMember;

namespace Pesedjet.Client.UI.Utilities.Rooms;

// Translates what the server sends (contract DTOs) into the models the screens draw.
public static class RoomModelFactory
{
    private const string JoinedNoticeKey = "WaitingRoom.JoinedNotice";

    public static Server.Contracts.DTOs.RoomMember CreateLocalMember(IPlayerSession playerSession)
    {
        Server.Contracts.DTOs.RoomMember member = new()
        {
            PlayerId = playerSession.PlayerId,
            Username = playerSession.GuestAlias ?? playerSession.CurrentPlayer?.Username ?? string.Empty,
            IsGuest = playerSession.IsGuest
        };

        return member;
    }

    public static RoomSnapshot CreateCreatedRoom(
        string code,
        int capacity,
        RoomVisibility visibility,
        IPlayerSession playerSession)
    {
        Server.Contracts.DTOs.RoomMember local = CreateLocalMember(playerSession);
        List<RoomMember> members = [CreateMember(local, playerSession.CurrentPlayer?.AvatarId, true)];
        List<RoomChatEntry> chat = [];

        return new RoomSnapshot(code, capacity, visibility, true, members, chat);
    }

    public static RoomSnapshot CreateJoinedRoom(RoomJoinResponse response, IPlayerSession playerSession)
    {
        Server.Contracts.DTOs.RoomMember local = CreateLocalMember(playerSession);
        List<RoomMember> members = new();

        foreach (Server.Contracts.DTOs.RoomMember existing in response.ExistingMembers)
        {
            members.Add(CreateMember(existing, null, existing.PlayerId == response.HostPlayerId));
        }

        members.Add(CreateMember(local, playerSession.CurrentPlayer?.AvatarId, local.PlayerId == response.HostPlayerId));

        RoomVisibility visibility = response.IsPublic ? RoomVisibility.Public : RoomVisibility.Private;
        bool isHost = local.PlayerId == response.HostPlayerId;
        List<RoomChatEntry> chat = [CreateJoinNotice(local.Username)];

        return new RoomSnapshot(response.RoomCode, response.MaxPlayers, visibility, isHost, members, chat);
    }

    public static RoomMember CreateMember(Server.Contracts.DTOs.RoomMember member, int? avatarId, bool isHost)
    {
        return new RoomMember(member.PlayerId, member.Username, avatarId, isHost);
    }

    public static RoomChatEntry CreateJoinNotice(string username)
    {
        return new RoomChatEntry(RoomChatKind.System, string.Empty, string.Empty, new LocalizedText(JoinedNoticeKey, username));
    }
}
