using Pesedjet.Client.UI.Models;

namespace Pesedjet.Client.UI.Utilities.Rooms;

public interface IRoomSession
{
    RoomSnapshot? Current { get; set; }
}
