namespace Pesedjet.Client.UI.Models.Lobbies;

public sealed record LobbyJoinResult(LobbyCodeError Error, int Occupancy, int Capacity);
