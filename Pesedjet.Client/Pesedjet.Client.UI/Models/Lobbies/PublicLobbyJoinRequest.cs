using LobbyMemberDto = Pesedjet.Server.Contracts.DTOs.LobbyMember;

namespace Pesedjet.Client.UI.Models.Lobbies;

public sealed record PublicLobbyJoinRequest(PublicLobbyItem Lobby, LobbyMemberDto Player);
