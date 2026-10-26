namespace Pesedjet.Client.Networking.Results;

public enum AccountRegistrationResult
{
    Registered,
    UsernameAlreadyRegistered,
    EmailAlreadyRegistered,
    InvalidData,
    ServerUnreachable,
    Failed
}
