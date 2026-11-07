using Pesedjet.Client.UI.Models.Lobbies;


namespace Pesedjet.Client.UI.Utilities.Lobbies;

public sealed class LobbySession : ILobbySession
{
    public LobbySnapshot? Current { get; set; }
}
