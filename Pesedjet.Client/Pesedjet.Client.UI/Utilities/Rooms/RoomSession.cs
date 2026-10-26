using Pesedjet.Client.UI.Models;

namespace Pesedjet.Client.UI.Utilities.Rooms;

public sealed class RoomSession : IRoomSession
{
    public RoomSnapshot? Current { get; set; }
}
