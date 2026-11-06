using Pesedjet.Client.UI.Models.Common;

namespace Pesedjet.Client.UI.Models.DialogParameters;

public sealed record MatchNotice(LocalizedText Title, LocalizedText Message, int? CardValue);
