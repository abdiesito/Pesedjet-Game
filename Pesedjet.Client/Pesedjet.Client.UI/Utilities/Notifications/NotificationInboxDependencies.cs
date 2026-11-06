using System;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Utilities.Session;

namespace Pesedjet.Client.UI.Utilities.Notifications;

public sealed record NotificationInboxDependencies(
    IFriendshipClient FriendshipClient,
    ILobbyInvitationClient LobbyInvitationClient,
    IPlayerSession PlayerSession,
    TimeProvider TimeProvider);
