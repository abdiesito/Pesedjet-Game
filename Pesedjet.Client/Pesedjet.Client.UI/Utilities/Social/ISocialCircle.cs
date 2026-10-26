using System.Collections.ObjectModel;
using Pesedjet.Client.UI.Models;

namespace Pesedjet.Client.UI.Utilities.Social;

public interface ISocialCircle
{
    ObservableCollection<FriendItem> Friends { get; }
    ObservableCollection<BlockedPlayerItem> BlockedPlayers { get; }
}
