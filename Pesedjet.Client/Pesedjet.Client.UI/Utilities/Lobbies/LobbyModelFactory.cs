using System.Collections.Generic;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Server.Contracts.DTOs;
using LobbyMember = Pesedjet.Client.UI.Models.Lobbies.LobbyMember;
using Pesedjet.Client.UI.Models.Common;
using Pesedjet.Client.UI.Models.Lobbies;

namespace Pesedjet.Client.UI.Utilities.Lobbies;

// Translates what the server sends (contract DTOs) into the models the screens draw.
public static class LobbyModelFactory
{
    private const string JoinedNoticeKey = "Lobby.JoinedNotice";

    public static Server.Contracts.DTOs.LobbyMember CreateLocalMember(IPlayerSession playerSession)
    {
        Server.Contracts.DTOs.LobbyMember member = new()
        {
            PlayerId = playerSession.PlayerId,
            Username = playerSession.GuestAlias ?? playerSession.CurrentPlayer?.Username ?? string.Empty,
            IsGuest = playerSession.IsGuest
        };

        return member;
    }

    public static LobbySnapshot CreateCreatedLobby(string code, LobbyCreationRequest request, IPlayerSession playerSession)
    {
        LobbyVisibility visibility = request.IsPublic ? LobbyVisibility.Public : LobbyVisibility.Private;

        Server.Contracts.DTOs.LobbyMember local = CreateLocalMember(playerSession);
        List<LobbyMember> members = [CreateMember(local, playerSession.CurrentPlayer?.AvatarId, true)];
        List<LobbyChatEntry> chat = [];

        return new LobbySnapshot(code, request.Capacity, visibility, true, members, chat);
    }

    public static LobbySnapshot CreateJoinedLobby(LobbyJoinResponse response, IPlayerSession playerSession)
    {
        Server.Contracts.DTOs.LobbyMember local = CreateLocalMember(playerSession);
        List<LobbyMember> members = new();

        foreach (Server.Contracts.DTOs.LobbyMember existing in response.ExistingMembers)
        {
            members.Add(CreateMember(existing, null, existing.PlayerId == response.HostPlayerId));
        }

        members.Add(CreateMember(local, playerSession.CurrentPlayer?.AvatarId, local.PlayerId == response.HostPlayerId));

        LobbyVisibility visibility = response.IsPublic ? LobbyVisibility.Public : LobbyVisibility.Private;
        bool isHost = local.PlayerId == response.HostPlayerId;
        List<LobbyChatEntry> chat = [CreateJoinNotice(local.Username)];

        return new LobbySnapshot(response.LobbyCode, response.MaxPlayers, visibility, isHost, members, chat);
    }

    public static LobbyMember CreateMember(Server.Contracts.DTOs.LobbyMember member, int? avatarId, bool isHost)
    {
        return new LobbyMember(member.PlayerId, member.Username, avatarId, isHost);
    }

    public static LobbyChatEntry CreateJoinNotice(string username)
    {
        return new LobbyChatEntry(LobbyChatKind.System, string.Empty, string.Empty, new LocalizedText(JoinedNoticeKey, username));
    }
}
