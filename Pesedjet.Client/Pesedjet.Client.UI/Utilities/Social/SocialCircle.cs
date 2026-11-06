using System;
using System.Collections.ObjectModel;
using System.Linq;
using Pesedjet.Client.UI.Models.Social;

namespace Pesedjet.Client.UI.Utilities.Social;

// TODO: Fill the lists from the server (friends ordered online, in a match, then by last connection; blocked players
// by block date) and keep them current through callbacks (CU-007, CU-008, CU-009).
public sealed class SocialCircle : ISocialCircle
{
    public ObservableCollection<FriendItem> Friends { get; } = new();

    public ObservableCollection<BlockedPlayerItem> BlockedPlayers { get; } = new();

    public void AddFriend(Guid playerId, string username)
    {
        bool isAlreadyFriend = Friends.Any(friend => friend.Id == playerId);

        if (!isAlreadyFriend)
        {
            FriendItem newFriend = new FriendItem
            {
                Id = playerId,
                Username = username,
                Presence = PresenceStatus.Offline
            };

            Friends.Add(newFriend);
        }
    }
}
