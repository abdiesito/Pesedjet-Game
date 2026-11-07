using Pesedjet.Server.Contracts.Internal;
using Pesedjet.Server.DataAccess.Repositories.Interfaces;

namespace Pesedjet.Server.PlayerManagement.Friendships;

public sealed record FriendshipServiceDependencies(
    IPlayerRepository PlayerRepository,
    IFriendshipRepository FriendshipRepository,
    IBlockedPlayerRepository BlockedPlayerRepository,
    IFriendshipNotifier Notifier);
