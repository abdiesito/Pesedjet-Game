#nullable enable

using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.ViewModels;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.UI.Navigation;

public static class LobbyNavigationExtensions
{
    public static void NavigateToJoinedLobby(
        this INavigator navigator,
        RoomJoinResponse response,
        RoomMember localPlayer)
    {
        navigator.NavigateTo<MatchLobbyViewModel>();

        if (navigator.CurrentViewModel is MatchLobbyViewModel lobbyViewModel)
        {
            lobbyViewModel.Initialize(
                response.RoomCode,
                localPlayer,
                response.HostPlayerId,
                response.MaxPlayers);
            lobbyViewModel.AddExistingMembers(response.ExistingMembers);
        }
    }

    public static void NavigateToCreatedLobby(
        this INavigator navigator,
        string roomCode,
        RoomMember host,
        int maxPlayers)
    {
        navigator.NavigateTo<MatchLobbyViewModel>();

        if (navigator.CurrentViewModel is MatchLobbyViewModel lobbyViewModel)
        {
            lobbyViewModel.Initialize(roomCode, host, host.PlayerId, maxPlayers);
        }
    }
}