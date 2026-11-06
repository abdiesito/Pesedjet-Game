using Pesedjet.Server.Contracts.Internal;
using Pesedjet.Server.DataAccess.Repositories.Interfaces;

namespace Pesedjet.Server.Matchmaking.Invitations;

public sealed record LobbyInvitationServiceDependencies(
    IFriendshipRepository FriendshipRepository,
    IBlockedPlayerRepository BlockedPlayerRepository,
    ILobbyDirectory LobbyDirectory,
    IFriendshipNotifier Notifier);
