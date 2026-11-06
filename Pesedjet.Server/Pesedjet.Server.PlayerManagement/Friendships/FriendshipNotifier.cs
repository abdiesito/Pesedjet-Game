using Pesedjet.Server.Contracts.Internal;
using System.Collections.Concurrent;
using CoreWCF;
using Pesedjet.Server.Contracts.Callbacks;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.PlayerManagement.Friendships;

public sealed class FriendshipNotifier : IFriendshipNotifier
{
    private readonly ConcurrentDictionary<Guid, FriendshipSubscription> _subscriptions = new();

    public void Subscribe(Guid playerId, IFriendshipCallback callback, string sessionId)
    {
        _subscriptions[playerId] = new FriendshipSubscription(callback, sessionId);
    }

    public void Unsubscribe(Guid playerId)
    {
        _subscriptions.TryRemove(playerId, out _);
    }

    public void UnsubscribeSession(string sessionId)
    {
        List<Guid> playerIds = _subscriptions
            .Where(pair => pair.Value.SessionId == sessionId)
            .Select(pair => pair.Key)
            .ToList();

        foreach (Guid playerId in playerIds)
        {
            Unsubscribe(playerId);
        }
    }

    public void NotifyFriendRequestReceived(Guid targetPlayerId, FriendRequestEntry request)
    {
        Send(targetPlayerId, SendFriendRequestReceived, request);
    }

    public void NotifyFriendshipEstablished(Guid targetPlayerId, FriendEntry friend)
    {
        Send(targetPlayerId, SendFriendshipEstablished, friend);
    }

    private static void SendFriendRequestReceived(IFriendshipCallback callback, FriendRequestEntry request)
    {
        callback.OnFriendRequestReceived(request);
    }

    private static void SendFriendshipEstablished(IFriendshipCallback callback, FriendEntry friend)
    {
        callback.OnFriendshipEstablished(friend);
    }

    private void Send<TArgument>(
        Guid targetPlayerId,
        Action<IFriendshipCallback, TArgument> notification,
        TArgument argument)
    {
        if (_subscriptions.TryGetValue(targetPlayerId, out FriendshipSubscription? subscription))
        {
            try
            {
                notification(subscription.Callback, argument);
            }
            catch (CommunicationException)
            {
                Unsubscribe(targetPlayerId);
            }
            catch (TimeoutException)
            {
                Unsubscribe(targetPlayerId);
            }
            catch (ObjectDisposedException)
            {
                Unsubscribe(targetPlayerId);
            }
        }
    }
}
