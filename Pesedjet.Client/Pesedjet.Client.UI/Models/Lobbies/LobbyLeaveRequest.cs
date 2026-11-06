using System;

namespace Pesedjet.Client.UI.Models.Lobbies;

public sealed record LobbyLeaveRequest(string LobbyCode, Guid PlayerId);
