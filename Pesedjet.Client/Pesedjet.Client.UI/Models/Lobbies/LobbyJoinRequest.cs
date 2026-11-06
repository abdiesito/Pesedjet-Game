using LobbyMemberDto = Pesedjet.Server.Contracts.DTOs.LobbyMember;

namespace Pesedjet.Client.UI.Models.Lobbies;

public sealed record LobbyJoinRequest(string LobbyCode, LobbyMemberDto Player);
