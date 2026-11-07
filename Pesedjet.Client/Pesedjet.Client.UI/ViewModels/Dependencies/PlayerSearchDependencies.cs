using System;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Social;
using Pesedjet.Client.UI.Utilities.Toasts;

namespace Pesedjet.Client.UI.ViewModels.Dependencies;

public sealed record PlayerSearchDependencies(
    IFriendshipClient FriendshipClient,
    IPlayerSession PlayerSession,
    ISocialCircle SocialCircle,
    IToastManager ToastManager,
    TimeProvider TimeProvider);
