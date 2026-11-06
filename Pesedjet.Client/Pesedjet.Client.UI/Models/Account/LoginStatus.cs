namespace Pesedjet.Client.UI.Models.Account;

public enum LoginStatus
{
    None,
    InvalidCredentials,
    TooManyAttempts,
    ServerUnreachable
}
