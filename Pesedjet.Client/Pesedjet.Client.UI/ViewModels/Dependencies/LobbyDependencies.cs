using System;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Utilities.Clipboard;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Lobbies;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Social;

namespace Pesedjet.Client.UI.ViewModels.Dependencies;

public sealed record LobbyDependencies(
    INavigator Navigator,
    IDialogManager DialogManager,
    IPlayerSession PlayerSession,
    ILobbySession LobbySession,
    IClipboardManager ClipboardManager,
    ISocialCircle SocialCircle,
    TimeProvider TimeProvider,
    ILobbyClient LobbyClient);
