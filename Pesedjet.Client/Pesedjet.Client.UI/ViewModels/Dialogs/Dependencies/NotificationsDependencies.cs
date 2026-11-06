using System;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Utilities.Lobbies;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Notifications;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Social;
using Pesedjet.Client.UI.Utilities.Toasts;

namespace Pesedjet.Client.UI.ViewModels.Dialogs.Dependencies;

public sealed record NotificationsDependencies(
    INotificationInbox Inbox,
    IFriendshipClient FriendshipClient,
    ILobbyInvitationClient LobbyInvitationClient,
    ILobbyClient LobbyClient,
    ILobbySession LobbySession,
    INavigator Navigator,
    IPlayerSession PlayerSession,
    ISocialCircle SocialCircle,
    IToastManager ToastManager,
    TimeProvider TimeProvider);
