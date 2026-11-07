using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Lobbies;
using Pesedjet.Client.UI.Utilities.Session;

namespace Pesedjet.Client.UI.ViewModels.Dependencies;

public sealed record GameMenuDependencies(
    INavigator Navigator,
    IDialogManager DialogManager,
    IPlayerSession PlayerSession,
    ILobbyClient LobbyClient,
    ILobbySession LobbySession,
    PublicLobbyListViewModel PublicLobbies);
