namespace Pesedjet.Server.Contracts.Faults;

public static class ServiceErrorCodes
{
    public const string UsernameAlreadyRegistered = "USERNAME_ALREADY_REGISTERED";
    public const string EmailAlreadyRegistered = "EMAIL_ALREADY_REGISTERED";
    public const string InvalidRegistrationData = "INVALID_REGISTRATION_DATA";
    public const string RoomNotFound = "ROOM_NOT_FOUND";
    public const string RoomFull = "ROOM_FULL";
    public const string RoomAlreadyStarted = "MSG-ROOM-009";
    public const string InvalidRoomCapacity = "INVALID_ROOM_CAPACITY";
}
