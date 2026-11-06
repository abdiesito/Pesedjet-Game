using System;

namespace Pesedjet.Client.UI.Models.Social;

public sealed record PlayerSearchCall(Guid RequesterId, string Term, int SearchNumber);
