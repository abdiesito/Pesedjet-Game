using System;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Matches;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Lobbies;
using Pesedjet.Client.UI.Utilities.Session;

namespace Pesedjet.Client.UI.ViewModels.Dependencies;

public sealed record MatchDependencies(
    INavigator Navigator,
    IDialogManager DialogManager,
    IPlayerSession PlayerSession,
    ILobbySession LobbySession,
    IMatchGateway MatchGateway,
    TopBarViewModel TopBar,
    TimeProvider TimeProvider);
