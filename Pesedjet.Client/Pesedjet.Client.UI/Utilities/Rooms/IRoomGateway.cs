using System.Threading.Tasks;
using Pesedjet.Client.UI.Models;

namespace Pesedjet.Client.UI.Utilities.Rooms;

public interface IRoomGateway
{
    Task<RoomSnapshot> CreateRoomAsync(int capacity, RoomVisibility visibility);

    Task<RoomJoinResult> JoinRoomAsync(string code);
}
