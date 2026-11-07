using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.Networking.Callbacks;

public sealed record PendingLobbyEvent(LobbyMember? Joined, Guid LeftPlayerId);
