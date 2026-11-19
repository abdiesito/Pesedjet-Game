using LobbyMemberDto = Pesedjet.Server.Contracts.DTO.LobbyMember;

namespace Pesedjet.Client.UI.Models.Lobbies;

public sealed record LobbyCreationRequest(LobbyMemberDto Host, bool IsPublic, int Capacity);
