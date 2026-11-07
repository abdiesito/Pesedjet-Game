using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Lobbies;
using Pesedjet.Client.UI.Utilities.Session;

namespace Pesedjet.Client.UI.ViewModels.Dependencies;

public sealed record PublicLobbyListDependencies(
    INavigator Navigator,
    ILobbyClient LobbyClient,
    ILobbySession LobbySession,
    IPlayerSession PlayerSession);
