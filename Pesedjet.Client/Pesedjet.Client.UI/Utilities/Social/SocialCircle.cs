using System.Collections.ObjectModel;
using Pesedjet.Client.UI.Models;

namespace Pesedjet.Client.UI.Utilities.Social;

// TODO: Fill the lists from the server (friends ordered online, in a match, then by last connection; blocked players
// by block date) and keep them current through callbacks (CU-007, CU-008, CU-009).
public sealed class SocialCircle : ISocialCircle
{
    public ObservableCollection<FriendItem> Friends { get; } = new();

    public ObservableCollection<BlockedPlayerItem> BlockedPlayers { get; } = new();
}
