using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.UI.Models.Social;

public sealed record PushedSocialEvent(
    FriendRequestEntry? FriendRequest,
    FriendEntry? Friend,
    LobbyInvitationEntry? LobbyInvitation,
    string? WithdrawnLobbyCode);
