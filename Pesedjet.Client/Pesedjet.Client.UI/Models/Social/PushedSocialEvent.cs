using Pesedjet.Server.Contracts.DTO;

namespace Pesedjet.Client.UI.Models.Social;

public sealed record PushedSocialEvent(
    FriendRequestEntry? FriendRequest,
    FriendEntry? Friend,
    LobbyInvitationEntry? LobbyInvitation,
    string? WithdrawnLobbyCode);
