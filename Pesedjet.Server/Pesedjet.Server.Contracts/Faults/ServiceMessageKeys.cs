namespace Pesedjet.Server.Contracts.Faults;

public static class ServiceMessageKeys
{
    public const string UsernameAlreadyRegistered = "RegisterAccount.UsernameAlreadyRegistered";
    public const string EmailAlreadyRegistered = "RegisterAccount.EmailAlreadyRegistered";
    public const string InvalidRegistrationData = "RegisterAccount.ValidationSummary";
    public const string RoomNotFound = "ROOM_NOT_FOUND";
    public const string RoomFull = "ROOM_FULL";    
    public const string RoomAlreadyStarted = "MSG-ROOM-009";
    public const string InvalidRoomCapacity = "MSG-ROOM-011";
    
}
