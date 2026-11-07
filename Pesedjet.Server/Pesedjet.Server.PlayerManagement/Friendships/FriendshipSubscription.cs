using Pesedjet.Server.Contracts.Callbacks;

namespace Pesedjet.Server.PlayerManagement.Friendships;

public sealed record FriendshipSubscription(IFriendshipCallback Callback, string SessionId);
