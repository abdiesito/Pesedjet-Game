namespace Pesedjet.Server.Contracts.Faults;

public static class ServiceErrorCodes
{
    public const string UsernameAlreadyRegistered = "USERNAME_ALREADY_REGISTERED";
    public const string EmailAlreadyRegistered = "EMAIL_ALREADY_REGISTERED";
    public const string InvalidRegistrationData = "INVALID_REGISTRATION_DATA";
    public const string LobbyNotFound = "ROOM_NOT_FOUND";
    public const string LobbyFull = "ROOM_FULL";
    public const string LobbyAlreadyStarted = "MSG-ROOM-009";
    public const string InvalidLobbyCapacity = "INVALID_ROOM_CAPACITY";
    public const string InvalidData = "INVALID_DATA";
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string InvalidCode = "INVALID_CODE";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    
}
