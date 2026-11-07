using System.Collections.Generic;
using Pesedjet.Client.UI.Models.Matches;

namespace Pesedjet.Client.UI.Models.DialogParameters;

public sealed record ObjectiveRequest(int CardValue, IReadOnlyList<PlayerSnapshot> Opponents);
