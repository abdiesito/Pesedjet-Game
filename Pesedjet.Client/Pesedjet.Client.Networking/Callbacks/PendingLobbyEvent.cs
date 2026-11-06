using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.Networking.Callbacks;

// A server event that arrived before anybody subscribed; it is replayed in order when the subscriber attaches.
public sealed record PendingLobbyEvent(LobbyMember? Joined, Guid LeftPlayerId);
