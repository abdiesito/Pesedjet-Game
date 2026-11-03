namespace Pesedjet.Client.Networking.Results;

public enum AuthResult
{
    Success,
    InvalidCredentials,
    UserNotFound,
    InvalidData,
    ServerUnreachable,
    Failed
}