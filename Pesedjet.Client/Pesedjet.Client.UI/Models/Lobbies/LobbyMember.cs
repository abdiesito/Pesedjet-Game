using System;

namespace Pesedjet.Client.UI.Models.Lobbies;

public sealed record LobbyMember(Guid PlayerId, string Username, int? AvatarId, bool IsHost);
