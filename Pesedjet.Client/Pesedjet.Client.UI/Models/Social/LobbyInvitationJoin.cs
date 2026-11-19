using LobbyMemberDto = Pesedjet.Server.Contracts.DTO.LobbyMember;

namespace Pesedjet.Client.UI.Models.Social;

public sealed record LobbyInvitationJoin(NotificationItem Item, LobbyMemberDto Player);
