namespace Pesedjet.Client.UI.Models;

public enum LoginStatus
{
    None,
    InvalidCredentials,
    TooManyAttempts,
    ServerUnreachable
}
