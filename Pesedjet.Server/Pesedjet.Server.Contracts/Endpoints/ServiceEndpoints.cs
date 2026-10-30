#nullable enable

namespace Pesedjet.Server.Contracts.Endpoints;

public static class ServiceEndpoints
{
    public const int TcpPort = 8081;

    public static readonly string AccountServiceAddress = $"net.tcp://localhost:{TcpPort}/AccountService";

    public static readonly string RoomServiceAddress = $"net.tcp://localhost:{TcpPort}/RoomService";
}