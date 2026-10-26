namespace Pesedjet.Client.UI.Models;

public sealed record RoomChatEntry(RoomChatKind Kind, string Author, string RawText, LocalizedText? Message = null)
{
    public string Text => Message?.Resolve() ?? RawText;

    public bool IsSystem => Kind == RoomChatKind.System;

    public bool IsOwn => Kind == RoomChatKind.Own;

    public bool IsOther => Kind == RoomChatKind.Other;
}
