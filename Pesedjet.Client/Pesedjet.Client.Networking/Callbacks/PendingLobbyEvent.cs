using Pesedjet.Server.Contracts.DTO;

namespace Pesedjet.Client.Networking.Callbacks;

public sealed record PendingLobbyEvent(LobbyMember? Joined, Guid LeftPlayerId);
