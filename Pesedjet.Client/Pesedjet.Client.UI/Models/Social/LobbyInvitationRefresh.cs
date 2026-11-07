using System;

namespace Pesedjet.Client.UI.Models.Social;

public sealed record LobbyInvitationRefresh(Guid PlayerId, int RefreshNumber);
