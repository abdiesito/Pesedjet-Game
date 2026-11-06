using System;

namespace Pesedjet.Client.UI.Models.Social;

public sealed record SubscriptionChange(Guid PreviousPlayerId, Guid CurrentPlayerId);
