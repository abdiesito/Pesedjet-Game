using System.Collections.Generic;

namespace Pesedjet.Client.UI.Models;

public sealed record ObjectiveRequest(int CardValue, IReadOnlyList<PlayerSnapshot> Opponents);
