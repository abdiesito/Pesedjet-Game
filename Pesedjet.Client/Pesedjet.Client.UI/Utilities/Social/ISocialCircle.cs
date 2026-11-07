using System;
using System.Collections.ObjectModel;
using Pesedjet.Client.UI.Models.Social;

namespace Pesedjet.Client.UI.Utilities.Social;

public interface ISocialCircle
{
    ObservableCollection<FriendItem> Friends { get; }
    ObservableCollection<BlockedPlayerItem> BlockedPlayers { get; }

    void AddFriend(Guid playerId, string username);
}
