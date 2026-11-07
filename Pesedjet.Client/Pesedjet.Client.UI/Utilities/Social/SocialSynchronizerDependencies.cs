using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Utilities.Notifications;
using Pesedjet.Client.UI.Utilities.Session;

namespace Pesedjet.Client.UI.Utilities.Social;

public sealed record SocialSynchronizerDependencies(
    IFriendshipClient FriendshipClient,
    IFriendshipNotificationClient NotificationClient,
    IPlayerSession PlayerSession,
    ISocialCircle SocialCircle,
    INotificationInbox Inbox);
