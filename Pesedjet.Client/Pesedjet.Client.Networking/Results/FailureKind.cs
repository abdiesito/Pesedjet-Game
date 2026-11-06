namespace Pesedjet.Client.Networking.Results;

public enum FailureKind
{
    // The operation succeeded.
    None,

    // The server understood the request and refused it by a business rule; ErrorCode and MessageKey say which.
    Business,

    // The server could not be reached or the connection broke.
    ServerUnreachable,

    // The server did not answer in time.
    Timeout,

    // Anything else: a technical failure on the server or an unexpected exception on the client.
    Unexpected
}
