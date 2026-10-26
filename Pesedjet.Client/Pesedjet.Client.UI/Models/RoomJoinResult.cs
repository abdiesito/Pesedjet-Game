namespace Pesedjet.Client.UI.Models;

public sealed record RoomJoinResult(
    RoomCodeError Error,
    int Occupancy,
    int Capacity,
    RoomSnapshot? Room = null);
