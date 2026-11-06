using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Social;

namespace Pesedjet.Client.UI.ViewModels.Dependencies;

public sealed record FriendsDependencies(
    INavigator Navigator,
    ISocialCircle SocialCircle,
    FriendListViewModel FriendList,
    PlayerSearchViewModel PlayerSearch,
    BlockedPlayersViewModel BlockedPlayers);
