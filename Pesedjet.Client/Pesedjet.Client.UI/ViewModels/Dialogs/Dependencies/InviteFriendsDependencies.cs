using System;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Toasts;

namespace Pesedjet.Client.UI.ViewModels.Dialogs.Dependencies;

public sealed record InviteFriendsDependencies(
    ILobbyInvitationClient LobbyInvitationClient,
    IPlayerSession PlayerSession,
    IToastManager ToastManager,
    TimeProvider TimeProvider);
