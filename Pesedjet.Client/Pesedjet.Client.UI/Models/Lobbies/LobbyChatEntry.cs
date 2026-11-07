using Pesedjet.Client.UI.Models.Common;

namespace Pesedjet.Client.UI.Models.Lobbies;

public sealed record LobbyChatEntry(LobbyChatKind Kind, string Author, string RawText, LocalizedText? Message = null)
{
    public string Text => Message?.Resolve() ?? RawText;

    public bool IsSystem => Kind == LobbyChatKind.System;

    public bool IsOwn => Kind == LobbyChatKind.Own;

    public bool IsOther => Kind == LobbyChatKind.Other;
}
