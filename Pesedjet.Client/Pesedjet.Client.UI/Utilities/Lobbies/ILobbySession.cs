using Pesedjet.Client.UI.Models.Lobbies;


namespace Pesedjet.Client.UI.Utilities.Lobbies;

public interface ILobbySession
{
    LobbySnapshot? Current { get; set; }
}
